using CDG.Data;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// 동일한 ID를 가진 기존 데이터와 가져오기 후보 데이터의 실제 내용이 같은지 비교하는 계약입니다.
    /// </summary>
    /// <typeparam name="T">비교할 데이터 항목 타입입니다.</typeparam>
    internal interface IDataImportEntryComparer<T> where T : IDataEntry
    {
        /// <summary>
        /// 기존 항목과 가져오기 후보 항목이 동일한 데이터 내용을 가지는지 확인합니다.
        /// 동일한 ID를 가진 두 항목에 대해서만 호출됩니다.
        /// </summary>
        /// <param name="current">현재 Asset에 존재하는 데이터 항목입니다.</param>
        /// <param name="incoming">가져오기 후보의 데이터 항목입니다.</param>
        /// <returns>내용이 동일하면 true, 변경되었다면 false입니다.</returns>
        bool AreEquivalent(T current, T incoming);
    }
}