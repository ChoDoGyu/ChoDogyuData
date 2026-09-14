using System;
using CDG.Data;
using CDG.Data.Validation;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// 가져오기 후보 데이터가 Data Framework의 공통 데이터 규칙을 만족하는지 검사합니다.
    /// Runtime의 데이터 검증 규칙을 재사용하며 후보 데이터 자체는 수정하지 않습니다.
    /// </summary>
    internal static class DataImportCandidateValidator
    {
        /// <summary>
        /// 지정한 가져오기 후보의 모든 데이터 항목을 검증합니다.
        /// </summary>
        /// <typeparam name="T">검증할 데이터 항목 타입입니다.</typeparam>
        /// <param name="candidate">검증할 가져오기 후보입니다.</param>
        /// <returns>발견된 모든 문제를 포함하는 검증 결과입니다.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="candidate"/>가 null인 경우 발생합니다.
        /// </exception>
        internal static DataValidationReport Validate<T>(DataImportCandidate<T> candidate) where T : IDataEntry
        {
            if (candidate == null)
            {
                throw new ArgumentNullException(nameof(candidate));
            }

            return DataTableValidator.Validate(candidate.Entries);
        }
    }
}