namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// 기존 데이터와 가져오기 후보를 비교했을 때 하나의 데이터 항목이 가지는 변경 상태입니다.
    /// </summary>
    internal enum DataImportDiffType
    {
        /// <summary>
        /// 기존 데이터에는 없고 가져오기 후보에 새로 존재하는 항목입니다.
        /// </summary>
        Added,

        /// <summary>
        /// 기존 데이터에는 존재하지만 가져오기 후보에는 없는 항목입니다.
        /// </summary>
        Removed,

        /// <summary>
        /// 동일한 ID가 양쪽에 존재하지만 데이터 내용이 변경된 항목입니다.
        /// </summary>
        Modified,

        /// <summary>
        /// 동일한 ID가 양쪽에 존재하고 데이터 내용도 동일한 항목입니다.
        /// </summary>
        Unchanged
    }
}