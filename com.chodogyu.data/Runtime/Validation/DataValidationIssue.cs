using System;

namespace CDG.Data.Validation
{
    /// <summary>
    /// 데이터 검증 과정에서 발견된 하나의 문제를 나타냅니다.
    /// 문제 종류와 메시지뿐 아니라 가능한 경우 문제가 발생한 항목의 위치와 ID를 함께 제공합니다.
    /// </summary>
    public sealed class DataValidationIssue
    {
        /// <summary>
        /// 발견된 문제의 종류입니다.
        /// </summary>
        public DataValidationIssueType Type { get; }

        /// <summary>
        /// 문제의 원인을 설명하는 메시지입니다.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 문제가 발견된 데이터 항목의 입력 순서 기준 인덱스입니다.
        /// 특정 항목과 연결되지 않은 문제인 경우 -1입니다.
        /// </summary>
        public int EntryIndex { get; }

        /// <summary>
        /// 문제가 발생한 데이터 항목의 ID입니다.
        /// null 항목처럼 ID를 확인할 수 없는 경우 null입니다.
        /// </summary>
        public string EntryId { get; }

        internal DataValidationIssue(DataValidationIssueType type, string message, int entryIndex = -1, string entryId = null)
        {
            Type = type;
            Message = message ?? throw new ArgumentNullException(nameof(message));
            EntryIndex = entryIndex;
            EntryId = entryId;
        }
    }
}