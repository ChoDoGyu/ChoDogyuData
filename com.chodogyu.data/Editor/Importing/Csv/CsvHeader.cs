using System;
using System.Collections.Generic;
using CDG.Core.Results;
using CDG.Data;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// CSV 문서의 첫 번째 행에서 생성된 읽기 전용 헤더 정보입니다.
    /// 열 이름은 대소문자를 구분하며 입력된 순서를 그대로 유지합니다.
    /// </summary>
    internal sealed class CsvHeader
    {
        private readonly string[] columns;
        private readonly IReadOnlyList<string> readOnlyColumns;
        private readonly Dictionary<string, int> indexesByName;

        /// <summary>
        /// 헤더에 포함된 열 수입니다.
        /// </summary>
        internal int Count => columns.Length;

        /// <summary>
        /// 헤더의 열 이름을 원본 순서대로 제공합니다.
        /// 반환된 컬렉션을 통해 열 이름을 변경할 수 없습니다.
        /// </summary>
        internal IReadOnlyList<string> Columns => readOnlyColumns;

        private CsvHeader(string[] columns, Dictionary<string, int> indexesByName)
        {
            this.columns = columns;
            readOnlyColumns = Array.AsReadOnly(columns);
            this.indexesByName = indexesByName;
        }

        /// <summary>
        /// 지정한 CSV 행을 헤더로 검증하고 생성합니다.
        /// 빈 열 이름, 앞뒤 공백이 포함된 이름 및 중복 이름은 허용하지 않습니다.
        /// </summary>
        internal static Result<CsvHeader> Create(CsvRow row)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            string[] columns = new string[row.Count];
            Dictionary<string, int> indexesByName = new Dictionary<string, int>(StringComparer.Ordinal);

            for (int index = 0; index < row.Count; index++)
            {
                string column = row[index];

                if (string.IsNullOrWhiteSpace(column))
                {
                    return CreateFailure(row.StartLine, index, "헤더 이름은 비어 있거나 공백만 포함할 수 없습니다.");
                }

                if (column != column.Trim())
                {
                    return CreateFailure(row.StartLine, index, "헤더 이름의 앞뒤에는 공백을 포함할 수 없습니다.");
                }

                if (!indexesByName.TryAdd(column, index))
                {
                    return CreateFailure(row.StartLine, index, $"중복된 헤더 이름입니다: '{column}'");
                }

                columns[index] = column;
            }

            return Result<CsvHeader>.Success(new CsvHeader(columns, indexesByName));
        }

        /// <summary>
        /// 지정한 이름의 열이 존재하는지 확인합니다.
        /// 열 이름 비교는 대소문자를 구분합니다.
        /// </summary>
        internal bool Contains(string columnName)
        {
            if (columnName == null)
            {
                return false;
            }

            return indexesByName.ContainsKey(columnName);
        }

        /// <summary>
        /// 지정한 열 이름의 인덱스를 조회합니다.
        /// 열이 존재하지 않으면 false를 반환하고 index에는 -1을 설정합니다.
        /// </summary>
        internal bool TryGetIndex(string columnName, out int index)
        {
            if (columnName == null || !indexesByName.TryGetValue(columnName, out index))
            {
                index = -1;
                return false;
            }

            return true;
        }

        private static Result<CsvHeader> CreateFailure(int line, int columnIndex, string message)
        {
            return Result<CsvHeader>.Failure(new ResultError(
                DataErrorCodes.ImportFailed,
                $"CSV 헤더 검증에 실패했습니다. {line}행 {columnIndex + 1}열: {message}"));
        }
    }
}