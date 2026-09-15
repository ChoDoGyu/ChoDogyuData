using System;
using CDG.Data;
using CDG.Data.Validation;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// 외부 데이터 파싱이 완료된 뒤 실제 Asset에 적용하기 전에 확인할 수 있는 가져오기 결과입니다.
    /// 후보 데이터, 검증 결과 및 필요한 경우 현재 데이터와의 Diff 결과를 함께 제공합니다.
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
        /// 현재 데이터와 가져오기 후보를 비교한 Diff 결과입니다.
        /// 후보 데이터가 유효하지 않아 Diff를 계산하지 않은 경우 null입니다.
        /// </summary>
        internal DataImportDiff<T> Diff { get; }

        /// <summary>
        /// 후보 데이터가 현재 Data Framework의 모든 검증 규칙을 만족하는지를 나타냅니다.
        /// </summary>
        internal bool IsValid => ValidationReport.IsValid;

        /// <summary>
        /// 현재 데이터와의 Diff가 계산되어 있는지를 나타냅니다.
        /// </summary>
        internal bool HasDiff => Diff != null;

        /// <summary>
        /// 지정한 후보 데이터와 검증 결과를 사용하여 Diff가 없는 가져오기 미리보기를 생성합니다.
        /// </summary>
        internal DataImportPreview(DataImportCandidate<T> candidate, DataValidationReport validationReport) : this(candidate, validationReport, null)
        {
        }

        /// <summary>
        /// 지정한 후보 데이터, 검증 결과 및 Diff 결과를 사용하여 가져오기 미리보기를 생성합니다.
        /// </summary>
        internal DataImportPreview(DataImportCandidate<T> candidate, DataValidationReport validationReport, DataImportDiff<T> diff)
        {
            Candidate = candidate ?? throw new ArgumentNullException(nameof(candidate));
            ValidationReport = validationReport ?? throw new ArgumentNullException(nameof(validationReport));
            Diff = diff;
        }
    }
}