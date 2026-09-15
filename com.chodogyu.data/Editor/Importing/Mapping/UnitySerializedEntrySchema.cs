using System;
using System.Collections.Generic;
using System.Reflection;
using CDG.Core.Results;
using CDG.Data;
using UnityEngine;

namespace CDG.Data.Editor.Importing
{
    /// <summary>
    /// Unity 직렬화 규칙에 따라 데이터 항목 타입에서 Import 대상 필드를 수집한 Schema입니다.
    /// 상속된 private SerializeField도 포함하며 필드 이름은 대소문자를 구분합니다.
    /// </summary>
    internal sealed class UnitySerializedEntrySchema
    {
        private readonly FieldInfo[] fields;
        private readonly IReadOnlyList<FieldInfo> readOnlyFields;
        private readonly Dictionary<string, FieldInfo> fieldsByName;

        internal int Count => fields.Length;
        internal IReadOnlyList<FieldInfo> Fields => readOnlyFields;

        private UnitySerializedEntrySchema(IEnumerable<FieldInfo> fields)
        {
            List<FieldInfo> snapshot = new List<FieldInfo>(fields);

            this.fields = snapshot.ToArray();
            readOnlyFields = Array.AsReadOnly(this.fields);

            fieldsByName = new Dictionary<string, FieldInfo>(StringComparer.Ordinal);

            for (int index = 0; index < this.fields.Length; index++)
            {
                FieldInfo field = this.fields[index];
                fieldsByName.Add(field.Name, field);
            }
        }

        /// <summary>
        /// 지정한 이름의 직렬화 필드를 조회합니다.
        /// 필드 이름 비교는 대소문자를 구분합니다.
        /// </summary>
        internal bool TryGetField(string fieldName, out FieldInfo field)
        {
            if (fieldName == null)
            {
                field = null;
                return false;
            }

            return fieldsByName.TryGetValue(fieldName, out field);
        }

        /// <summary>
        /// 지정한 IDataEntry 타입의 Unity 직렬화 필드를 분석하여 Schema를 생성합니다.
        /// </summary>
        internal static Result<UnitySerializedEntrySchema> Create(Type entryType)
        {
            if (entryType == null)
            {
                throw new ArgumentNullException(nameof(entryType));
            }

            if (!typeof(IDataEntry).IsAssignableFrom(entryType))
            {
                return CreateFailure(
                    $"'{entryType.FullName}' 타입은 IDataEntry를 구현하지 않습니다.");
            }

            if (entryType.IsInterface || entryType.IsAbstract)
            {
                return CreateFailure(
                    $"'{entryType.FullName}' 타입은 직접 생성할 수 없는 타입입니다.");
            }

            if (entryType.ContainsGenericParameters)
            {
                return CreateFailure(
                    $"'{entryType.FullName}' 타입에는 닫히지 않은 Generic 매개변수가 존재합니다.");
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(entryType))
            {
                return CreateFailure(
                    $"'{entryType.FullName}' 타입은 UnityEngine.Object를 상속하므로 일반 데이터 항목으로 자동 매핑할 수 없습니다.");
            }

            if (!Attribute.IsDefined(entryType, typeof(SerializableAttribute), false))
            {
                return CreateFailure(
                    $"'{entryType.FullName}' 타입에는 Serializable 특성이 필요합니다.");
            }

            List<Type> hierarchy = new List<Type>();
            Type currentType = entryType;

            while (currentType != null &&
                   currentType != typeof(object) &&
                   currentType != typeof(ValueType))
            {
                hierarchy.Add(currentType);
                currentType = currentType.BaseType;
            }

            hierarchy.Reverse();

            List<FieldInfo> serializedFields = new List<FieldInfo>();
            HashSet<string> fieldNames = new HashSet<string>(StringComparer.Ordinal);

            for (int typeIndex = 0; typeIndex < hierarchy.Count; typeIndex++)
            {
                FieldInfo[] declaredFields = hierarchy[typeIndex].GetFields(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly);

                Array.Sort(
                    declaredFields,
                    (left, right) => left.MetadataToken.CompareTo(right.MetadataToken));

                for (int fieldIndex = 0; fieldIndex < declaredFields.Length; fieldIndex++)
                {
                    FieldInfo field = declaredFields[fieldIndex];

                    if (!IsUnitySerializedField(field))
                    {
                        continue;
                    }

                    if (!fieldNames.Add(field.Name))
                    {
                        return CreateFailure(
                            $"'{entryType.FullName}' 타입에 중복된 직렬화 필드 이름이 존재합니다: '{field.Name}'");
                    }

                    serializedFields.Add(field);
                }
            }

            return Result<UnitySerializedEntrySchema>.Success(
                new UnitySerializedEntrySchema(serializedFields));
        }

        private static bool IsUnitySerializedField(FieldInfo field)
        {
            if (field.IsStatic ||
                field.IsLiteral ||
                field.IsInitOnly ||
                field.IsNotSerialized)
            {
                return false;
            }

            if (field.IsPublic)
            {
                return true;
            }

            if (field.GetCustomAttribute<SerializeField>() != null)
            {
                return true;
            }

            return field.GetCustomAttribute<SerializeReference>() != null;
        }

        private static Result<UnitySerializedEntrySchema> CreateFailure(string message)
        {
            return Result<UnitySerializedEntrySchema>.Failure(new ResultError(
                DataErrorCodes.ImportFailed,
                message));
        }
    }
}