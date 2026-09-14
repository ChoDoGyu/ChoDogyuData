using System;
using System.Collections.Generic;
using CDG.Data;
using CDG.Data.Editor.Importing;
using NUnit.Framework;

namespace CDG.Data.Tests.Editor
{
    public sealed class DataImportCandidateTests
    {
        [Test]
        public void Constructor_ValidEntries_PreservesOriginalOrder()
        {
            TestEntry first = new TestEntry("item_001");
            TestEntry second = new TestEntry("item_002");

            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                first,
                second
            });

            Assert.That(candidate.Count, Is.EqualTo(2));
            Assert.That(candidate.Entries[0], Is.SameAs(first));
            Assert.That(candidate.Entries[1], Is.SameAs(second));
        }

        [Test]
        public void Constructor_SourceModifiedAfterCreation_KeepsSnapshot()
        {
            TestEntry first = new TestEntry("item_001");

            List<TestEntry> source = new List<TestEntry>
            {
                first
            };

            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(source);

            source.Clear();
            source.Add(new TestEntry("item_999"));

            Assert.That(candidate.Count, Is.EqualTo(1));
            Assert.That(candidate.Entries[0], Is.SameAs(first));
        }

        [Test]
        public void Entries_ModificationThroughIList_ThrowsNotSupportedException()
        {
            TestEntry original = new TestEntry("item_001");

            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                original
            });

            IList<TestEntry> entries = (IList<TestEntry>)candidate.Entries;

            Assert.Throws<NotSupportedException>(() => entries[0] = new TestEntry("item_999"));
            Assert.That(candidate.Entries[0], Is.SameAs(original));
        }

        [Test]
        public void Constructor_NullEntries_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DataImportCandidate<TestEntry>(null));
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