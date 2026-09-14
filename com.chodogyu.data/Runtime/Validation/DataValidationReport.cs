using System;
using System.Collections.Generic;

namespace CDG.Data.Validation
{
    /// <summary>
    /// 데이터 검증 과정에서 발견된 모든 문제를 포함하는 읽기 전용 결과입니다.
    /// 문제가 하나도 없으면 유효한 검증 결과로 간주됩니다.
    /// </summary>
    public sealed class DataValidationReport
    {
        private readonly DataValidationIssue[] issues;
        private readonly IReadOnlyList<DataValidationIssue> readOnlyIssues;

        /// <summary>
        /// 검증 과정에서 문제가 발견되지 않았는지를 나타냅니다.
        /// </summary>
        public bool IsValid => issues.Length == 0;

        /// <summary>
        /// 검증 과정에서 발견된 문제 수입니다.
        /// </summary>
        public int Count => issues.Length;

        /// <summary>
        /// 검증 과정에서 발견된 모든 문제를 발견 순서대로 제공합니다.
        /// 반환된 컬렉션은 외부에서 수정할 수 없습니다.
        /// </summary>
        public IReadOnlyList<DataValidationIssue> Issues => readOnlyIssues;

        internal DataValidationReport(IEnumerable<DataValidationIssue> issues)
        {
            if (issues == null)
            {
                throw new ArgumentNullException(nameof(issues));
            }

            List<DataValidationIssue> snapshot = new List<DataValidationIssue>(issues);

            this.issues = snapshot.ToArray();
            readOnlyIssues = Array.AsReadOnly(this.issues);
        }
    }
}