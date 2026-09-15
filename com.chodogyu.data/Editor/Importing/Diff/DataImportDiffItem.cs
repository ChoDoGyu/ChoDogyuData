using System;
using CDG.Data;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// 기존 데이터와 가져오기 후보를 비교한 하나의 항목별 변경 결과입니다.
    /// </summary>
    /// <typeparam name="T">비교된 데이터 항목 타입입니다.</typeparam>
    internal sealed class DataImportDiffItem<T> where T : IDataEntry
    {
        /// <summary>
        /// 비교 대상 데이터의 ID입니다.
        /// </summary>
        internal string Id { get; }

        /// <summary>
        /// 이 항목의 변경 상태입니다.
        /// </summary>
        internal DataImportDiffType Type { get; }

        /// <summary>
        /// 현재 Asset에 존재하는 항목입니다.
        /// Added 상태에서는 기본값일 수 있습니다.
        /// </summary>
        internal T CurrentEntry { get; }

        /// <summary>
        /// 가져오기 후보에 존재하는 항목입니다.
        /// Removed 상태에서는 기본값일 수 있습니다.
        /// </summary>
        internal T IncomingEntry { get; }

        internal DataImportDiffItem(string id, DataImportDiffType type, T currentEntry, T incomingEntry)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Diff 항목의 ID는 비어 있을 수 없습니다.", nameof(id));
            }

            Id = id;
            Type = type;
            CurrentEntry = currentEntry;
            IncomingEntry = incomingEntry;
        }
    }
}