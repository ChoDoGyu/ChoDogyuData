namespace CDG.Data
{
    /// <summary>
    /// Data Framework에서 사용하는 안정적인 오류 코드 모음입니다.
    /// 외부 코드에서는 오류 메시지 대신 오류 코드를 기준으로 실패 원인을 구분할 수 있습니다.
    /// </summary>
    public static class DataErrorCodes
    {
        /// <summary>
        /// 데이터 ID가 유효하지 않을 때 사용하는 오류 코드입니다.
        /// </summary>
        public const string InvalidId = "DATA_INVALID_ID";

        /// <summary>
        /// 요청한 데이터 ID를 찾을 수 없을 때 사용하는 오류 코드입니다.
        /// </summary>
        public const string NotFound = "DATA_NOT_FOUND";

        /// <summary>
        /// 데이터 검증 결과로 테이블을 생성할 수 없을 때 사용하는 오류 코드입니다.
        /// </summary>
        public const string ValidationFailed = "DATA_VALIDATION_FAILED";

        /// <summary>
        /// 외부 데이터 가져오기 작업이 실패했을 때 사용하는 오류 코드입니다.
        /// </summary>
        public const string ImportFailed = "DATA_IMPORT_FAILED";
    }
}