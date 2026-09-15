using System;
using System.Collections.Generic;
using CDG.Core.Results;
using CDG.Data;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// CSV 문자열을 파싱하고 각 데이터 행을 지정한 데이터 타입으로 변환합니다.
    /// 가져오기 과정에서는 대상 DataTableAsset을 직접 수정하지 않습니다.
    /// </summary>
    /// <typeparam name="T">CSV에서 생성할 데이터 항목 타입입니다.</typeparam>
    internal sealed class CsvDataImporter<T> : IDataTextImporter<T> where T : IDataEntry
    {
        private readonly ICsvRowMapper<T> rowMapper;

        /// <summary>
        /// 지정한 행 매퍼를 사용하는 CSV Importer를 생성합니다.
        /// </summary>
        /// <param name="rowMapper">CSV 데이터 행을 데이터 항목으로 변환할 매퍼입니다.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="rowMapper"/>가 null인 경우 발생합니다.
        /// </exception>
        internal CsvDataImporter(ICsvRowMapper<T> rowMapper)
        {
            this.rowMapper = rowMapper ?? throw new ArgumentNullException(nameof(rowMapper));
        }

        /// <summary>
        /// CSV 문자열을 파싱하고 헤더를 제외한 각 행을 데이터 항목으로 변환합니다.
        /// CSV 문법, 헤더 또는 행 변환에 문제가 있으면 가져오기 실패 결과를 반환합니다.
        /// </summary>
        public Result<DataImportCandidate<T>> Import(string text)
        {
            Result<CsvDocument> parseResult = CsvParser.Parse(text);

            if (parseResult.IsFailure)
            {
                return Result<DataImportCandidate<T>>.Failure(parseResult.Error);
            }

            CsvDocument document = parseResult.Value;

            if (document.Count == 0)
            {
                return CreateFailure("CSV 문서에 헤더 행이 없습니다.");
            }

            Result<CsvHeader> headerResult = CsvHeader.Create(document[0]);

            if (headerResult.IsFailure)
            {
                return Result<DataImportCandidate<T>>.Failure(headerResult.Error);
            }

            CsvHeader header = headerResult.Value;

            if (rowMapper is ICsvHeaderValidator headerValidator)
            {
                Result headerValidationResult = headerValidator.ValidateHeader(header);

                if (headerValidationResult.IsFailure)
                {
                    return Result<DataImportCandidate<T>>.Failure(
                        headerValidationResult.Error);
                }
            }

            List<T> entries = new List<T>(document.Count - 1);

            for (int index = 1; index < document.Count; index++)
            {
                CsvRow row = document[index];

                if (row.Count != header.Count)
                {
                    return CreateFailure(
                        $"CSV {row.StartLine}행의 열 수가 헤더와 일치하지 않습니다. 헤더: {header.Count}, 데이터 행: {row.Count}");
                }

                Result<T> mapResult = rowMapper.Map(header, row);

                if (mapResult == null)
                {
                    return CreateFailure(
                        $"CSV {row.StartLine}행 변환 결과가 null입니다.");
                }

                if (mapResult.IsFailure)
                {
                    return CreateFailure(
                        $"CSV {row.StartLine}행 데이터 변환에 실패했습니다. {mapResult.Error.Message}");
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