using System;
using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Editor.Importing;
using NUnit.Framework;
using UnityEngine;

namespace CDG.Data.Tests.Editor
{
    public sealed class UnityJsonObjectMapperTests
    {
        [Test]
        public void Map_ValidJson_MapsPrivateSerializedFields()
        {
            UnityJsonObjectMapper<TestEntry> mapper =
                new UnityJsonObjectMapper<TestEntry>();

            JsonObjectSource source = new JsonObjectSource(
                "{\"id\":\"item_001\",\"value\":25,\"enabled\":true}",
                1);

            Result<TestEntry> result = mapper.Map(source);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Id, Is.EqualTo("item_001"));
            Assert.That(result.Value.Value, Is.EqualTo(25));
            Assert.That(result.Value.Enabled, Is.True);
        }

        [Test]
        public void Map_NestedJson_MapsNestedSerializableData()
        {
            UnityJsonObjectMapper<NestedEntry> mapper =
                new UnityJsonObjectMapper<NestedEntry>();

            JsonObjectSource source = new JsonObjectSource(
                "{\"id\":\"item_001\",\"stats\":{\"attack\":35}}",
                1);

            Result<NestedEntry> result = mapper.Map(source);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Id, Is.EqualTo("item_001"));
            Assert.That(result.Value.Attack, Is.EqualTo(35));
        }

        [Test]
        public void Map_NonSerializableEntry_ReturnsImportFailed()
        {
            UnityJsonObjectMapper<NonSerializableEntry> mapper =
                new UnityJsonObjectMapper<NonSerializableEntry>();

            JsonObjectSource source = new JsonObjectSource(
                "{\"id\":\"item_001\"}",
                1);

            Result<NonSerializableEntry> result =
                mapper.Map(source);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Map_NullSource_ThrowsArgumentNullException()
        {
            UnityJsonObjectMapper<TestEntry> mapper =
                new UnityJsonObjectMapper<TestEntry>();

            Assert.Throws<ArgumentNullException>(() =>
                mapper.Map(null));
        }

        [Test]
        public void Map_InheritedPrivateSerializedFields_MapsBaseAndDerivedFields()
        {
            UnityJsonObjectMapper<InheritedEntry> mapper =
                new UnityJsonObjectMapper<InheritedEntry>();

            JsonObjectSource source = new JsonObjectSource(
                "{\"id\":\"item_001\",\"baseName\":\"Base Item\",\"value\":50}",
                1);

            Result<InheritedEntry> result =
                mapper.Map(source);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Id, Is.EqualTo("item_001"));
            Assert.That(result.Value.BaseName, Is.EqualTo("Base Item"));
            Assert.That(result.Value.Value, Is.EqualTo(50));
        }

        [Test]
        public void Map_NestedArrayData_MapsAllElements()
        {
            UnityJsonObjectMapper<ArrayEntry> mapper =
                new UnityJsonObjectMapper<ArrayEntry>();

            JsonObjectSource source = new JsonObjectSource(
                "{\"id\":\"item_001\",\"stats\":[{\"attack\":10},{\"attack\":25}]}",
                1);

            Result<ArrayEntry> result =
                mapper.Map(source);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Id, Is.EqualTo("item_001"));
            Assert.That(result.Value.StatCount, Is.EqualTo(2));
            Assert.That(result.Value.GetAttack(0), Is.EqualTo(10));
            Assert.That(result.Value.GetAttack(1), Is.EqualTo(25));
        }

        [Test]
        public void Map_EscapedUnicodeString_PreservesDecodedValue()
        {
            UnityJsonObjectMapper<TextEntry> mapper =
                new UnityJsonObjectMapper<TextEntry>();

            JsonObjectSource source = new JsonObjectSource(
                "{\"id\":\"item_001\",\"text\":\"한글\\n\\\"quoted\\\"\\\\path\"}",
                1);

            Result<TextEntry> result =
                mapper.Map(source);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Id, Is.EqualTo("item_001"));

            Assert.That(
                result.Value.Text,
                Is.EqualTo("한글\n\"quoted\"\\path"));
        }

        [Test]
        public void Map_MissingSerializedFields_UsesDefaultValues()
        {
            UnityJsonObjectMapper<TestEntry> mapper =
                new UnityJsonObjectMapper<TestEntry>();

            JsonObjectSource source = new JsonObjectSource(
                "{\"id\":\"item_001\"}",
                1);

            Result<TestEntry> result =
                mapper.Map(source);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Id, Is.EqualTo("item_001"));
            Assert.That(result.Value.Value, Is.EqualTo(0));
            Assert.That(result.Value.Enabled, Is.False);
        }

        [Test]
        public void Map_MalformedJson_ReturnsImportFailed()
        {
            UnityJsonObjectMapper<TestEntry> mapper =
                new UnityJsonObjectMapper<TestEntry>();

            JsonObjectSource source = new JsonObjectSource(
                "{",
                1);

            Result<TestEntry> result =
                mapper.Map(source);

            Assert.That(result.IsFailure, Is.True);

            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Serializable]
        private sealed class TestEntry : IDataEntry
        {
            [SerializeField]
            private string id;

            [SerializeField]
            private int value;

            [SerializeField]
            private bool enabled;

            public string Id => id;
            public int Value => value;
            public bool Enabled => enabled;
        }

        [Serializable]
        private sealed class NestedEntry : IDataEntry
        {
            [SerializeField]
            private string id;

            [SerializeField]
            private Stats stats;

            public string Id => id;
            public int Attack => stats.attack;
        }

        [Serializable]
        private abstract class InheritedBaseEntry : IDataEntry
        {
            [SerializeField]
            private string id;

            [SerializeField]
            private string baseName;

            public string Id => id;
            public string BaseName => baseName;
        }

        [Serializable]
        private sealed class InheritedEntry : InheritedBaseEntry
        {
            [SerializeField]
            private int value;

            public int Value => value;
        }

        [Serializable]
        private sealed class ArrayEntry : IDataEntry
        {
            [SerializeField]
            private string id;

            [SerializeField]
            private Stats[] stats;

            public string Id => id;
            public int StatCount => stats?.Length ?? 0;

            public int GetAttack(int index)
            {
                return stats[index].attack;
            }
        }

        [Serializable]
        private sealed class TextEntry : IDataEntry
        {
            [SerializeField]
            private string id;

            [SerializeField]
            private string text;

            public string Id => id;
            public string Text => text;
        }

        [Serializable]
        private struct Stats
        {
            public int attack;
        }

        private sealed class NonSerializableEntry : IDataEntry
        {
            public string Id => null;
        }
    }
}