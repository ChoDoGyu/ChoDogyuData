using System;
using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Editor.Importing;
using UnityEngine;

namespace CDG.Data.Editor
{
    /// <summary>
    /// 하나의 DataTableAsset과 Entry 타입에 대해 CSV/JSON Import Preview 및 Apply 흐름을 관리합니다.
    /// EditorWindow는 이 Generic 타입을 직접 다루지 않고 IDataImportSession을 통해 사용합니다.
    /// </summary>
    /// <typeparam name="T">Import 대상 데이터 항목 타입입니다.</typeparam>
    internal sealed class DataImportSession<T> : IDataImportSession where T : IDataEntry
    {
        private readonly DataTableAsset<T> target;
        private readonly CsvDataImporter<T> csvImporter;
        private readonly JsonDataImporter<T> jsonImporter;
        private readonly UnitySerializedEntryComparer<T> comparer;

        private DataImportPreview<T> typedPreview;
        private DataImportSessionPreview currentPreview;

        /// <summary>
        /// 이 Session이 처리하는 데이터 항목 타입입니다.
        /// </summary>
        public Type EntryType => typeof(T);

        /// <summary>
        /// Import 대상 DataTableAsset입니다.
        /// </summary>
        public UnityEngine.Object TargetAsset => target;

        /// <summary>
        /// 현재 생성된 Preview가 존재하는지를 나타냅니다.
        /// </summary>
        public bool HasPreview => currentPreview != null;

        /// <summary>
        /// 가장 최근에 생성된 비제네릭 Preview입니다.
        /// </summary>
        public DataImportSessionPreview CurrentPreview => currentPreview;

        internal DataImportPreview<T> TypedPreview => typedPreview;

        internal DataImportSession(DataTableAsset<T> target)
        {
            this.target = target != null
                ? target
                : throw new ArgumentNullException(nameof(target));

            csvImporter = new CsvDataImporter<T>(
                new UnityCsvRowMapper<T>());

            jsonImporter = new JsonDataImporter<T>(
                new UnityJsonObjectMapper<T>());

            comparer = new UnitySerializedEntryComparer<T>();
        }

        /// <summary>
        /// 지정한 외부 문자열을 가져와 현재 Target에 대한 Preview를 생성합니다.
        /// 새로운 Preview 생성 시 기존 Preview는 먼저 제거됩니다.
        /// </summary>
        public Result<DataImportSessionPreview> CreatePreview(string text, DataImportFormat format)
        {
            ClearPreview();

            IDataTextImporter<T> importer;

            switch (format)
            {
                case DataImportFormat.Csv:
                    importer = csvImporter;
                    break;

                case DataImportFormat.Json:
                    importer = jsonImporter;
                    break;

                default:
                    return Result<DataImportSessionPreview>.Failure(
                        new ResultError(
                            DataErrorCodes.ImportFailed,
                            $"지원하지 않는 Import 형식입니다: {format}"));
            }

            Result<DataImportPreview<T>> result =
                DataImportProcessor.Import(
                    text,
                    importer,
                    target,
                    comparer);

            if (result.IsFailure)
            {
                return Result<DataImportSessionPreview>.Failure(
                    result.Error);
            }

            typedPreview = result.Value;
            currentPreview =
                DataImportSessionPreview.Create(typedPreview);

            return Result<DataImportSessionPreview>.Success(
                currentPreview);
        }

        /// <summary>
        /// 현재 Preview의 후보 데이터를 대상 Asset에 적용합니다.
        /// Apply 시도가 끝난 Preview는 성공 여부와 관계없이 다시 사용할 수 없도록 제거합니다.
        /// </summary>
        public Result ApplyPreview()
        {
            if (typedPreview == null)
            {
                return Result.Failure(new ResultError(
                    DataErrorCodes.ImportFailed,
                    "적용할 Import Preview가 없습니다."));
            }

            Result result;

            try
            {
                result = DataImportApplier.Apply(
                    target,
                    typedPreview);
            }
            finally
            {
                ClearPreview();
            }

            return result;
        }

        /// <summary>
        /// 현재 Session에 보관된 Preview를 제거합니다.
        /// Target Asset 자체는 수정하지 않습니다.
        /// </summary>
        public void ClearPreview()
        {
            typedPreview = null;
            currentPreview = null;
        }
    }
}