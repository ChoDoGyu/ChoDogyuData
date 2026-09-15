using CDG.Core.Results;
using CDG.Data;
using UnityEngine;

namespace CDG.Data.Samples.BasicImport
{
    /// <summary>
    /// SampleItemTableAsset을 런타임용 DataTable로 생성하고
    /// Contains, TryGet 및 Get을 사용하여 ID 기반 조회를 수행하는 예제입니다.
    /// </summary>
    public sealed class SampleItemLookupExample : MonoBehaviour
    {
        [SerializeField]
        private SampleItemTableAsset tableAsset;

        [SerializeField]
        private string lookupId = "sword_knight";

        private void Start()
        {
            if (tableAsset == null)
            {
                Debug.LogError(
                    "[CDG Data Sample] SampleItemTableAsset이 지정되지 않았습니다.",
                    this);

                return;
            }

            Result<DataTable<SampleItemData>> buildResult =
                tableAsset.Build();

            if (buildResult.IsFailure)
            {
                Debug.LogError(
                    $"[CDG Data Sample] DataTable 생성 실패: {buildResult.Error.Code} - {buildResult.Error.Message}",
                    this);

                return;
            }

            DataTable<SampleItemData> table =
                buildResult.Value;

            Debug.Log(
                $"[CDG Data Sample] DataTable 생성 완료. Count: {table.Count}",
                this);

            bool contains =
                table.Contains(lookupId);

            Debug.Log(
                $"[CDG Data Sample] Contains('{lookupId}'): {contains}",
                this);

            if (table.TryGet(
                lookupId,
                out SampleItemData tryGetItem))
            {
                Debug.Log(
                    $"[CDG Data Sample] TryGet 성공 - ID: {tryGetItem.Id}, Name: {tryGetItem.DisplayName}, Power: {tryGetItem.Power}, Rarity: {tryGetItem.Rarity}",
                    this);
            }
            else
            {
                Debug.LogWarning(
                    $"[CDG Data Sample] TryGet 실패 - ID: {lookupId}",
                    this);
            }

            Result<SampleItemData> getResult =
                table.Get(lookupId);

            if (getResult.IsSuccess)
            {
                SampleItemData item =
                    getResult.Value;

                Debug.Log(
                    $"[CDG Data Sample] Get 성공 - ID: {item.Id}, Name: {item.DisplayName}, Power: {item.Power}, Rarity: {item.Rarity}",
                    this);

                return;
            }

            Debug.LogWarning(
                $"[CDG Data Sample] Get 실패: {getResult.Error.Code} - {getResult.Error.Message}",
                this);
        }
    }
}