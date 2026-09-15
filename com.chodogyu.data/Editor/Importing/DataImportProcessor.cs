using System;
using System.Collections.Generic;
using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Validation;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// 외부 문자열 입력부터 후보 데이터 생성, 검증 및 현재 데이터와의 Diff 계산까지 이어지는
    /// 공통 가져오기 흐름을 처리합니다.
    /// 실제 대상 Asset의 데이터는 수정하지 않습니다.
    /// </summary>
    internal static class DataImportProcessor
    {
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

        internal static Result<DataImportPreview<T>> Import<T>(string text, IDataTextImporter<T> importer, IReadOnlyList<T> currentEntries, IDataImportEntryComparer<T> comparer) where T : IDataEntry
        {
            if (importer == null)
            {
                throw new ArgumentNullException(nameof(importer));
            }

            if (currentEntries == null)
            {
                throw new ArgumentNullException(nameof(currentEntries));
            }

            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            Result<DataImportPreview<T>> importResult = Import(text, importer);

            if (importResult.IsFailure)
            {
                return importResult;
            }

            DataImportPreview<T> preview = importResult.Value;

            if (!preview.IsValid)
            {
                return importResult;
            }

            Result<DataImportDiff<T>> diffResult = DataImportDiffBuilder.Build(
                currentEntries,
                preview.Candidate,
                comparer);

            if (diffResult.IsFailure)
            {
                return Result<DataImportPreview<T>>.Failure(diffResult.Error);
            }

            return Result<DataImportPreview<T>>.Success(new DataImportPreview<T>(
                preview.Candidate,
                preview.ValidationReport,
                diffResult.Value));
        }

        /// <summary>
        /// 외부 데이터를 가져와 검증하고 지정한 대상 Asset과 비교하여
        /// Apply 가능한 상태 스냅샷이 포함된 Preview를 생성합니다.
        /// </summary>
        internal static Result<DataImportPreview<T>> Import<T>(string text, IDataTextImporter<T> importer, DataTableAsset<T> target, IDataImportEntryComparer<T> comparer) where T : IDataEntry
        {
            if (importer == null)
            {
                throw new ArgumentNullException(nameof(importer));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            Result<DataImportPreview<T>> importResult = Import(text, importer);

            if (importResult.IsFailure)
            {
                return importResult;
            }

            DataImportPreview<T> preview = importResult.Value;

            if (!preview.IsValid)
            {
                return importResult;
            }

            DataImportStateSnapshot<T> beforeSnapshot = DataImportStateSnapshot<T>.Capture(
                target,
                preview.Candidate);

            Result<DataImportDiff<T>> diffResult = DataImportDiffBuilder.Build(
                target.Entries,
                preview.Candidate,
                comparer);

            if (diffResult.IsFailure)
            {
                return Result<DataImportPreview<T>>.Failure(diffResult.Error);
            }

            DataImportStateSnapshot<T> afterSnapshot = DataImportStateSnapshot<T>.Capture(
                target,
                preview.Candidate);

            if (!beforeSnapshot.HasSameState(afterSnapshot))
            {
                return Result<DataImportPreview<T>>.Failure(new ResultError(
                    DataErrorCodes.ImportFailed,
                    "Diff 계산 중 대상 Asset 또는 가져오기 후보의 상태가 변경되었습니다."));
            }

            return Result<DataImportPreview<T>>.Success(new DataImportPreview<T>(
                preview.Candidate,
                preview.ValidationReport,
                diffResult.Value,
                afterSnapshot));
        }
    }
}