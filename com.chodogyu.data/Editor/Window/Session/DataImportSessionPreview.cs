using System;
using System.Collections.Generic;
using CDG.Data;
using CDG.Data.Editor.Importing;
using CDG.Data.Validation;

namespace CDG.Data.Editor
{
    /// <summary>
    /// Generic Entry 타입을 노출하지 않고 EditorWindow에서 사용할 수 있도록 변환한 Import Preview입니다.
    /// Validation 결과와 Diff 요약 정보를 제공합니다.
    /// </summary>
    internal sealed class DataImportSessionPreview
    {
        private readonly DataImportSessionDiffItem[] diffItems;
        private readonly IReadOnlyList<DataImportSessionDiffItem> readOnlyDiffItems;

        /// <summary>
        /// 가져오기 후보에 포함된 데이터 항목 수입니다.
        /// </summary>
        internal int CandidateCount { get; }

        /// <summary>
        /// 후보 데이터의 Validation 결과입니다.
        /// </summary>
        internal DataValidationReport ValidationReport { get; }

        /// <summary>
        /// 후보 데이터가 유효한지 나타냅니다.
        /// </summary>
        internal bool IsValid => ValidationReport.IsValid;

        /// <summary>
        /// 대상 Asset과의 Diff가 계산되었는지를 나타냅니다.
        /// </summary>
        internal bool HasDiff { get; }

        /// <summary>
        /// Apply 시 상태 변경 여부를 확인할 Snapshot이 존재하는지를 나타냅니다.
        /// </summary>
        internal bool HasStateSnapshot { get; }

        /// <summary>
        /// 현재 Preview가 Apply 가능한 상태인지 나타냅니다.
        /// </summary>
        internal bool CanApply => IsValid && HasDiff && HasStateSnapshot;

        /// <summary>
        /// 추가되는 데이터 항목 수입니다.
        /// </summary>
        internal int AddedCount { get; }

        /// <summary>
        /// 제거되는 데이터 항목 수입니다.
        /// </summary>
        internal int RemovedCount { get; }

        /// <summary>
        /// 변경되는 데이터 항목 수입니다.
        /// </summary>
        internal int ModifiedCount { get; }

        /// <summary>
        /// 변경되지 않은 데이터 항목 수입니다.
        /// </summary>
        internal int UnchangedCount { get; }

        /// <summary>
        /// 실제 데이터 변경이 존재하는지를 나타냅니다.
        /// </summary>
        internal bool HasChanges => AddedCount > 0 || RemovedCount > 0 || ModifiedCount > 0;

        /// <summary>
        /// EditorWindow에서 표시할 Diff 항목입니다.
        /// </summary>
        internal IReadOnlyList<DataImportSessionDiffItem> DiffItems => readOnlyDiffItems;

        private DataImportSessionPreview(int candidateCount, DataValidationReport validationReport, bool hasDiff, bool hasStateSnapshot, int addedCount, int removedCount, int modifiedCount, int unchangedCount, IEnumerable<DataImportSessionDiffItem> diffItems)
        {
            if (validationReport == null)
            {
                throw new ArgumentNullException(nameof(validationReport));
            }

            if (diffItems == null)
            {
                throw new ArgumentNullException(nameof(diffItems));
            }

            CandidateCount = candidateCount;
            ValidationReport = validationReport;
            HasDiff = hasDiff;
            HasStateSnapshot = hasStateSnapshot;
            AddedCount = addedCount;
            RemovedCount = removedCount;
            ModifiedCount = modifiedCount;
            UnchangedCount = unchangedCount;

            List<DataImportSessionDiffItem> snapshot =
                new List<DataImportSessionDiffItem>(diffItems);

            this.diffItems = snapshot.ToArray();
            readOnlyDiffItems = Array.AsReadOnly(this.diffItems);
        }

        /// <summary>
        /// Generic Import Preview를 EditorWindow에서 사용할 수 있는 비제네릭 Preview로 변환합니다.
        /// </summary>
        internal static DataImportSessionPreview Create<T>(DataImportPreview<T> preview) where T : IDataEntry
        {
            if (preview == null)
            {
                throw new ArgumentNullException(nameof(preview));
            }

            List<DataImportSessionDiffItem> items =
                new List<DataImportSessionDiffItem>();

            int addedCount = 0;
            int removedCount = 0;
            int modifiedCount = 0;
            int unchangedCount = 0;

            if (preview.Diff != null)
            {
                addedCount = preview.Diff.AddedCount;
                removedCount = preview.Diff.RemovedCount;
                modifiedCount = preview.Diff.ModifiedCount;
                unchangedCount = preview.Diff.UnchangedCount;

                for (int index = 0; index < preview.Diff.Count; index++)
                {
                    DataImportDiffItem<T> item =
                        preview.Diff.Items[index];

                    items.Add(new DataImportSessionDiffItem(
                        item.Id,
                        item.Type));
                }
            }

            return new DataImportSessionPreview(
                preview.Candidate.Count,
                preview.ValidationReport,
                preview.HasDiff,
                preview.HasStateSnapshot,
                addedCount,
                removedCount,
                modifiedCount,
                unchangedCount,
                items);
        }
    }
}