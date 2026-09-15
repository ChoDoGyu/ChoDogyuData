using System;
using System.Collections.Generic;
using CDG.Data;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// 기존 데이터와 가져오기 후보 전체를 비교한 읽기 전용 Diff 결과입니다.
    /// 항목별 변경 상태와 상태별 개수를 제공합니다.
    /// </summary>
    /// <typeparam name="T">비교된 데이터 항목 타입입니다.</typeparam>
    internal sealed class DataImportDiff<T> where T : IDataEntry
    {
        private readonly DataImportDiffItem<T>[] items;
        private readonly IReadOnlyList<DataImportDiffItem<T>> readOnlyItems;

        /// <summary>
        /// 모든 Diff 항목을 비교 결과 순서대로 제공합니다.
        /// </summary>
        internal IReadOnlyList<DataImportDiffItem<T>> Items => readOnlyItems;

        /// <summary>
        /// 전체 Diff 항목 수입니다.
        /// </summary>
        internal int Count => items.Length;

        /// <summary>
        /// 새로 추가되는 항목 수입니다.
        /// </summary>
        internal int AddedCount { get; }

        /// <summary>
        /// 삭제되는 항목 수입니다.
        /// </summary>
        internal int RemovedCount { get; }

        /// <summary>
        /// 내용이 변경되는 항목 수입니다.
        /// </summary>
        internal int ModifiedCount { get; }

        /// <summary>
        /// 기존 데이터와 동일한 항목 수입니다.
        /// </summary>
        internal int UnchangedCount { get; }

        /// <summary>
        /// 실제 데이터 변경이 하나 이상 존재하는지 나타냅니다.
        /// </summary>
        internal bool HasChanges => AddedCount > 0 || RemovedCount > 0 || ModifiedCount > 0;

        internal DataImportDiff(IEnumerable<DataImportDiffItem<T>> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            List<DataImportDiffItem<T>> snapshot = new List<DataImportDiffItem<T>>(items);

            int addedCount = 0;
            int removedCount = 0;
            int modifiedCount = 0;
            int unchangedCount = 0;

            for (int index = 0; index < snapshot.Count; index++)
            {
                DataImportDiffItem<T> item = snapshot[index];

                if (item == null)
                {
                    throw new ArgumentException(
                        "Diff 결과에는 null 항목을 포함할 수 없습니다.",
                        nameof(items));
                }

                switch (item.Type)
                {
                    case DataImportDiffType.Added:
                        addedCount++;
                        break;

                    case DataImportDiffType.Removed:
                        removedCount++;
                        break;

                    case DataImportDiffType.Modified:
                        modifiedCount++;
                        break;

                    case DataImportDiffType.Unchanged:
                        unchangedCount++;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(items),
                            $"지원하지 않는 Diff 상태입니다: {item.Type}");
                }
            }

            this.items = snapshot.ToArray();
            readOnlyItems = Array.AsReadOnly(this.items);

            AddedCount = addedCount;
            RemovedCount = removedCount;
            ModifiedCount = modifiedCount;
            UnchangedCount = unchangedCount;
        }
    }
}