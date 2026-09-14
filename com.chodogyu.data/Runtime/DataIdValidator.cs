namespace CDG.Data
{
    /// <summary>
    /// Data Framework에서 사용하는 문자열 ID의 기본 유효성 규칙을 검사합니다.
    /// ID는 비어 있지 않아야 하며 앞뒤에 공백을 포함할 수 없습니다.
    /// </summary>
    internal static class DataIdValidator
    {
        /// <summary>
        /// 지정한 ID가 Data Framework의 기본 ID 규칙을 만족하는지 확인합니다.
        /// </summary>
        internal static bool IsValid(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            return id == id.Trim();
        }
    }
}