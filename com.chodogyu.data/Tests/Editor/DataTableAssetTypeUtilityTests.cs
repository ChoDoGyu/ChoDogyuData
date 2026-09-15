using System;
using CDG.Data;
using CDG.Data.Editor;
using NUnit.Framework;
using UnityEngine;

namespace CDG.Data.Tests.Editor
{
    public sealed class DataTableAssetTypeUtilityTests
    {
        [Test]
        public void TryGetEntryType_DirectDataTableAsset_ReturnsEntryType()
        {
            bool result = DataTableAssetTypeUtility.TryGetEntryType(
                typeof(DirectTestAsset),
                out Type entryType);

            Assert.That(result, Is.True);
            Assert.That(entryType, Is.EqualTo(typeof(TestEntry)));
        }

        [Test]
        public void TryGetEntryType_IndirectDataTableAsset_ReturnsEntryType()
        {
            bool result = DataTableAssetTypeUtility.TryGetEntryType(
                typeof(IndirectTestAsset),
                out Type entryType);

            Assert.That(result, Is.True);
            Assert.That(entryType, Is.EqualTo(typeof(TestEntry)));
        }

        [Test]
        public void TryGetEntryType_NonDataTableAsset_ReturnsFalse()
        {
            bool result = DataTableAssetTypeUtility.TryGetEntryType(
                typeof(PlainScriptableObject),
                out Type entryType);

            Assert.That(result, Is.False);
            Assert.That(entryType, Is.Null);
        }

        [Test]
        public void TryGetEntryType_NullObject_ReturnsFalse()
        {
            bool result = DataTableAssetTypeUtility.TryGetEntryType(
                (UnityEngine.Object)null,
                out Type entryType);

            Assert.That(result, Is.False);
            Assert.That(entryType, Is.Null);
        }

        [Test]
        public void TryGetEntryType_NullType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                DataTableAssetTypeUtility.TryGetEntryType(
                    (Type)null,
                    out _));
        }

        [Serializable]
        private sealed class TestEntry : IDataEntry
        {
            [SerializeField]
            private string id;

            public string Id => id;
        }

        private sealed class DirectTestAsset : DataTableAsset<TestEntry>
        {
        }

        private abstract class IntermediateTestAsset<T> : DataTableAsset<T> where T : IDataEntry
        {
        }

        private sealed class IndirectTestAsset : IntermediateTestAsset<TestEntry>
        {
        }

        private sealed class PlainScriptableObject : ScriptableObject
        {
        }
    }
}