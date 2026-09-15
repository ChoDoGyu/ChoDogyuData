using System;
using CDG.Data;
using UnityEditor;
using UnityEngine;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// Import Preview가 생성된 시점의 대상 Asset과 후보 데이터 상태를 보관합니다.
    /// Apply 직전에 현재 상태와 비교하여 오래된 Preview의 적용을 차단하는 데 사용합니다.
    /// </summary>
    /// <typeparam name="T">가져오기 데이터 항목 타입입니다.</typeparam>
    internal sealed class DataImportStateSnapshot<T> where T : IDataEntry
    {
        private readonly int targetInstanceId;
        private readonly string targetSerializedState;
        private readonly string[] candidateSerializedStates;

        private DataImportStateSnapshot(int targetInstanceId, string targetSerializedState, string[] candidateSerializedStates)
        {
            this.targetInstanceId = targetInstanceId;
            this.targetSerializedState = targetSerializedState;
            this.candidateSerializedStates = candidateSerializedStates;
        }

        /// <summary>
        /// 현재 대상 Asset과 Candidate 상태를 직렬화하여 스냅샷을 생성합니다.
        /// </summary>
        internal static DataImportStateSnapshot<T> Capture(DataTableAsset<T> target, DataImportCandidate<T> candidate)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (candidate == null)
            {
                throw new ArgumentNullException(nameof(candidate));
            }

            string[] candidateStates = new string[candidate.Count];

            for (int index = 0; index < candidate.Count; index++)
            {
                candidateStates[index] = SerializeEntry(candidate.Entries[index]);
            }

            return new DataImportStateSnapshot<T>(
                target.GetInstanceID(),
                EditorJsonUtility.ToJson(target),
                candidateStates);
        }

        /// <summary>
        /// 현재 대상 Asset과 Candidate가 스냅샷 생성 시점과 동일한 상태인지 확인합니다.
        /// 대상 Asset 인스턴스, 직렬화 상태, Candidate 순서 및 각 항목 상태를 모두 비교합니다.
        /// </summary>
        internal bool Matches(DataTableAsset<T> target, DataImportCandidate<T> candidate)
        {
            if (target == null || candidate == null)
            {
                return false;
            }

            if (target.GetInstanceID() != targetInstanceId)
            {
                return false;
            }

            if (!string.Equals(EditorJsonUtility.ToJson(target), targetSerializedState, StringComparison.Ordinal))
            {
                return false;
            }

            if (candidate.Count != candidateSerializedStates.Length)
            {
                return false;
            }

            for (int index = 0; index < candidate.Count; index++)
            {
                string currentState = SerializeEntry(candidate.Entries[index]);

                if (!string.Equals(currentState, candidateSerializedStates[index], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 두 스냅샷이 동일한 대상과 동일한 직렬화 상태를 나타내는지 확인합니다.
        /// </summary>
        internal bool HasSameState(DataImportStateSnapshot<T> other)
        {
            if (other == null)
            {
                return false;
            }

            if (targetInstanceId != other.targetInstanceId)
            {
                return false;
            }

            if (!string.Equals(targetSerializedState, other.targetSerializedState, StringComparison.Ordinal))
            {
                return false;
            }

            if (candidateSerializedStates.Length != other.candidateSerializedStates.Length)
            {
                return false;
            }

            for (int index = 0; index < candidateSerializedStates.Length; index++)
            {
                if (!string.Equals(candidateSerializedStates[index], other.candidateSerializedStates[index], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        private static string SerializeEntry(T entry)
        {
            if (ReferenceEquals(entry, null))
            {
                return null;
            }

            return JsonUtility.ToJson(entry);
        }
    }
}