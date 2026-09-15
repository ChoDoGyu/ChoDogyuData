using System;
using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Validation;
using UnityEditor;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// 검증과 Diff 계산이 완료된 가져오기 Preview를 실제 DataTableAsset에 적용합니다.
    /// Preview 생성 이후 대상 Asset 또는 후보 데이터가 변경된 경우 적용을 거부합니다.
    /// </summary>
    internal static class DataImportApplier
    {
        private const string UndoName = "Apply Imported Data";

        internal static Result Apply<T>(DataTableAsset<T> target, DataImportPreview<T> preview) where T : IDataEntry
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (preview == null)
            {
                throw new ArgumentNullException(nameof(preview));
            }

            if (!preview.IsValid)
            {
                return Result.Failure(new ResultError(
                    DataErrorCodes.ValidationFailed,
                    "유효하지 않은 가져오기 Preview는 Asset에 적용할 수 없습니다."));
            }

            if (!preview.HasDiff)
            {
                return Result.Failure(new ResultError(
                    DataErrorCodes.ImportFailed,
                    "Diff가 계산되지 않은 가져오기 Preview는 Asset에 적용할 수 없습니다."));
            }

            DataValidationReport latestValidation = DataImportCandidateValidator.Validate(preview.Candidate);

            if (!latestValidation.IsValid)
            {
                return Result.Failure(new ResultError(
                    DataErrorCodes.ValidationFailed,
                    $"가져오기 후보가 Preview 생성 이후 유효하지 않은 상태로 변경되었습니다. 발견된 문제 수: {latestValidation.Count}"));
            }

            if (!preview.HasStateSnapshot)
            {
                return Result.Failure(new ResultError(
                    DataErrorCodes.ImportFailed,
                    "대상 Asset 상태 스냅샷이 없는 Preview는 적용할 수 없습니다."));
            }

            if (!preview.StateSnapshot.Matches(target, preview.Candidate))
            {
                return Result.Failure(new ResultError(
                    DataErrorCodes.ImportFailed,
                    "Preview 생성 이후 대상 Asset 또는 가져오기 후보가 변경되었습니다. Preview를 다시 생성해야 합니다."));
            }

            Undo.RegisterCompleteObjectUndo(target, UndoName);

            Result replaceResult = target.ReplaceEntries(preview.Candidate.Entries);

            if (replaceResult.IsFailure)
            {
                return replaceResult;
            }

            EditorUtility.SetDirty(target);

            return Result.Success();
        }
    }
}