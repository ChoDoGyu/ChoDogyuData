using System;
using System.IO;
using CDG.Core.Results;
using CDG.Data;

namespace CDG.Data.Editor
{
    /// <summary>
    /// Data Framework Editor에서 선택한 CSV 또는 JSON 원본 파일을 읽습니다.
    /// 선택한 Import 형식과 파일 확장자가 일치하는지 확인하며 파일 내용은 변경하지 않습니다.
    /// </summary>
    internal static class DataImportSourceFileReader
    {
        /// <summary>
        /// 지정한 파일의 전체 문자열을 원본 그대로 읽습니다.
        /// </summary>
        internal static Result<string> Read(string path, DataImportFormat format)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return CreateFailure(
                    "가져올 데이터 파일 경로가 비어 있습니다.");
            }

            string expectedExtension;

            switch (format)
            {
                case DataImportFormat.Csv:
                    expectedExtension = ".csv";
                    break;

                case DataImportFormat.Json:
                    expectedExtension = ".json";
                    break;

                default:
                    return CreateFailure(
                        $"지원하지 않는 Import 형식입니다: {format}");
            }

            try
            {
                string actualExtension = Path.GetExtension(path);

                if (!string.Equals(
                    actualExtension,
                    expectedExtension,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return CreateFailure(
                        $"선택한 Import 형식에는 '{expectedExtension}' 파일이 필요합니다. 현재 파일: '{actualExtension}'");
                }

                if (!File.Exists(path))
                {
                    return CreateFailure(
                        $"데이터 파일을 찾을 수 없습니다: '{path}'");
                }

                string text = File.ReadAllText(path);

                return Result<string>.Success(text);
            }
            catch (ArgumentException exception)
            {
                return CreateFailure(
                    $"파일 경로를 처리할 수 없습니다. {exception.Message}");
            }
            catch (NotSupportedException exception)
            {
                return CreateFailure(
                    $"지원하지 않는 파일 경로입니다. {exception.Message}");
            }
            catch (UnauthorizedAccessException exception)
            {
                return CreateFailure(
                    $"데이터 파일에 접근할 권한이 없습니다. {exception.Message}");
            }
            catch (IOException exception)
            {
                return CreateFailure(
                    $"데이터 파일을 읽는 중 오류가 발생했습니다. {exception.Message}");
            }
        }

        private static Result<string> CreateFailure(string message)
        {
            return Result<string>.Failure(new ResultError(
                DataErrorCodes.ImportFailed,
                message));
        }
    }
}