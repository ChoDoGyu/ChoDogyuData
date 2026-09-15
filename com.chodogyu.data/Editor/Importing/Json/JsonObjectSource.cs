using System;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// JSON 루트 배열에서 분리된 하나의 Object 원본을 나타냅니다.
    /// Object 문자열은 파싱 과정에서 내용을 변경하지 않고 그대로 보관합니다.
    /// </summary>
    internal sealed class JsonObjectSource
    {
        /// <summary>
        /// JSON Object의 원본 문자열입니다.
        /// 여는 중괄호와 닫는 중괄호를 포함합니다.
        /// </summary>
        internal string Text { get; }

        /// <summary>
        /// 원본 JSON 문자열에서 이 Object가 시작된 줄 번호입니다.
        /// 첫 번째 줄은 1입니다.
        /// </summary>
        internal int StartLine { get; }

        internal JsonObjectSource(string text, int startLine)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));

            if (startLine < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(startLine));
            }

            StartLine = startLine;
        }
    }
}