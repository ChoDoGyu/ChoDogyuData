using System;
using CDG.Data;

namespace CDG.Data.Editor
{
    /// <summary>
    /// 임의의 Unity Object 또는 타입이 DataTableAsset 계열인지 확인하고
    /// 실제 데이터 항목 타입을 찾기 위한 Editor 전용 유틸리티입니다.
    /// </summary>
    internal static class DataTableAssetTypeUtility
    {
        /// <summary>
        /// 지정한 Unity Object에서 DataTableAsset의 데이터 항목 타입을 찾습니다.
        /// </summary>
        internal static bool TryGetEntryType(UnityEngine.Object target, out Type entryType)
        {
            entryType = null;

            if (target == null)
            {
                return false;
            }

            return TryGetEntryType(target.GetType(), out entryType);
        }

        /// <summary>
        /// 지정한 타입의 상속 계층에서 DataTableAsset&lt;T&gt;를 찾아 T를 반환합니다.
        /// 중간 기반 클래스를 상속한 경우에도 전체 기반 타입을 탐색합니다.
        /// </summary>
        internal static bool TryGetEntryType(Type assetType, out Type entryType)
        {
            if (assetType == null)
            {
                throw new ArgumentNullException(nameof(assetType));
            }

            entryType = null;
            Type currentType = assetType;

            while (currentType != null)
            {
                if (currentType.IsGenericType &&
                    currentType.GetGenericTypeDefinition() == typeof(DataTableAsset<>))
                {
                    entryType = currentType.GetGenericArguments()[0];
                    return true;
                }

                currentType = currentType.BaseType;
            }

            return false;
        }
    }
}