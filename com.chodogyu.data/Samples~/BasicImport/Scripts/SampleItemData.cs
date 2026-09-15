using System;
using CDG.Data;
using UnityEngine;

namespace CDG.Data.Samples.BasicImport
{
    /// <summary>
    /// Basic Import Sample에서 사용하는 간단한 아이템 데이터입니다.
    /// CSV와 JSON 자동 매핑 예제를 위해 문자열, 정수 및 Enum 필드를 포함합니다.
    /// </summary>
    [Serializable]
    public sealed class SampleItemData : IDataEntry
    {
        [SerializeField]
        private string id;

        [SerializeField]
        private string displayName;

        [SerializeField]
        private int power;

        [SerializeField]
        private SampleItemRarity rarity;

        /// <summary>
        /// 데이터 항목을 식별하는 고유 ID입니다.
        /// </summary>
        public string Id => id;

        /// <summary>
        /// 화면에 표시할 아이템 이름입니다.
        /// </summary>
        public string DisplayName => displayName;

        /// <summary>
        /// 예제에서 사용하는 아이템 능력치입니다.
        /// </summary>
        public int Power => power;

        /// <summary>
        /// 아이템의 예제 등급입니다.
        /// </summary>
        public SampleItemRarity Rarity => rarity;
    }

    /// <summary>
    /// Basic Import Sample에서 사용하는 아이템 등급입니다.
    /// </summary>
    public enum SampleItemRarity
    {
        Common = 0,
        Rare = 1,
        Epic = 2
    }
}