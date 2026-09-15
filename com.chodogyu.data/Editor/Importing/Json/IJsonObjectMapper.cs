using CDG.Core.Results;
using CDG.Data;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// 하나의 JSON Object 원본을 지정한 데이터 항목 타입으로 변환하는 계약입니다.
    /// JSON 필드와 실제 데이터 타입 사이의 매핑 책임을 담당합니다.
    /// </summary>
    /// <typeparam name="T">변환할 데이터 항목 타입입니다.</typeparam>
    internal interface IJsonObjectMapper<T> where T : IDataEntry
    {
        /// <summary>
        /// 지정한 JSON Object를 데이터 항목으로 변환합니다.
        /// 필요한 필드가 없거나 값을 변환할 수 없는 경우 실패 결과를 반환합니다.
        /// </summary>
        /// <param name="source">변환할 JSON Object 원본입니다.</param>
        /// <returns>변환된 데이터 항목 또는 실패 정보를 포함하는 결과입니다.</returns>
        Result<T> Map(JsonObjectSource source);
    }
}