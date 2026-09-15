using CDG.Data;
using UnityEngine;

namespace CDG.Data.Samples.BasicImport
{
    /// <summary>
    /// SampleItemData를 저장하는 Basic Import Sample용 DataTableAsset입니다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SampleItemTable",
        menuName = "CDG Data Samples/Sample Item Table")]
    public sealed class SampleItemTableAsset : DataTableAsset<SampleItemData>
    {
    }
}