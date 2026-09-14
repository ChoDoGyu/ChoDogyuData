using System;
using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Validation;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// 외부 문자열 입력부터 파싱된 후보 데이터의 검증까지 이어지는 공통 가져오기 흐름을 처리합니다.
    /// 실제 대상 Asset의 데이터는 수정하지 않습니다.
    /// </summary>
    internal static class DataImportProcessor
    {
        /// <summary>
        /// 지정한 문자열을 검사하고 Importer를 통해 후보 데이터를 생성한 뒤 데이터 검증까지 수행합니다.
        /// 문자열 또는 파싱 자체의 실패는 실패 Result로 반환하며,
        /// 파싱에 성공한 데이터의 검증 문제는 <see cref="DataImportPreview{T}"/>에 보존합니다.
        /// </summary>
        internal static Result<DataImportPreview<T>> Import<T>(string text, IDataTextImporter<T> importer) where T : IDataEntry
        {
            if (importer == null)
            {
                throw new ArgumentNullException(nameof(importer));
            }

            Result textValidationResult = DataImportTextValidator.Validate(text);

            if (textValidationResult.IsFailure)
            {
                return Result<DataImportPreview<T>>.Failure(textValidationResult.Error);
            }

            Result<DataImportCandidate<T>> importResult = importer.Import(text);

            if (importResult.IsFailure)
            {
                return Result<DataImportPreview<T>>.Failure(importResult.Error);
            }

            DataImportCandidate<T> candidate = importResult.Value;

            if (candidate == null)
            {
                return Result<DataImportPreview<T>>.Failure(new ResultError(
                    DataErrorCodes.ImportFailed,
                    "Importer가 유효한 데이터 후보를 반환하지 않았습니다."));
            }

            DataValidationReport validationReport = DataImportCandidateValidator.Validate(candidate);
            DataImportPreview<T> preview = new DataImportPreview<T>(candidate, validationReport);

            return Result<DataImportPreview<T>>.Success(preview);
        }
    }
}