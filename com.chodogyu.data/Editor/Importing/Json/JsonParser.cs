using System.Collections.Generic;
using CDG.Core.Results;
using CDG.Data;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// JSON 루트 배열에서 각 Object의 원본 문자열을 분리합니다.
    /// 문자열, 중첩 Object 및 중첩 Array 구조를 고려하여 Object 경계를 판단합니다.
    /// </summary>
    internal static class JsonParser
    {
        /// <summary>
        /// 지정한 JSON 문자열의 루트 배열을 파싱합니다.
        /// 루트는 배열이어야 하며 배열의 각 항목은 JSON Object여야 합니다.
        /// </summary>
        internal static Result<JsonDocument> Parse(string text)
        {
            Result inputValidationResult = DataImportTextValidator.Validate(text);

            if (inputValidationResult.IsFailure)
            {
                return Result<JsonDocument>.Failure(inputValidationResult.Error);
            }

            List<JsonObjectSource> objects = new List<JsonObjectSource>();

            int index = 0;
            int currentLine = 1;

            SkipWhitespace(text, ref index, ref currentLine);

            if (index >= text.Length || text[index] != '[')
            {
                return CreateFailure<JsonDocument>(
                    currentLine,
                    "JSON 루트는 배열로 시작해야 합니다.");
            }

            index++;
            SkipWhitespace(text, ref index, ref currentLine);

            if (index < text.Length && text[index] == ']')
            {
                index++;
                SkipWhitespace(text, ref index, ref currentLine);

                if (index != text.Length)
                {
                    return CreateFailure<JsonDocument>(
                        currentLine,
                        "루트 배열이 끝난 뒤 추가 문자가 존재합니다.");
                }

                return Result<JsonDocument>.Success(new JsonDocument(objects));
            }

            while (index < text.Length)
            {
                if (text[index] != '{')
                {
                    return CreateFailure<JsonDocument>(
                        currentLine,
                        "JSON 루트 배열의 각 항목은 Object여야 합니다.");
                }

                Result<JsonObjectSource> objectResult = ParseObject(
                    text,
                    ref index,
                    ref currentLine);

                if (objectResult.IsFailure)
                {
                    return Result<JsonDocument>.Failure(objectResult.Error);
                }

                objects.Add(objectResult.Value);

                SkipWhitespace(text, ref index, ref currentLine);

                if (index >= text.Length)
                {
                    return CreateFailure<JsonDocument>(
                        currentLine,
                        "JSON 루트 배열이 닫히지 않았습니다.");
                }

                if (text[index] == ']')
                {
                    index++;
                    SkipWhitespace(text, ref index, ref currentLine);

                    if (index != text.Length)
                    {
                        return CreateFailure<JsonDocument>(
                            currentLine,
                            "루트 배열이 끝난 뒤 추가 문자가 존재합니다.");
                    }

                    return Result<JsonDocument>.Success(new JsonDocument(objects));
                }

                if (text[index] != ',')
                {
                    return CreateFailure<JsonDocument>(
                        currentLine,
                        "JSON Object 뒤에는 쉼표 또는 루트 배열의 닫는 대괄호가 와야 합니다.");
                }

                index++;
                SkipWhitespace(text, ref index, ref currentLine);

                if (index >= text.Length)
                {
                    return CreateFailure<JsonDocument>(
                        currentLine,
                        "JSON 루트 배열이 닫히지 않았습니다.");
                }

                if (text[index] == ']')
                {
                    return CreateFailure<JsonDocument>(
                        currentLine,
                        "JSON 루트 배열의 마지막 항목 뒤에는 쉼표를 사용할 수 없습니다.");
                }
            }

            return CreateFailure<JsonDocument>(
                currentLine,
                "JSON 루트 배열이 닫히지 않았습니다.");
        }

        private static Result<JsonObjectSource> ParseObject(string text, ref int index, ref int currentLine)
        {
            int startIndex = index;
            int startLine = currentLine;

            Stack<char> expectedClosings = new Stack<char>();
            bool inString = false;

            while (index < text.Length)
            {
                char character = text[index];

                if (inString)
                {
                    if (character == '\\')
                    {
                        Result escapeResult = ConsumeEscape(text, ref index);

                        if (escapeResult.IsFailure)
                        {
                            return Result<JsonObjectSource>.Failure(new ResultError(
                                DataErrorCodes.ImportFailed,
                                $"JSON 파싱에 실패했습니다. {currentLine}행: {escapeResult.Error.Message}"));
                        }

                        continue;
                    }

                    if (character == '"')
                    {
                        inString = false;
                        index++;
                        continue;
                    }

                    if (character == '\r' || character == '\n')
                    {
                        return CreateFailure<JsonObjectSource>(
                            currentLine,
                            "JSON 문자열 내부에는 이스케이프되지 않은 줄바꿈을 사용할 수 없습니다.");
                    }

                    index++;
                    continue;
                }

                if (character == '"')
                {
                    inString = true;
                    index++;
                    continue;
                }

                if (character == '{')
                {
                    expectedClosings.Push('}');
                    index++;
                    continue;
                }

                if (character == '[')
                {
                    expectedClosings.Push(']');
                    index++;
                    continue;
                }

                if (character == '}' || character == ']')
                {
                    if (expectedClosings.Count == 0 || expectedClosings.Peek() != character)
                    {
                        return CreateFailure<JsonObjectSource>(
                            currentLine,
                            "JSON Object 내부의 괄호 구조가 올바르지 않습니다.");
                    }

                    expectedClosings.Pop();
                    index++;

                    if (expectedClosings.Count == 0)
                    {
                        string objectText = text.Substring(
                            startIndex,
                            index - startIndex);

                        return Result<JsonObjectSource>.Success(
                            new JsonObjectSource(objectText, startLine));
                    }

                    continue;
                }

                if (character == '\r' || character == '\n')
                {
                    ConsumeLineBreak(
                        text,
                        ref index,
                        ref currentLine);

                    continue;
                }

                index++;
            }

            if (inString)
            {
                return CreateFailure<JsonObjectSource>(
                    currentLine,
                    "JSON 문자열이 닫히지 않았습니다.");
            }

            return CreateFailure<JsonObjectSource>(
                startLine,
                "JSON Object가 닫히지 않았습니다.");
        }

        private static Result ConsumeEscape(string text, ref int index)
        {
            if (index + 1 >= text.Length)
            {
                return Result.Failure(new ResultError(
                    DataErrorCodes.ImportFailed,
                    "JSON 문자열의 이스케이프 문자가 완성되지 않았습니다."));
            }

            char escape = text[index + 1];

            if (escape == 'u')
            {
                if (index + 5 >= text.Length)
                {
                    return Result.Failure(new ResultError(
                        DataErrorCodes.ImportFailed,
                        "JSON Unicode 이스케이프는 4개의 16진수 문자를 포함해야 합니다."));
                }

                for (int offset = 2; offset <= 5; offset++)
                {
                    if (!IsHexDigit(text[index + offset]))
                    {
                        return Result.Failure(new ResultError(
                            DataErrorCodes.ImportFailed,
                            "JSON Unicode 이스케이프에 올바르지 않은 문자가 포함되어 있습니다."));
                    }
                }

                index += 6;
                return Result.Success();
            }

            if (escape != '"' &&
                escape != '\\' &&
                escape != '/' &&
                escape != 'b' &&
                escape != 'f' &&
                escape != 'n' &&
                escape != 'r' &&
                escape != 't')
            {
                return Result.Failure(new ResultError(
                    DataErrorCodes.ImportFailed,
                    $"지원되지 않는 JSON 이스케이프 문자입니다: '\\{escape}'"));
            }

            index += 2;
            return Result.Success();
        }

        private static void SkipWhitespace(string text, ref int index, ref int currentLine)
        {
            while (index < text.Length)
            {
                char character = text[index];

                if (character == ' ' || character == '\t')
                {
                    index++;
                    continue;
                }

                if (character == '\r' || character == '\n')
                {
                    ConsumeLineBreak(
                        text,
                        ref index,
                        ref currentLine);

                    continue;
                }

                break;
            }
        }

        private static void ConsumeLineBreak(string text, ref int index, ref int currentLine)
        {
            if (text[index] == '\r' &&
                index + 1 < text.Length &&
                text[index + 1] == '\n')
            {
                index += 2;
            }
            else
            {
                index++;
            }

            currentLine++;
        }

        private static bool IsHexDigit(char character)
        {
            return (character >= '0' && character <= '9') ||
                   (character >= 'a' && character <= 'f') ||
                   (character >= 'A' && character <= 'F');
        }

        private static Result<T> CreateFailure<T>(int line, string message)
        {
            return Result<T>.Failure(new ResultError(
                DataErrorCodes.ImportFailed,
                $"JSON 파싱에 실패했습니다. {line}행: {message}"));
        }
    }
}