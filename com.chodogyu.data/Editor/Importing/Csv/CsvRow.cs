using System;
using System.Collections.Generic;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// CSV 문서에서 파싱된 하나의 행을 나타냅니다.
    /// 필드 값은 원본 순서를 유지하며 외부에서 수정할 수 없습니다.
    /// </summary>
    internal sealed class CsvRow
    {
        private readonly string[] fields;
        private readonly IReadOnlyList<string> readOnlyFields;

        /// <summary>
        /// 행에 포함된 필드 수입니다.
        /// </summary>
        internal int Count => fields.Length;

        /// <summary>
        /// 원본 CSV에서 이 행이 시작된 줄 번호입니다.
        /// 첫 번째 줄은 1입니다.
        /// </summary>
        internal int StartLine { get; }

        /// <summary>
        /// 행에 포함된 필드를 원본 순서대로 제공합니다.
        /// </summary>
        internal IReadOnlyList<string> Fields => readOnlyFields;

        /// <summary>
        /// 지정한 위치의 필드 값을 반환합니다.
        /// </summary>
        internal string this[int index] => fields[index];

        internal CsvRow(IEnumerable<string> fields, int startLine)
        {
            if (fields == null)
            {
                throw new ArgumentNullException(nameof(fields));
            }

            if (startLine < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(startLine));
            }

            List<string> snapshot = new List<string>(fields);

            this.fields = snapshot.ToArray();
            readOnlyFields = Array.AsReadOnly(this.fields);
            StartLine = startLine;
        }
    }
}