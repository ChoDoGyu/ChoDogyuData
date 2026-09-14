using CDG.Core.Results;
using CDG.Data;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// 문자열 형태의 외부 데이터를 읽어 Asset 적용 전 데이터 후보로 변환하는 Importer 계약입니다.
    /// Import 과정에서는 대상 Asset을 직접 수정하지 않습니다.
    /// </summary>
    /// <typeparam name="T">가져올 데이터 항목 타입입니다.</typeparam>
    internal interface IDataTextImporter<T> where T : IDataEntry
    {
        /// <summary>
        /// 지정한 문자열을 해석하여 데이터 후보를 생성합니다.
        /// 형식 오류 등으로 가져올 수 없는 경우 실패 결과를 반환합니다.
        /// </summary>
        /// <param name="text">가져올 원본 문자열입니다.</param>
        /// <returns>가져오기 후보 또는 실패 정보를 포함하는 결과입니다.</returns>
        Result<DataImportCandidate<T>> Import(string text);
    }
}