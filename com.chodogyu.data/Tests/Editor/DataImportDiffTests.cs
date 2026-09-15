using System;
using System.Collections.Generic;
using CDG.Data;
using CDG.Data.Editor.Importing;
using NUnit.Framework;

namespace CDG.Data.Tests.Editor
{
    public sealed class DataImportDiffTests
    {
        [Test]
        public void Constructor_MixedItems_CalculatesCounts()
        {
            DataImportDiff<TestEntry> diff = new DataImportDiff<TestEntry>(new[]
            {
                CreateItem("item_001", DataImportDiffType.Added),
                CreateItem("item_002", DataImportDiffType.Removed),
                CreateItem("item_003", DataImportDiffType.Modified),
                CreateItem("item_004", DataImportDiffType.Unchanged)
            });

            Assert.That(diff.Count, Is.EqualTo(4));
            Assert.That(diff.AddedCount, Is.EqualTo(1));
            Assert.That(diff.RemovedCount, Is.EqualTo(1));
            Assert.That(diff.ModifiedCount, Is.EqualTo(1));
            Assert.That(diff.UnchangedCount, Is.EqualTo(1));
            Assert.That(diff.HasChanges, Is.True);
        }

        [Test]
        public void Constructor_OnlyUnchangedItems_HasChangesIsFalse()
        {
            DataImportDiff<TestEntry> diff = new DataImportDiff<TestEntry>(new[]
            {
                CreateItem("item_001", DataImportDiffType.Unchanged),
                CreateItem("item_002", DataImportDiffType.Unchanged)
            });

            Assert.That(diff.HasChanges, Is.False);
            Assert.That(diff.UnchangedCount, Is.EqualTo(2));
        }

        [Test]
        public void Constructor_EmptyItems_ReturnsEmptyDiff()
        {
            DataImportDiff<TestEntry> diff =
                new DataImportDiff<TestEntry>(Array.Empty<DataImportDiffItem<TestEntry>>());

            Assert.That(diff.Count, Is.EqualTo(0));
            Assert.That(diff.AddedCount, Is.EqualTo(0));
            Assert.That(diff.RemovedCount, Is.EqualTo(0));
            Assert.That(diff.ModifiedCount, Is.EqualTo(0));
            Assert.That(diff.UnchangedCount, Is.EqualTo(0));
            Assert.That(diff.HasChanges, Is.False);
        }

        [Test]
        public void Constructor_NullItems_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new DataImportDiff<TestEntry>(null));
        }

        [Test]
        public void Constructor_ItemsContainingNull_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new DataImportDiff<TestEntry>(new DataImportDiffItem<TestEntry>[]
                {
                    null
                }));
        }

        [Test]
        public void Items_ModificationThroughIList_ThrowsNotSupportedException()
        {
            DataImportDiffItem<TestEntry> item =
                CreateItem("item_001", DataImportDiffType.Unchanged);

            DataImportDiff<TestEntry> diff =
                new DataImportDiff<TestEntry>(new[] { item });

            IList<DataImportDiffItem<TestEntry>> items =
                (IList<DataImportDiffItem<TestEntry>>)diff.Items;

            Assert.Throws<NotSupportedException>(() =>
                items[0] = CreateItem("item_999", DataImportDiffType.Added));

            Assert.That(diff.Items[0], Is.SameAs(item));
        }

        [Test]
        public void DiffItem_StoresProvidedValues()
        {
            TestEntry current = new TestEntry("item_001");
            TestEntry incoming = new TestEntry("item_001");

            DataImportDiffItem<TestEntry> item =
                new DataImportDiffItem<TestEntry>(
                    "item_001",
                    DataImportDiffType.Modified,
                    current,
                    incoming);

            Assert.That(item.Id, Is.EqualTo("item_001"));
            Assert.That(item.Type, Is.EqualTo(DataImportDiffType.Modified));
            Assert.That(item.CurrentEntry, Is.SameAs(current));
            Assert.That(item.IncomingEntry, Is.SameAs(incoming));
        }

        [Test]
        public void DiffItem_EmptyId_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new DataImportDiffItem<TestEntry>(
                    "",
                    DataImportDiffType.Added,
                    default,
                    new TestEntry("item_001")));
        }

        private static DataImportDiffItem<TestEntry> CreateItem(string id, DataImportDiffType type)
        {
            TestEntry current =
                type == DataImportDiffType.Added
                    ? null
                    : new TestEntry(id);

            TestEntry incoming =
                type == DataImportDiffType.Removed
                    ? null
                    : new TestEntry(id);

            return new DataImportDiffItem<TestEntry>(
                id,
                type,
                current,
                incoming);
        }

        private sealed class TestEntry : IDataEntry
        {
            public string Id { get; }

            public TestEntry(string id)
            {
                Id = id;
            }
        }
    }
}