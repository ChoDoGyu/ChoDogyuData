using System;
using System.Collections.Generic;
using CDG.Core.Results;
using CDG.Data;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// JSON 루트 배열을 파싱하고 각 JSON Object를 지정한 데이터 타입으로 변환합니다.
    /// 가져오기 과정에서는 대상 DataTableAsset을 직접 수정하지 않습니다.
    /// </summary>
    /// <typeparam name="T">JSON에서 생성할 데이터 항목 타입입니다.</typeparam>
    internal sealed class JsonDataImporter<T> : IDataTextImporter<T> where T : IDataEntry
    {
        private readonly IJsonObjectMapper<T> objectMapper;

        /// <summary>
        /// 지정한 Object Mapper를 사용하는 JSON Importer를 생성합니다.
        /// </summary>
        /// <param name="objectMapper">JSON Object를 데이터 항목으로 변환할 Mapper입니다.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="objectMapper"/>가 null인 경우 발생합니다.
        /// </exception>
        internal JsonDataImporter(IJsonObjectMapper<T> objectMapper)
        {
            this.objectMapper = objectMapper ?? throw new ArgumentNullException(nameof(objectMapper));
        }

        /// <summary>
        /// JSON 문자열을 파싱하고 루트 배열의 각 Object를 데이터 항목으로 변환합니다.
        /// JSON 구조 또는 Object 변환에 문제가 있으면 가져오기 실패 결과를 반환합니다.
        /// </summary>
        public Result<DataImportCandidate<T>> Import(string text)
        {
            Result<JsonDocument> parseResult = JsonParser.Parse(text);

            if (parseResult.IsFailure)
            {
                return Result<DataImportCandidate<T>>.Failure(parseResult.Error);
            }

            JsonDocument document = parseResult.Value;
            List<T> entries = new List<T>(document.Count);

            for (int index = 0; index < document.Count; index++)
            {
                JsonObjectSource source = document[index];
                Result<T> mapResult = objectMapper.Map(source);

                if (mapResult == null)
                {
                    return CreateFailure(
                        $"JSON {source.StartLine}행 Object 변환 결과가 null입니다.");
                }

                if (mapResult.IsFailure)
                {
                    return CreateFailure(
                        $"JSON {source.StartLine}행 Object 변환에 실패했습니다. {mapResult.Error.Message}");
                }

                entries.Add(mapResult.Value);
            }

            return Result<DataImportCandidate<T>>.Success(
                new DataImportCandidate<T>(entries));
        }

        private static Result<DataImportCandidate<T>> CreateFailure(string message)
        {
            return Result<DataImportCandidate<T>>.Failure(new ResultError(
                DataErrorCodes.ImportFailed,
                message));
        }
    }
}