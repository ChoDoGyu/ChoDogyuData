using System;
using System.Collections.Generic;
using CDG.Core.Results;

namespace CDG.Data
{
    /// <summary>
    /// 동일한 데이터 타입의 항목을 ID 기준으로 관리하고 조회하는 읽기 전용 데이터 테이블입니다.
    /// 테이블이 생성된 이후에는 항목의 추가 및 제거를 지원하지 않습니다.
    /// </summary>
    /// <typeparam name="T">테이블에서 관리할 데이터 항목 타입입니다.</typeparam>
    public sealed class DataTable<T> where T : IDataEntry
    {
        private readonly T[] entries;
        private readonly IReadOnlyList<T> readOnlyEntries;
        private readonly Dictionary<string, T> entriesById;

        /// <summary>
        /// 테이블에 포함된 데이터 항목 수입니다.
        /// </summary>
        public int Count => entries.Length;

        /// <summary>
        /// 테이블에 포함된 데이터 항목을 입력 순서대로 제공합니다.
        /// 반환된 컬렉션을 통해 항목의 추가, 제거 또는 교체를 수행할 수 없습니다.
        /// </summary>
        public IReadOnlyList<T> Entries => readOnlyEntries;

        private DataTable(T[] entries, Dictionary<string, T> entriesById)
        {
            this.entries = entries;
            readOnlyEntries = Array.AsReadOnly(entries);
            this.entriesById = entriesById;
        }

        /// <summary>
        /// 지정한 데이터 항목들로 읽기 전용 데이터 테이블을 생성합니다.
        /// 입력 순서는 유지되며 ID 조회는 대소문자를 구분합니다.
        /// </summary>
        /// <param name="source">테이블에 포함할 데이터 항목입니다.</param>
        /// <returns>테이블 생성 성공 여부와 생성된 테이블 또는 오류 정보를 반환합니다.</returns>
        public static Result<DataTable<T>> Create(IEnumerable<T> source)
        {
            if (source == null)
            {
                return Result<DataTable<T>>.Failure(new ResultError(DataErrorCodes.ValidationFailed, "데이터 테이블의 입력 컬렉션은 null일 수 없습니다."));
            }

            List<T> snapshot = new List<T>();
            Dictionary<string, T> entriesById = new Dictionary<string, T>(StringComparer.Ordinal);

            foreach (T entry in source)
            {
                if (entry == null)
                {
                    return Result<DataTable<T>>.Failure(new ResultError(DataErrorCodes.ValidationFailed, "데이터 테이블에는 null 항목을 포함할 수 없습니다."));
                }

                if (!DataIdValidator.IsValid(entry.Id))
                {
                    return Result<DataTable<T>>.Failure(new ResultError(DataErrorCodes.InvalidId, $"유효하지 않은 데이터 ID입니다: '{entry.Id}'"));
                }

                if (!entriesById.TryAdd(entry.Id, entry))
                {
                    return Result<DataTable<T>>.Failure(new ResultError(DataErrorCodes.ValidationFailed, $"중복된 데이터 ID입니다: '{entry.Id}'"));
                }

                snapshot.Add(entry);
            }

            DataTable<T> table = new DataTable<T>(snapshot.ToArray(), entriesById);
            return Result<DataTable<T>>.Success(table);
        }

        /// <summary>
        /// 지정한 ID의 데이터가 테이블에 존재하는지 확인합니다.
        /// 유효하지 않은 ID가 전달되면 false를 반환합니다.
        /// </summary>
        public bool Contains(string id)
        {
            if (!DataIdValidator.IsValid(id))
            {
                return false;
            }

            return entriesById.ContainsKey(id);
        }

        /// <summary>
        /// 지정한 ID의 데이터를 조회합니다.
        /// 유효하지 않거나 존재하지 않는 ID인 경우 false를 반환합니다.
        /// </summary>
        /// <param name="id">조회할 데이터의 ID입니다.</param>
        /// <param name="entry">조회에 성공한 경우 해당 데이터 항목입니다.</param>
        public bool TryGet(string id, out T entry)
        {
            if (!DataIdValidator.IsValid(id))
            {
                entry = default;
                return false;
            }

            return entriesById.TryGetValue(id, out entry);
        }

        /// <summary>
        /// 지정한 ID의 데이터를 조회하고 성공 또는 실패 결과를 반환합니다.
        /// 잘못된 ID와 존재하지 않는 ID는 서로 다른 오류 코드로 구분됩니다.
        /// </summary>
        /// <param name="id">조회할 데이터의 ID입니다.</param>
        /// <returns>조회 성공 시 데이터 항목을 포함하고, 실패 시 오류 정보를 포함하는 결과입니다.</returns>
        public Result<T> Get(string id)
        {
            if (!DataIdValidator.IsValid(id))
            {
                return Result<T>.Failure(new ResultError(DataErrorCodes.InvalidId, $"유효하지 않은 데이터 ID입니다: '{id}'"));
            }

            if (!entriesById.TryGetValue(id, out T entry))
            {
                return Result<T>.Failure(new ResultError(DataErrorCodes.NotFound, $"데이터를 찾을 수 없습니다: '{id}'"));
            }

            return Result<T>.Success(entry);
        }
    }
}