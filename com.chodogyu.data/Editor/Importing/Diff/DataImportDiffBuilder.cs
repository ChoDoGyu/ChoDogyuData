using System;
using System.Collections.Generic;
using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Validation;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// 현재 데이터와 가져오기 후보를 ID 기준으로 비교하여 항목별 Diff 결과를 생성합니다.
    /// 가져오기 후보의 순서를 우선 유지하고, 삭제되는 기존 항목은 결과의 뒤에 기존 순서대로 추가합니다.
    /// </summary>
    internal static class DataImportDiffBuilder
    {
        /// <summary>
        /// 현재 데이터와 가져오기 후보를 비교하여 추가, 삭제, 변경 및 동일 상태를 계산합니다.
        /// 양쪽 데이터는 Diff 계산 전에 Data Framework의 공통 데이터 규칙을 만족해야 합니다.
        /// </summary>
        /// <typeparam name="T">비교할 데이터 항목 타입입니다.</typeparam>
        /// <param name="currentEntries">현재 Asset에 저장되어 있는 데이터 항목입니다.</param>
        /// <param name="candidate">가져오기 과정에서 생성된 데이터 후보입니다.</param>
        /// <param name="comparer">동일한 ID를 가진 두 항목의 실제 내용을 비교할 Comparer입니다.</param>
        /// <returns>계산된 Diff 결과 또는 검증 실패 정보를 포함하는 결과입니다.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="currentEntries"/>, <paramref name="candidate"/> 또는
        /// <paramref name="comparer"/>가 null인 경우 발생합니다.
        /// </exception>
        internal static Result<DataImportDiff<T>> Build<T>(
            IReadOnlyList<T> currentEntries,
            DataImportCandidate<T> candidate,
            IDataImportEntryComparer<T> comparer) where T : IDataEntry
        {
            if (currentEntries == null)
            {
                throw new ArgumentNullException(nameof(currentEntries));
            }

            if (candidate == null)
            {
                throw new ArgumentNullException(nameof(candidate));
            }

            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            DataValidationReport currentValidation =
                DataTableValidator.Validate(currentEntries);

            if (!currentValidation.IsValid)
            {
                return Result<DataImportDiff<T>>.Failure(new ResultError(
                    DataErrorCodes.ValidationFailed,
                    $"현재 데이터 검증에 실패하여 Diff를 생성할 수 없습니다. 발견된 문제 수: {currentValidation.Count}"));
            }

            DataValidationReport candidateValidation =
                DataImportCandidateValidator.Validate(candidate);

            if (!candidateValidation.IsValid)
            {
                return Result<DataImportDiff<T>>.Failure(new ResultError(
                    DataErrorCodes.ValidationFailed,
                    $"가져오기 후보 검증에 실패하여 Diff를 생성할 수 없습니다. 발견된 문제 수: {candidateValidation.Count}"));
            }

            Dictionary<string, T> currentById =
                new Dictionary<string, T>(StringComparer.Ordinal);

            for (int index = 0; index < currentEntries.Count; index++)
            {
                T currentEntry = currentEntries[index];
                currentById.Add(currentEntry.Id, currentEntry);
            }

            HashSet<string> incomingIds =
                new HashSet<string>(StringComparer.Ordinal);

            List<DataImportDiffItem<T>> items =
                new List<DataImportDiffItem<T>>(
                    currentEntries.Count + candidate.Count);

            for (int index = 0; index < candidate.Count; index++)
            {
                T incomingEntry = candidate.Entries[index];

                incomingIds.Add(incomingEntry.Id);

                if (!currentById.TryGetValue(
                    incomingEntry.Id,
                    out T currentEntry))
                {
                    items.Add(new DataImportDiffItem<T>(
                        incomingEntry.Id,
                        DataImportDiffType.Added,
                        default,
                        incomingEntry));

                    continue;
                }

                bool isEquivalent =
                    comparer.AreEquivalent(currentEntry, incomingEntry);

                items.Add(new DataImportDiffItem<T>(
                    incomingEntry.Id,
                    isEquivalent
                        ? DataImportDiffType.Unchanged
                        : DataImportDiffType.Modified,
                    currentEntry,
                    incomingEntry));
            }

            for (int index = 0; index < currentEntries.Count; index++)
            {
                T currentEntry = currentEntries[index];

                if (incomingIds.Contains(currentEntry.Id))
                {
                    continue;
                }

                items.Add(new DataImportDiffItem<T>(
                    currentEntry.Id,
                    DataImportDiffType.Removed,
                    currentEntry,
                    default));
            }

            return Result<DataImportDiff<T>>.Success(
                new DataImportDiff<T>(items));
        }
    }
}