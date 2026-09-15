using System;
using CDG.Data;
using CDG.Data.Validation;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// 외부 데이터 파싱이 완료된 뒤 실제 Asset에 적용하기 전에 확인할 수 있는 가져오기 결과입니다.
    /// 후보 데이터, 검증 결과, Diff 및 필요한 경우 Preview 생성 시점의 상태 스냅샷을 제공합니다.
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
        /// Diff를 계산하지 않은 경우 null입니다.
        /// </summary>
        internal DataImportDiff<T> Diff { get; }

        /// <summary>
        /// Diff가 생성된 시점의 대상 Asset과 Candidate 상태입니다.
        /// 대상 Asset과 연결되지 않은 Preview에서는 null입니다.
        /// </summary>
        internal DataImportStateSnapshot<T> StateSnapshot { get; }

        /// <summary>
        /// 후보 데이터가 현재 Data Framework의 모든 검증 규칙을 만족하는지를 나타냅니다.
        /// </summary>
        internal bool IsValid => ValidationReport.IsValid;

        /// <summary>
        /// 현재 데이터와의 Diff가 계산되어 있는지를 나타냅니다.
        /// </summary>
        internal bool HasDiff => Diff != null;

        /// <summary>
        /// Apply 전에 비교할 상태 스냅샷이 존재하는지를 나타냅니다.
        /// </summary>
        internal bool HasStateSnapshot => StateSnapshot != null;

        internal DataImportPreview(DataImportCandidate<T> candidate, DataValidationReport validationReport) : this(candidate, validationReport, null, null)
        {
        }

        internal DataImportPreview(DataImportCandidate<T> candidate, DataValidationReport validationReport, DataImportDiff<T> diff) : this(candidate, validationReport, diff, null)
        {
        }

        internal DataImportPreview(DataImportCandidate<T> candidate, DataValidationReport validationReport, DataImportDiff<T> diff, DataImportStateSnapshot<T> stateSnapshot)
        {
            Candidate = candidate ?? throw new ArgumentNullException(nameof(candidate));
            ValidationReport = validationReport ?? throw new ArgumentNullException(nameof(validationReport));
            Diff = diff;
            StateSnapshot = stateSnapshot;
        }
    }
}