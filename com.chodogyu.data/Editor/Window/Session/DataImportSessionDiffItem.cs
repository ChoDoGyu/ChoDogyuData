using System;
using CDG.Data.Editor.Importing;

namespace CDG.Data.Editor
{
    /// <summary>
    /// Generic 데이터 타입과 관계없이 EditorWindow에서 표시할 수 있는 하나의 Import Diff 항목입니다.
    /// </summary>
    internal sealed class DataImportSessionDiffItem
    {
        /// <summary>
        /// 비교된 데이터 항목의 ID입니다.
        /// </summary>
        internal string Id { get; }

        /// <summary>
        /// 데이터 항목의 변경 상태입니다.
        /// </summary>
        internal DataImportDiffType Type { get; }

        internal DataImportSessionDiffItem(string id, DataImportDiffType type)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException(
                    "Diff 항목의 ID는 비어 있을 수 없습니다.",
                    nameof(id));
            }

            Id = id;
            Type = type;
        }
    }
}