using CDG.Core.Results;
using CDG.Data;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// CSV의 하나의 데이터 행을 지정한 데이터 항목 타입으로 변환하는 계약입니다.
    /// CSV 열 구조와 실제 데이터 타입 사이의 매핑 책임을 담당합니다.
    /// </summary>
    /// <typeparam name="T">변환할 데이터 항목 타입입니다.</typeparam>
    internal interface ICsvRowMapper<T> where T : IDataEntry
    {
        /// <summary>
        /// CSV 헤더와 하나의 데이터 행을 사용하여 데이터 항목을 생성합니다.
        /// 필요한 열이 없거나 필드 값을 변환할 수 없는 경우 실패 결과를 반환합니다.
        /// </summary>
        /// <param name="header">현재 CSV 문서의 헤더 정보입니다.</param>
        /// <param name="row">변환할 CSV 데이터 행입니다.</param>
        /// <returns>변환된 데이터 항목 또는 실패 정보를 포함하는 결과입니다.</returns>
        Result<T> Map(CsvHeader header, CsvRow row);
    }
}