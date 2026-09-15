using System;
using CDG.Core.Results;
using UnityEngine;

namespace CDG.Data.Editor
{
    /// <summary>
    /// Generic Entry 타입을 알지 못하는 EditorWindow와 실제 Generic Import 시스템 사이를 연결하는 계약입니다.
    /// </summary>
    internal interface IDataImportSession
    {
        /// <summary>
        /// 이 Session이 처리하는 IDataEntry 타입입니다.
        /// </summary>
        Type EntryType { get; }

        /// <summary>
        /// Import 대상 DataTableAsset입니다.
        /// </summary>
        UnityEngine.Object TargetAsset { get; }

        /// <summary>
        /// 현재 생성된 Preview가 존재하는지를 나타냅니다.
        /// </summary>
        bool HasPreview { get; }

        /// <summary>
        /// 가장 최근에 성공적으로 생성된 Preview입니다.
        /// Preview가 없으면 null입니다.
        /// </summary>
        DataImportSessionPreview CurrentPreview { get; }

        /// <summary>
        /// 지정한 문자열을 선택한 형식으로 가져와 Validation과 Diff Preview를 생성합니다.
        /// </summary>
        Result<DataImportSessionPreview> CreatePreview(string text, DataImportFormat format);

        /// <summary>
        /// 현재 Preview를 대상 Asset에 적용합니다.
        /// Apply를 시도한 Preview는 성공 여부와 관계없이 폐기됩니다.
        /// </summary>
        Result ApplyPreview();

        /// <summary>
        /// 현재 Session에 보관된 Preview를 제거합니다.
        /// </summary>
        void ClearPreview();
    }
}