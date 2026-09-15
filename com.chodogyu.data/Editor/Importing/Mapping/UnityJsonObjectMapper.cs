using System;
using CDG.Core.Results;
using CDG.Data;
using UnityEngine;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// Unity JSON 직렬화 규칙을 사용하여 JSON Object를 데이터 항목으로 자동 변환합니다.
    /// public 필드와 SerializeField가 적용된 private 필드를 대상으로 합니다.
    /// </summary>
    internal sealed class UnityJsonObjectMapper<T> : IJsonObjectMapper<T> where T : IDataEntry
    {
        private readonly Result<UnitySerializedEntrySchema> schemaResult;

        internal UnityJsonObjectMapper()
        {
            schemaResult = UnitySerializedEntrySchema.Create(typeof(T));
        }

        /// <summary>
        /// 지정한 JSON Object를 Unity 직렬화 규칙에 따라 데이터 항목으로 변환합니다.
        /// </summary>
        public Result<T> Map(JsonObjectSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (schemaResult.IsFailure)
            {
                return Result<T>.Failure(schemaResult.Error);
            }

            try
            {
                T entry = JsonUtility.FromJson<T>(source.Text);

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
                    $"'{typeof(T).FullName}' JSON 변환에 실패했습니다. {exception.Message}");
            }
            catch (InvalidOperationException exception)
            {
                return CreateFailure(
                    $"'{typeof(T).FullName}' JSON 변환에 실패했습니다. {exception.Message}");
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