using CDG.Core.Results;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// CSV 데이터 행 변환 전에 Header 구조를 추가로 검증할 수 있는 선택적 계약입니다.
    /// </summary>
    internal interface ICsvHeaderValidator
    {
        /// <summary>
        /// 지정한 CSV Header가 현재 Mapper에서 처리 가능한 구조인지 검증합니다.
        /// </summary>
        Result ValidateHeader(CsvHeader header);
    }
}