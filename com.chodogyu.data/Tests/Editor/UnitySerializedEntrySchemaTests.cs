using System;
using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Editor.Importing;
using NUnit.Framework;
using UnityEngine;

namespace CDG.Data.Tests.Editor
{
    public sealed class UnitySerializedEntrySchemaTests
    {
        [Test]
        public void Create_PublicAndSerializeFieldFields_CollectsOnlySerializableFields()
        {
            Result<UnitySerializedEntrySchema> result =
                UnitySerializedEntrySchema.Create(typeof(TestEntry));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(2));

            Assert.That(
                result.Value.TryGetField("id", out _),
                Is.True);

            Assert.That(
                result.Value.TryGetField("value", out _),
                Is.True);

            Assert.That(
                result.Value.TryGetField("ignored", out _),
                Is.False);

            Assert.That(
                result.Value.TryGetField("runtimeOnly", out _),
                Is.False);
        }

        [Test]
        public void Create_InheritedPrivateSerializeField_IncludesBaseField()
        {
            Result<UnitySerializedEntrySchema> result =
                UnitySerializedEntrySchema.Create(
                    typeof(DerivedEntry));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(2));
            Assert.That(result.Value.TryGetField("id", out _), Is.True);
            Assert.That(result.Value.TryGetField("value", out _), Is.True);
        }

        [Test]
        public void Create_NonDataEntry_ReturnsImportFailed()
        {
            Result<UnitySerializedEntrySchema> result =
                UnitySerializedEntrySchema.Create(
                    typeof(PlainSerializableType));

            Assert.That(result.IsFailure, Is.True);
            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Create_AbstractEntry_ReturnsImportFailed()
        {
            Result<UnitySerializedEntrySchema> result =
                UnitySerializedEntrySchema.Create(
                    typeof(AbstractEntry));

            Assert.That(result.IsFailure, Is.True);
            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Create_UnityObjectEntry_ReturnsImportFailed()
        {
            Result<UnitySerializedEntrySchema> result =
                UnitySerializedEntrySchema.Create(
                    typeof(UnityObjectEntry));

            Assert.That(result.IsFailure, Is.True);
            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Create_NonSerializableEntry_ReturnsImportFailed()
        {
            Result<UnitySerializedEntrySchema> result =
                UnitySerializedEntrySchema.Create(
                    typeof(NonSerializableEntry));

            Assert.That(result.IsFailure, Is.True);
            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Create_NullType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                UnitySerializedEntrySchema.Create(null));
        }

        [Serializable]
        private sealed class TestEntry : IDataEntry
        {
            public string id;

            [SerializeField]
            private int value;

            private int ignored;

            [NonSerialized]
            public int runtimeOnly;

            public string Id => id;
        }

        [Serializable]
        private abstract class BaseEntry : IDataEntry
        {
            [SerializeField]
            private string id;

            public string Id => id;
        }

        [Serializable]
        private sealed class DerivedEntry : BaseEntry
        {
            [SerializeField]
            private int value;
        }

        [Serializable]
        private sealed class PlainSerializableType
        {
            public string value;
        }

        [Serializable]
        private abstract class AbstractEntry : IDataEntry
        {
            [SerializeField]
            private string id;

            public string Id => id;
        }

        private sealed class UnityObjectEntry : ScriptableObject, IDataEntry
        {
            public string Id => "unity";
        }

        private sealed class NonSerializableEntry : IDataEntry
        {
            public string Id => "non_serializable";
        }
    }
}