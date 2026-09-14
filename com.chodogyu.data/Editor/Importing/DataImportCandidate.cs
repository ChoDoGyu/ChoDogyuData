using System;
using System.Collections.Generic;
using CDG.Data;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// 외부 데이터 파싱이 성공한 뒤 실제 Asset에 적용되기 전까지 유지되는 데이터 후보입니다.
    /// 생성 시 입력 데이터를 스냅샷으로 보관하며 외부에서는 내용을 수정할 수 없습니다.
    /// </summary>
    /// <typeparam name="T">가져온 데이터 항목 타입입니다.</typeparam>
    internal sealed class DataImportCandidate<T> where T : IDataEntry
    {
        private readonly T[] entries;
        private readonly IReadOnlyList<T> readOnlyEntries;

        /// <summary>
        /// 가져오기 후보에 포함된 데이터 항목 수입니다.
        /// </summary>
        internal int Count => entries.Length;

        /// <summary>
        /// 가져오기 후보에 포함된 데이터 항목을 원본 순서대로 제공합니다.
        /// 반환된 컬렉션을 통해 항목을 추가, 제거 또는 교체할 수 없습니다.
        /// </summary>
        internal IReadOnlyList<T> Entries => readOnlyEntries;

        /// <summary>
        /// 지정한 데이터 항목을 복사하여 변경되지 않는 가져오기 후보를 생성합니다.
        /// </summary>
        /// <param name="entries">후보로 보관할 데이터 항목입니다.</param>
        internal DataImportCandidate(IEnumerable<T> entries)
        {
            if (entries == null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            List<T> snapshot = new List<T>(entries);

            this.entries = snapshot.ToArray();
            readOnlyEntries = Array.AsReadOnly(this.entries);
        }
    }
}