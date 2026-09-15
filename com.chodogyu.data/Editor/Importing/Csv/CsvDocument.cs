using System;
using System.Collections.Generic;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// CSV 문자열을 파싱하여 생성된 읽기 전용 문서입니다.
    /// 파싱된 행의 순서를 그대로 유지합니다.
    /// </summary>
    internal sealed class CsvDocument
    {
        private readonly CsvRow[] rows;
        private readonly IReadOnlyList<CsvRow> readOnlyRows;

        /// <summary>
        /// 문서에 포함된 행 수입니다.
        /// </summary>
        internal int Count => rows.Length;

        /// <summary>
        /// 파싱된 모든 행을 원본 순서대로 제공합니다.
        /// </summary>
        internal IReadOnlyList<CsvRow> Rows => readOnlyRows;

        /// <summary>
        /// 지정한 위치의 행을 반환합니다.
        /// </summary>
        internal CsvRow this[int index] => rows[index];

        internal CsvDocument(IEnumerable<CsvRow> rows)
        {
            if (rows == null)
            {
                throw new ArgumentNullException(nameof(rows));
            }

            List<CsvRow> snapshot = new List<CsvRow>(rows);

            this.rows = snapshot.ToArray();
            readOnlyRows = Array.AsReadOnly(this.rows);
        }
    }
}