using System;
using System.Collections.Generic;
using CDG.Core.Results;
using NUnit.Framework;
using UnityEngine;

namespace CDG.Data.Tests.Runtime
{
    public sealed class DataTableAssetMutationTests
    {
        private readonly List<TestDataTableAsset> createdAssets = new List<TestDataTableAsset>();

        [TearDown]
        public void TearDown()
        {
            foreach (TestDataTableAsset asset in createdAssets)
            {
                if (asset != null)
                {
                    UnityEngine.Object.DestroyImmediate(asset);
                }
            }

            createdAssets.Clear();
        }

        [Test]
        public void ReplaceEntries_ValidSource_ReplacesAllEntriesInOrder()
        {
            TestDataTableAsset asset = CreateAsset();

            TestEntry first = new TestEntry("item_001");
            TestEntry second = new TestEntry("item_002");

            Result result = asset.ReplaceEntries(new[]
            {
                first,
                second
            });

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(asset.Count, Is.EqualTo(2));
            Assert.That(asset.Entries[0], Is.SameAs(first));
            Assert.That(asset.Entries[1], Is.SameAs(second));
        }

        [Test]
        public void ReplaceEntries_EmptySource_ClearsEntries()
        {
            TestDataTableAsset asset = CreateAsset();

            Result initialResult = asset.ReplaceEntries(new[]
            {
                new TestEntry("item_001")
            });

            Assert.That(initialResult.IsSuccess, Is.True);
            Assert.That(asset.Count, Is.EqualTo(1));

            Result result = asset.ReplaceEntries(Array.Empty<TestEntry>());

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(asset.Count, Is.EqualTo(0));
            Assert.That(asset.Entries, Is.Empty);
        }

        [Test]
        public void ReplaceEntries_InvalidSource_ReturnsValidationFailedAndPreservesExistingEntries()
        {
            TestDataTableAsset asset = CreateAsset();
            TestEntry original = new TestEntry("item_original");

            Result initialResult = asset.ReplaceEntries(new[]
            {
                original
            });

            Assert.That(initialResult.IsSuccess, Is.True);

            Result result = asset.ReplaceEntries(new[]
            {
                new TestEntry("item_001"),
                new TestEntry("item_001")
            });

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ValidationFailed));

            Assert.That(asset.Count, Is.EqualTo(1));
            Assert.That(asset.Entries[0], Is.SameAs(original));
        }

        [Test]
        public void ReplaceEntries_NullSource_ThrowsArgumentNullException()
        {
            TestDataTableAsset asset = CreateAsset();

            Assert.Throws<ArgumentNullException>(() =>
                asset.ReplaceEntries(null));
        }

        [Test]
        public void ReplaceEntries_CreatesSnapshotIndependentFromSourceChanges()
        {
            TestDataTableAsset asset = CreateAsset();
            TestEntry original = new TestEntry("item_001");

            List<TestEntry> source = new List<TestEntry>
            {
                original
            };

            Result result = asset.ReplaceEntries(source);

            Assert.That(result.IsSuccess, Is.True);

            source[0] = new TestEntry("item_999");
            source.Add(new TestEntry("item_002"));

            Assert.That(asset.Count, Is.EqualTo(1));
            Assert.That(asset.Entries[0], Is.SameAs(original));
        }

        [Test]
        public void ReplaceEntries_AfterEntriesAccess_RebuildsReadOnlyView()
        {
            TestDataTableAsset asset = CreateAsset();

            TestEntry first = new TestEntry("item_001");

            Result firstResult = asset.ReplaceEntries(new[]
            {
                first
            });

            Assert.That(firstResult.IsSuccess, Is.True);

            IReadOnlyList<TestEntry> firstView = asset.Entries;

            TestEntry second = new TestEntry("item_002");

            Result secondResult = asset.ReplaceEntries(new[]
            {
                second
            });

            Assert.That(secondResult.IsSuccess, Is.True);

            IReadOnlyList<TestEntry> secondView = asset.Entries;

            Assert.That(secondView, Is.Not.SameAs(firstView));

            Assert.That(firstView.Count, Is.EqualTo(1));
            Assert.That(firstView[0], Is.SameAs(first));

            Assert.That(secondView.Count, Is.EqualTo(1));
            Assert.That(secondView[0], Is.SameAs(second));
        }

        private TestDataTableAsset CreateAsset()
        {
            TestDataTableAsset asset = ScriptableObject.CreateInstance<TestDataTableAsset>();
            createdAssets.Add(asset);
            return asset;
        }

        [Serializable]
        private sealed class TestEntry : IDataEntry
        {
            [SerializeField]
            private string id;

            public string Id => id;

            public TestEntry(string id)
            {
                this.id = id;
            }
        }

        private sealed class TestDataTableAsset : DataTableAsset<TestEntry>
        {
        }
    }
}