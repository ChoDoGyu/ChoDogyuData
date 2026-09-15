using System.Collections.Generic;
using System.Text;
using CDG.Core.Results;
using CDG.Data;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// 쉼표로 구분된 CSV 문자열을 읽기 전용 CSV 문서로 변환합니다.
    /// 따옴표 필드, 이스케이프된 따옴표 및 따옴표 내부 줄바꿈을 지원합니다.
    /// </summary>
    internal static class CsvParser
    {
        /// <summary>
        /// 지정한 CSV 문자열을 파싱합니다.
        /// 잘못된 CSV 문법이 발견되면 <see cref="DataErrorCodes.ImportFailed"/> 오류를 반환합니다.
        /// </summary>
        internal static Result<CsvDocument> Parse(string text)
        {
            Result inputValidationResult = DataImportTextValidator.Validate(text);

            if (inputValidationResult.IsFailure)
            {
                return Result<CsvDocument>.Failure(inputValidationResult.Error);
            }

            List<CsvRow> rows = new List<CsvRow>();
            List<string> fields = new List<string>();
            StringBuilder fieldBuilder = new StringBuilder();

            bool inQuotes = false;
            bool closedQuotedField = false;
            bool rowOpen = false;

            int currentLine = 1;
            int rowStartLine = 1;

            for (int index = 0; index < text.Length; index++)
            {
                char character = text[index];

                if (inQuotes)
                {
                    if (character == '"')
                    {
                        if (index + 1 < text.Length && text[index + 1] == '"')
                        {
                            fieldBuilder.Append('"');
                            index++;
                            continue;
                        }

                        inQuotes = false;
                        closedQuotedField = true;
                        continue;
                    }

                    if (character == '\r')
                    {
                        fieldBuilder.Append('\r');

                        if (index + 1 < text.Length && text[index + 1] == '\n')
                        {
                            fieldBuilder.Append('\n');
                            index++;
                        }

                        currentLine++;
                        continue;
                    }

                    if (character == '\n')
                    {
                        fieldBuilder.Append('\n');
                        currentLine++;
                        continue;
                    }

                    fieldBuilder.Append(character);
                    continue;
                }

                if (closedQuotedField)
                {
                    if (character == ',')
                    {
                        fields.Add(fieldBuilder.ToString());
                        fieldBuilder.Clear();

                        closedQuotedField = false;
                        rowOpen = true;
                        continue;
                    }

                    if (character == '\r' || character == '\n')
                    {
                        AddRow(rows, fields, fieldBuilder, rowStartLine);

                        closedQuotedField = false;
                        rowOpen = false;

                        ConsumeRowSeparator(text, ref index, character);
                        currentLine++;
                        rowStartLine = currentLine;
                        continue;
                    }

                    return CreateFailure<CsvDocument>(
                        currentLine,
                        "닫는 따옴표 뒤에는 쉼표, 줄바꿈 또는 문자열의 끝만 올 수 있습니다.");
                }

                if (character == '"')
                {
                    if (fieldBuilder.Length != 0)
                    {
                        return CreateFailure<CsvDocument>(
                            currentLine,
                            "따옴표 필드는 필드의 첫 문자부터 시작해야 합니다.");
                    }

                    inQuotes = true;
                    rowOpen = true;
                    continue;
                }

                if (character == ',')
                {
                    fields.Add(fieldBuilder.ToString());
                    fieldBuilder.Clear();

                    rowOpen = true;
                    continue;
                }

                if (character == '\r' || character == '\n')
                {
                    AddRow(rows, fields, fieldBuilder, rowStartLine);

                    rowOpen = false;

                    ConsumeRowSeparator(text, ref index, character);
                    currentLine++;
                    rowStartLine = currentLine;
                    continue;
                }

                fieldBuilder.Append(character);
                rowOpen = true;
            }

            if (inQuotes)
            {
                return CreateFailure<CsvDocument>(
                    currentLine,
                    "따옴표 필드가 닫히지 않은 상태에서 CSV 문자열이 끝났습니다.");
            }

            if (rowOpen || fields.Count > 0 || fieldBuilder.Length > 0 || closedQuotedField)
            {
                AddRow(rows, fields, fieldBuilder, rowStartLine);
            }

            return Result<CsvDocument>.Success(new CsvDocument(rows));
        }

        private static void AddRow(List<CsvRow> rows, List<string> fields, StringBuilder fieldBuilder, int startLine)
        {
            fields.Add(fieldBuilder.ToString());
            fieldBuilder.Clear();

            rows.Add(new CsvRow(fields, startLine));
            fields.Clear();
        }

        private static void ConsumeRowSeparator(string text, ref int index, char character)
        {
            if (character == '\r' && index + 1 < text.Length && text[index + 1] == '\n')
            {
                index++;
            }
        }

        private static Result<T> CreateFailure<T>(int line, string message)
        {
            return Result<T>.Failure(new ResultError(
                DataErrorCodes.ImportFailed,
                $"CSV 파싱에 실패했습니다. {line}행: {message}"));
        }
    }
}