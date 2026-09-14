namespace CDG.Data
{
    /// <summary>
    /// Data Framework에서 관리되는 개별 데이터 항목의 공통 계약입니다.
    /// 각 항목은 테이블 내에서 식별할 수 있는 고유한 문자열 ID를 제공해야 합니다.
    /// </summary>
    public interface IDataEntry
    {
        /// <summary>
        /// 데이터 항목을 식별하는 고유 ID입니다.
        /// null, 빈 문자열, 공백만 포함한 값과 앞뒤 공백이 포함된 값은 유효하지 않습니다.
        /// </summary>
        string Id { get; }
    }
}