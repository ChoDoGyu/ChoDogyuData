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