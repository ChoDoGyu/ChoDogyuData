using System;
using System.Collections.Generic;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// JSON 루트 배열에서 분리된 Object들을 원본 순서대로 보관하는 읽기 전용 문서입니다.
    /// </summary>
    internal sealed class JsonDocument
    {
        private readonly JsonObjectSource[] objects;
        private readonly IReadOnlyList<JsonObjectSource> readOnlyObjects;

        /// <summary>
        /// 문서에 포함된 JSON Object 수입니다.
        /// </summary>
        internal int Count => objects.Length;

        /// <summary>
        /// JSON Object들을 원본 배열 순서대로 제공합니다.
        /// </summary>
        internal IReadOnlyList<JsonObjectSource> Objects => readOnlyObjects;

        /// <summary>
        /// 지정한 위치의 JSON Object를 반환합니다.
        /// </summary>
        internal JsonObjectSource this[int index] => objects[index];

        internal JsonDocument(IEnumerable<JsonObjectSource> objects)
        {
            if (objects == null)
            {
                throw new ArgumentNullException(nameof(objects));
            }

            List<JsonObjectSource> snapshot = new List<JsonObjectSource>(objects);

            this.objects = snapshot.ToArray();
            readOnlyObjects = Array.AsReadOnly(this.objects);
        }
    }
}