using CDG.Core.Results;
using CDG.Data;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// 외부 데이터 가져오기에 전달되는 원본 문자열의 공통 입력 규칙을 검사합니다.
    /// null, 빈 문자열 및 공백만 포함된 문자열은 가져오기 입력으로 허용하지 않습니다.
    /// </summary>
    internal static class DataImportTextValidator
    {
        /// <summary>
        /// 지정한 문자열이 외부 데이터 가져오기 입력으로 사용 가능한지 검사합니다.
        /// 앞뒤 공백을 제거하거나 원본 문자열을 변경하지 않습니다.
        /// </summary>
        internal static Result Validate(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Result.Failure(new ResultError(
                    DataErrorCodes.ImportFailed,
                    "가져올 원본 문자열은 null, 빈 문자열 또는 공백만 포함된 값일 수 없습니다."));
            }

            return Result.Success();
        }
    }
}