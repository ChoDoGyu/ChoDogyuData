using System;
using CDG.Data;
using CDG.Data.Validation;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// 외부 데이터 파싱이 완료된 뒤 실제 Asset에 적용하기 전에 확인할 수 있는 가져오기 결과입니다.
    /// 파싱된 후보 데이터와 해당 데이터의 검증 결과를 함께 제공합니다.
    /// </summary>
    /// <typeparam name="T">가져온 데이터 항목 타입입니다.</typeparam>
    internal sealed class DataImportPreview<T> where T : IDataEntry
    {
        /// <summary>
        /// 파싱을 통해 생성된 데이터 후보입니다.
        /// </summary>
        internal DataImportCandidate<T> Candidate { get; }

        /// <summary>
        /// 후보 데이터에 대한 Data Framework 검증 결과입니다.
        /// </summary>
        internal DataValidationReport ValidationReport { get; }

        /// <summary>
        /// 후보 데이터가 현재 Data Framework의 모든 검증 규칙을 만족하는지를 나타냅니다.
        /// </summary>
        internal bool IsValid => ValidationReport.IsValid;

        /// <summary>
        /// 지정한 후보 데이터와 검증 결과를 사용하여 가져오기 미리보기 결과를 생성합니다.
        /// </summary>
        internal DataImportPreview(DataImportCandidate<T> candidate, DataValidationReport validationReport)
        {
            Candidate = candidate ?? throw new ArgumentNullException(nameof(candidate));
            ValidationReport = validationReport ?? throw new ArgumentNullException(nameof(validationReport));
        }
    }
}