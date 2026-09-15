using System;
using System.Reflection;
using System.Text;
using CDG.Core.Results;
using CDG.Data;
using UnityEngine;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// Unity 직렬화 필드 이름과 CSV Header를 자동으로 연결하여 데이터 항목을 생성합니다.
    /// CSV 자동 매핑은 단일 값 형태의 직렬화 필드만 지원합니다.
    /// </summary>
    internal sealed class UnityCsvRowMapper<T> : ICsvRowMapper<T>, ICsvHeaderValidator where T : IDataEntry
    {
        private readonly Result<UnitySerializedEntrySchema> schemaResult;

        internal UnityCsvRowMapper()
        {
            schemaResult = UnitySerializedEntrySchema.Create(typeof(T));
        }

        /// <summary>
        /// CSV Header와 대상 타입의 Unity 직렬화 필드 구성이 정확히 일치하는지 확인합니다.
        /// </summary>
        public Result ValidateHeader(CsvHeader header)
        {
            if (header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }

            if (schemaResult.IsFailure)
            {
                return Result.Failure(schemaResult.Error);
            }

            UnitySerializedEntrySchema schema = schemaResult.Value;

            if (header.Count != schema.Count)
            {
                return Result.Failure(new ResultError(
                    DataErrorCodes.ImportFailed,
                    $"CSV Header와 '{typeof(T).Name}' 직렬화 필드 수가 일치하지 않습니다. Header: {header.Count}, 필드: {schema.Count}"));
            }

            for (int index = 0; index < header.Count; index++)
            {
                string columnName = header.Columns[index];

                if (!schema.TryGetField(columnName, out FieldInfo field))
                {
                    return Result.Failure(new ResultError(
                        DataErrorCodes.ImportFailed,
                        $"CSV Header '{columnName}'과 일치하는 직렬화 필드를 '{typeof(T).Name}' 타입에서 찾을 수 없습니다."));
                }

                Result supportedTypeResult =
                    CsvFieldJsonValueConverter.ValidateSupportedType(
                        field.FieldType);

                if (supportedTypeResult.IsFailure)
                {
                    return supportedTypeResult;
                }
            }

            return Result.Success();
        }

        /// <summary>
        /// CSV 행의 값을 대상 타입의 직렬화 필드에 맞춰 변환하고 데이터 항목을 생성합니다.
        /// </summary>
        public Result<T> Map(CsvHeader header, CsvRow row)
        {
            if (header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }

            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            Result headerValidationResult = ValidateHeader(header);

            if (headerValidationResult.IsFailure)
            {
                return Result<T>.Failure(headerValidationResult.Error);
            }

            if (row.Count != header.Count)
            {
                return CreateFailure(
                    $"CSV 데이터 행의 열 수가 Header와 일치하지 않습니다. Header: {header.Count}, 데이터 행: {row.Count}");
            }

            UnitySerializedEntrySchema schema = schemaResult.Value;
            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.Append('{');

            for (int index = 0; index < header.Count; index++)
            {
                if (index > 0)
                {
                    jsonBuilder.Append(',');
                }

                string columnName = header.Columns[index];
                schema.TryGetField(columnName, out FieldInfo field);

                Result<string> valueResult =
                    CsvFieldJsonValueConverter.ConvertToJsonLiteral(
                        row[index],
                        field.FieldType);

                if (valueResult.IsFailure)
                {
                    return CreateFailure(
                        $"'{columnName}' 열 변환에 실패했습니다. {valueResult.Error.Message}");
                }

                jsonBuilder.Append(
                    CsvFieldJsonValueConverter.ToJsonString(columnName));

                jsonBuilder.Append(':');
                jsonBuilder.Append(valueResult.Value);
            }

            jsonBuilder.Append('}');

            try
            {
                T entry = JsonUtility.FromJson<T>(
                    jsonBuilder.ToString());

                if (ReferenceEquals(entry, null))
                {
                    return CreateFailure(
                        $"'{typeof(T).FullName}' 데이터 항목을 생성하지 못했습니다.");
                }

                return Result<T>.Success(entry);
            }
            catch (ArgumentException exception)
            {
                return CreateFailure(
                    $"'{typeof(T).FullName}' 데이터 항목 생성에 실패했습니다. {exception.Message}");
            }
            catch (InvalidOperationException exception)
            {
                return CreateFailure(
                    $"'{typeof(T).FullName}' 데이터 항목 생성에 실패했습니다. {exception.Message}");
            }
        }

        private static Result<T> CreateFailure(string message)
        {
            return Result<T>.Failure(new ResultError(
                DataErrorCodes.ImportFailed,
                message));
        }
    }
}