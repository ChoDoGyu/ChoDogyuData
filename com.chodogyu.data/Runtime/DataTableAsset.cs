using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CDG.Core.Results;
using CDG.Data.Validation;
using UnityEngine;

namespace CDG.Data
{
    /// <summary>
    /// Unity Asset으로 저장할 수 있는 정적 데이터 테이블의 기반 클래스입니다.
    /// 직렬화된 데이터 항목을 검증하거나 런타임용 <see cref="DataTable{T}"/>로 생성할 수 있습니다.
    /// </summary>
    /// <typeparam name="T">Asset에 저장할 데이터 항목 타입입니다.</typeparam>
    public abstract class DataTableAsset<T> : ScriptableObject where T : IDataEntry
    {
        [SerializeField]
        private List<T> entries = new List<T>();

        private List<T> readOnlyEntriesSource;
        private ReadOnlyCollection<T> readOnlyEntries;

        /// <summary>
        /// Asset에 저장된 데이터 항목 수입니다.
        /// </summary>
        public int Count => entries.Count;

        /// <summary>
        /// Asset에 저장된 데이터 항목을 직렬화된 순서대로 제공합니다.
        /// 반환된 컬렉션을 통해 항목을 추가, 제거 또는 교체할 수 없습니다.
        /// 내부 직렬화 목록이 교체된 경우에는 최신 목록을 기준으로 읽기 전용 뷰를 다시 생성합니다.
        /// </summary>
        public IReadOnlyList<T> Entries
        {
            get
            {
                if (!ReferenceEquals(readOnlyEntriesSource, entries))
                {
                    readOnlyEntriesSource = entries;
                    readOnlyEntries = entries.AsReadOnly();
                }

                return readOnlyEntries;
            }
        }

        /// <summary>
        /// 현재 Asset에 저장된 데이터 항목 전체를 검증합니다.
        /// 검증 과정에서는 Asset의 데이터를 수정하지 않습니다.
        /// </summary>
        /// <returns>발견된 모든 문제를 포함하는 검증 결과입니다.</returns>
        public DataValidationReport Validate()
        {
            return DataTableValidator.Validate(entries);
        }

        /// <summary>
        /// 현재 Asset에 저장된 데이터를 사용하여 런타임용 읽기 전용 데이터 테이블을 생성합니다.
        /// 유효하지 않은 데이터가 포함된 경우 실패 결과를 반환합니다.
        /// </summary>
        /// <returns>생성된 데이터 테이블 또는 검증 실패 정보를 포함하는 결과입니다.</returns>
        public Result<DataTable<T>> Build()
        {
            DataValidationReport report = Validate();

            if (!report.IsValid)
            {
                return Result<DataTable<T>>.Failure(new ResultError(
                    DataErrorCodes.ValidationFailed,
                    $"데이터 테이블 Asset 검증에 실패했습니다. 발견된 문제 수: {report.Count}"));
            }

            return DataTable<T>.Create(entries);
        }

        /// <summary>
        /// 현재 직렬화된 데이터 전체를 지정한 데이터로 교체합니다.
        /// 새 데이터는 먼저 독립된 스냅샷으로 생성하고 검증하며,
        /// 검증에 실패하면 기존 Asset 데이터는 변경하지 않습니다.
        /// </summary>
        /// <param name="source">Asset에 새로 저장할 데이터 항목입니다.</param>
        /// <returns>교체 성공 여부 또는 검증 실패 정보를 포함하는 결과입니다.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/>가 null인 경우 발생합니다.
        /// </exception>
        internal Result ReplaceEntries(IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            List<T> snapshot = new List<T>(source);
            DataValidationReport report = DataTableValidator.Validate(snapshot);

            if (!report.IsValid)
            {
                return Result.Failure(new ResultError(
                    DataErrorCodes.ValidationFailed,
                    $"데이터 테이블 Asset 교체 데이터 검증에 실패했습니다. 발견된 문제 수: {report.Count}"));
            }

            entries = snapshot;
            readOnlyEntriesSource = null;
            readOnlyEntries = null;

            return Result.Success();
        }
    }
}