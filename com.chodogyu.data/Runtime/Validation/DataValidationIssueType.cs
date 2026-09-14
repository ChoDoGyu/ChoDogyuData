namespace CDG.Data.Validation
{
    /// <summary>
    /// 데이터 검증 과정에서 발견할 수 있는 문제의 종류를 나타냅니다.
    /// </summary>
    public enum DataValidationIssueType
    {
        /// <summary>
        /// 검증 대상 컬렉션 자체가 null인 경우입니다.
        /// </summary>
        NullSource,

        /// <summary>
        /// 데이터 컬렉션 내부에 null 항목이 포함된 경우입니다.
        /// </summary>
        NullEntry,

        /// <summary>
        /// 데이터 항목의 ID가 기본 ID 규칙을 만족하지 않는 경우입니다.
        /// </summary>
        InvalidId,

        /// <summary>
        /// 동일한 ID를 가진 데이터 항목이 둘 이상 존재하는 경우입니다.
        /// </summary>
        DuplicateId
    }
}