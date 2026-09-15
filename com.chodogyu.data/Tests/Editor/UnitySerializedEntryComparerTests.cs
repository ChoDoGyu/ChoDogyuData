using System;
using CDG.Data;
using CDG.Data.Editor.Importing;
using NUnit.Framework;
using UnityEngine;

namespace CDG.Data.Tests.Editor
{
    public sealed class UnitySerializedEntryComparerTests
    {
        [Test]
        public void AreEquivalent_SameSerializedValues_ReturnsTrue()
        {
            TestEntry current = new TestEntry(
                "item_001",
                10,
                100);

            TestEntry incoming = new TestEntry(
                "item_001",
                10,
                100);

            UnitySerializedEntryComparer<TestEntry> comparer =
                new UnitySerializedEntryComparer<TestEntry>();

            bool result = comparer.AreEquivalent(
                current,
                incoming);

            Assert.That(result, Is.True);
        }

        [Test]
        public void AreEquivalent_DifferentSerializedValue_ReturnsFalse()
        {
            TestEntry current = new TestEntry(
                "item_001",
                10,
                100);

            TestEntry incoming = new TestEntry(
                "item_001",
                20,
                100);

            UnitySerializedEntryComparer<TestEntry> comparer =
                new UnitySerializedEntryComparer<TestEntry>();

            bool result = comparer.AreEquivalent(
                current,
                incoming);

            Assert.That(result, Is.False);
        }

        [Test]
        public void AreEquivalent_DifferentNonSerializedValue_ReturnsTrue()
        {
            TestEntry current = new TestEntry(
                "item_001",
                10,
                100);

            TestEntry incoming = new TestEntry(
                "item_001",
                10,
                999);

            UnitySerializedEntryComparer<TestEntry> comparer =
                new UnitySerializedEntryComparer<TestEntry>();

            bool result = comparer.AreEquivalent(
                current,
                incoming);

            Assert.That(result, Is.True);
        }

        [Serializable]
        private sealed class TestEntry : IDataEntry
        {
            [SerializeField]
            private string id;

            [SerializeField]
            private int value;

            [NonSerialized]
            private int runtimeValue;

            public string Id => id;

            public TestEntry(string id, int value, int runtimeValue)
            {
                this.id = id;
                this.value = value;
                this.runtimeValue = runtimeValue;
            }
        }
    }
}