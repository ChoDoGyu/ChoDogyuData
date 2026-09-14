using System;
using System.Collections.Generic;

namespace CDG.Data.Validation
{
    /// <summary>
    /// 데이터 항목 컬렉션을 검사하고 테이블 생성에 영향을 주는 문제를 수집합니다.
    /// 검증 과정에서는 입력 데이터를 수정하지 않습니다.
    /// </summary>
    public static class DataTableValidator
    {
        /// <summary>
        /// 지정한 데이터 항목들을 검증하고 발견된 모든 문제를 반환합니다.
        /// ID 비교는 대소문자를 구분하며, 유효한 ID에 대해서만 중복 여부를 검사합니다.
        /// </summary>
        /// <typeparam name="T">검증할 데이터 항목 타입입니다.</typeparam>
        /// <param name="source">검증할 데이터 항목 컬렉션입니다.</param>
        /// <returns>발견된 모든 문제를 포함하는 검증 결과입니다.</returns>
        public static DataValidationReport Validate<T>(IEnumerable<T> source) where T : IDataEntry
        {
            List<DataValidationIssue> issues = new List<DataValidationIssue>();

            if (source == null)
            {
                issues.Add(new DataValidationIssue(
                    DataValidationIssueType.NullSource,
                    "데이터 검증 대상 컬렉션은 null일 수 없습니다."));

                return new DataValidationReport(issues);
            }

            HashSet<string> registeredIds = new HashSet<string>(StringComparer.Ordinal);
            int index = 0;

            foreach (T entry in source)
            {
                if (entry == null)
                {
                    issues.Add(new DataValidationIssue(
                        DataValidationIssueType.NullEntry,
                        "데이터 컬렉션에 null 항목이 포함되어 있습니다.",
                        index));

                    index++;
                    continue;
                }

                string id = entry.Id;

                if (!DataIdValidator.IsValid(id))
                {
                    issues.Add(new DataValidationIssue(
                        DataValidationIssueType.InvalidId,
                        $"유효하지 않은 데이터 ID입니다: '{id}'",
                        index,
                        id));

                    index++;
                    continue;
                }

                if (!registeredIds.Add(id))
                {
                    issues.Add(new DataValidationIssue(
                        DataValidationIssueType.DuplicateId,
                        $"중복된 데이터 ID입니다: '{id}'",
                        index,
                        id));
                }

                index++;
            }

            return new DataValidationReport(issues);
        }
    }
}