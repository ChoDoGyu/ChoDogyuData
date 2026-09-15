using System;
using System.Globalization;
using System.Text;
using CDG.Core.Results;
using CDG.Data;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// CSV 문자열 값을 대상 직렬화 필드 타입에 맞는 JSON 값으로 변환합니다.
    /// CSV 자동 매핑에서는 문자열, Bool, 숫자 및 Enum과 같은 단일 값 타입을 지원합니다.
    /// </summary>
    internal static class CsvFieldJsonValueConverter
    {
        internal static Result ValidateSupportedType(Type fieldType)
        {
            if (fieldType == null)
            {
                throw new ArgumentNullException(nameof(fieldType));
            }

            if (fieldType == typeof(string) ||
                fieldType == typeof(bool) ||
                fieldType.IsEnum ||
                IsSupportedNumberType(fieldType))
            {
                return Result.Success();
            }

            return Result.Failure(new ResultError(
                DataErrorCodes.ImportFailed,
                $"CSV 자동 매핑에서 지원하지 않는 필드 타입입니다: '{fieldType.FullName}'. 복합 데이터는 JSON Import를 사용하세요."));
        }

        internal static Result<string> ConvertToJsonLiteral(string value, Type fieldType)
        {
            if (fieldType == null)
            {
                throw new ArgumentNullException(nameof(fieldType));
            }

            if (value == null)
            {
                return CreateFailure("CSV 필드 값이 null입니다.");
            }

            Result supportedTypeResult = ValidateSupportedType(fieldType);

            if (supportedTypeResult.IsFailure)
            {
                return Result<string>.Failure(supportedTypeResult.Error);
            }

            if (fieldType == typeof(string))
            {
                return Result<string>.Success(ToJsonString(value));
            }

            if (value != value.Trim())
            {
                return CreateFailure(
                    $"'{fieldType.Name}' 값의 앞뒤에는 공백을 포함할 수 없습니다.");
            }

            if (fieldType == typeof(bool))
            {
                if (!bool.TryParse(value, out bool boolValue))
                {
                    return CreateFailure(
                        $"'{value}' 값을 Bool로 변환할 수 없습니다.");
                }

                return Result<string>.Success(
                    boolValue ? "true" : "false");
            }

            if (fieldType.IsEnum)
            {
                return ConvertEnum(value, fieldType);
            }

            return ConvertNumber(value, fieldType);
        }

        internal static string ToJsonString(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            StringBuilder builder = new StringBuilder(value.Length + 2);
            builder.Append('"');

            for (int index = 0; index < value.Length; index++)
            {
                char character = value[index];

                switch (character)
                {
                    case '"':
                        builder.Append("\\\"");
                        break;

                    case '\\':
                        builder.Append("\\\\");
                        break;

                    case '\b':
                        builder.Append("\\b");
                        break;

                    case '\f':
                        builder.Append("\\f");
                        break;

                    case '\n':
                        builder.Append("\\n");
                        break;

                    case '\r':
                        builder.Append("\\r");
                        break;

                    case '\t':
                        builder.Append("\\t");
                        break;

                    default:
                        if (character < 0x20)
                        {
                            builder.Append("\\u");
                            builder.Append(
                                ((int)character).ToString(
                                    "x4",
                                    CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            builder.Append(character);
                        }

                        break;
                }
            }

            builder.Append('"');
            return builder.ToString();
        }

        private static Result<string> ConvertEnum(string value, Type enumType)
        {
            if (!Enum.TryParse(
                enumType,
                value,
                false,
                out object parsedValue))
            {
                return CreateFailure(
                    $"'{value}' 값을 Enum '{enumType.Name}'으로 변환할 수 없습니다.");
            }

            Type underlyingType = Enum.GetUnderlyingType(enumType);

            string numericValue;

            switch (Type.GetTypeCode(underlyingType))
            {
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    numericValue = System.Convert.ToUInt64(
                        parsedValue,
                        CultureInfo.InvariantCulture).ToString(
                            CultureInfo.InvariantCulture);
                    break;

                default:
                    numericValue = System.Convert.ToInt64(
                        parsedValue,
                        CultureInfo.InvariantCulture).ToString(
                            CultureInfo.InvariantCulture);
                    break;
            }

            return Result<string>.Success(numericValue);
        }

        private static Result<string> ConvertNumber(string value, Type fieldType)
        {
            switch (Type.GetTypeCode(fieldType))
            {
                case TypeCode.SByte:
                    if (sbyte.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out sbyte sbyteValue))
                    {
                        return Result<string>.Success(sbyteValue.ToString(CultureInfo.InvariantCulture));
                    }

                    break;

                case TypeCode.Byte:
                    if (byte.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte byteValue))
                    {
                        return Result<string>.Success(byteValue.ToString(CultureInfo.InvariantCulture));
                    }

                    break;

                case TypeCode.Int16:
                    if (short.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out short shortValue))
                    {
                        return Result<string>.Success(shortValue.ToString(CultureInfo.InvariantCulture));
                    }

                    break;

                case TypeCode.UInt16:
                    if (ushort.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ushort ushortValue))
                    {
                        return Result<string>.Success(ushortValue.ToString(CultureInfo.InvariantCulture));
                    }

                    break;

                case TypeCode.Int32:
                    if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
                    {
                        return Result<string>.Success(intValue.ToString(CultureInfo.InvariantCulture));
                    }

                    break;

                case TypeCode.UInt32:
                    if (uint.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint uintValue))
                    {
                        return Result<string>.Success(uintValue.ToString(CultureInfo.InvariantCulture));
                    }

                    break;

                case TypeCode.Int64:
                    if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long longValue))
                    {
                        return Result<string>.Success(longValue.ToString(CultureInfo.InvariantCulture));
                    }

                    break;

                case TypeCode.UInt64:
                    if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong ulongValue))
                    {
                        return Result<string>.Success(ulongValue.ToString(CultureInfo.InvariantCulture));
                    }

                    break;

                case TypeCode.Single:
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue) &&
                        !float.IsNaN(floatValue) &&
                        !float.IsInfinity(floatValue))
                    {
                        return Result<string>.Success(floatValue.ToString("R", CultureInfo.InvariantCulture));
                    }

                    break;

                case TypeCode.Double:
                    if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double doubleValue) &&
                        !double.IsNaN(doubleValue) &&
                        !double.IsInfinity(doubleValue))
                    {
                        return Result<string>.Success(doubleValue.ToString("R", CultureInfo.InvariantCulture));
                    }

                    break;
            }

            return CreateFailure(
                $"'{value}' 값을 '{fieldType.Name}' 타입으로 변환할 수 없습니다.");
        }

        private static bool IsSupportedNumberType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                    return true;

                default:
                    return false;
            }
        }

        private static Result<string> CreateFailure(string message)
        {
            return Result<string>.Failure(new ResultError(
                DataErrorCodes.ImportFailed,
                message));
        }
    }
}