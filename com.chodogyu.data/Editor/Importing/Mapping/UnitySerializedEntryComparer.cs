using System;
using CDG.Data;
using UnityEngine;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// Unity 직렬화 결과를 기준으로 두 데이터 항목의 실제 저장 상태가 동일한지 비교합니다.
    /// 직렬화 대상이 아닌 런타임 전용 필드나 프로퍼티는 비교에 포함하지 않습니다.
    /// </summary>
    /// <typeparam name="T">비교할 데이터 항목 타입입니다.</typeparam>
    internal sealed class UnitySerializedEntryComparer<T> : IDataImportEntryComparer<T> where T : IDataEntry
    {
        /// <summary>
        /// 두 데이터 항목의 Unity 직렬화 상태가 동일한지 확인합니다.
        /// </summary>
        public bool AreEquivalent(T current, T incoming)
        {
            if (ReferenceEquals(current, incoming))
            {
                return true;
            }

            if (ReferenceEquals(current, null) ||
                ReferenceEquals(incoming, null))
            {
                return false;
            }

            string currentJson = JsonUtility.ToJson(current);
            string incomingJson = JsonUtility.ToJson(incoming);

            return string.Equals(
                currentJson,
                incomingJson,
                StringComparison.Ordinal);
        }
    }
}