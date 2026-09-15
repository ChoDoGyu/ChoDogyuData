using System;
using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Editor.Importing;
using NUnit.Framework;

namespace CDG.Data.Tests.Editor
{
    public sealed class DataImportDiffBuilderTests
    {
        [Test]
        public void Build_MixedChanges_ReturnsExpectedTypesAndOrder()
        {
            TestEntry currentA = new TestEntry("item_a", 10);
            TestEntry currentB = new TestEntry("item_b", 20);
            TestEntry currentC = new TestEntry("item_c", 30);

            TestEntry incomingB = new TestEntry("item_b", 25);
            TestEntry incomingA = new TestEntry("item_a", 10);
            TestEntry incomingD = new TestEntry("item_d", 40);

            DataImportCandidate<TestEntry> candidate =
                new DataImportCandidate<TestEntry>(new[]
                {
                    incomingB,
                    incomingA,
                    incomingD
                });

            TestComparer comparer = new TestComparer();

            Result<DataImportDiff<TestEntry>> result =
                DataImportDiffBuilder.Build(
                    new[]
                    {
                        currentA,
                        currentB,
                        currentC
                    },
                    candidate,
                    comparer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(4));

            Assert.That(result.Value.Items[0].Id, Is.EqualTo("item_b"));
            Assert.That(
                result.Value.Items[0].Type,
                Is.EqualTo(DataImportDiffType.Modified));

            Assert.That(result.Value.Items[1].Id, Is.EqualTo("item_a"));
            Assert.That(
                result.Value.Items[1].Type,
                Is.EqualTo(DataImportDiffType.Unchanged));

            Assert.That(result.Value.Items[2].Id, Is.EqualTo("item_d"));
            Assert.That(
                result.Value.Items[2].Type,
                Is.EqualTo(DataImportDiffType.Added));

            Assert.That(result.Value.Items[3].Id, Is.EqualTo("item_c"));
            Assert.That(
                result.Value.Items[3].Type,
                Is.EqualTo(DataImportDiffType.Removed));

            Assert.That(result.Value.AddedCount, Is.EqualTo(1));
            Assert.That(result.Value.RemovedCount, Is.EqualTo(1));
            Assert.That(result.Value.ModifiedCount, Is.EqualTo(1));
            Assert.That(result.Value.UnchangedCount, Is.EqualTo(1));

            Assert.That(comparer.CallCount, Is.EqualTo(2));
        }

        [Test]
        public void Build_EquivalentEntries_ReturnsUnchanged()
        {
            TestEntry current = new TestEntry("item_001", 10);
            TestEntry incoming = new TestEntry("item_001", 10);

            DataImportCandidate<TestEntry> candidate =
                new DataImportCandidate<TestEntry>(new[]
                {
                    incoming
                });

            TestComparer comparer = new TestComparer();

            Result<DataImportDiff<TestEntry>> result =
                DataImportDiffBuilder.Build(
                    new[] { current },
                    candidate,
                    comparer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(
                result.Value.Items[0].Type,
                Is.EqualTo(DataImportDiffType.Unchanged));

            Assert.That(
                result.Value.Items[0].CurrentEntry,
                Is.SameAs(current));

            Assert.That(
                result.Value.Items[0].IncomingEntry,
                Is.SameAs(incoming));

            Assert.That(comparer.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void Build_DifferentEntries_ReturnsModified()
        {
            TestEntry current = new TestEntry("item_001", 10);
            TestEntry incoming = new TestEntry("item_001", 20);

            DataImportCandidate<TestEntry> candidate =
                new DataImportCandidate<TestEntry>(new[]
                {
                    incoming
                });

            TestComparer comparer = new TestComparer();

            Result<DataImportDiff<TestEntry>> result =
                DataImportDiffBuilder.Build(
                    new[] { current },
                    candidate,
                    comparer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(
                result.Value.Items[0].Type,
                Is.EqualTo(DataImportDiffType.Modified));

            Assert.That(comparer.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void Build_NewEntry_ReturnsAddedWithoutCallingComparer()
        {
            TestEntry incoming = new TestEntry("item_001", 10);

            DataImportCandidate<TestEntry> candidate =
                new DataImportCandidate<TestEntry>(new[]
                {
                    incoming
                });

            TestComparer comparer = new TestComparer();

            Result<DataImportDiff<TestEntry>> result =
                DataImportDiffBuilder.Build(
                    Array.Empty<TestEntry>(),
                    candidate,
                    comparer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(
                result.Value.Items[0].Type,
                Is.EqualTo(DataImportDiffType.Added));

            Assert.That(result.Value.Items[0].CurrentEntry, Is.Null);
            Assert.That(
                result.Value.Items[0].IncomingEntry,
                Is.SameAs(incoming));

            Assert.That(comparer.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void Build_MissingIncomingEntry_ReturnsRemovedWithoutCallingComparer()
        {
            TestEntry current = new TestEntry("item_001", 10);

            DataImportCandidate<TestEntry> candidate =
                new DataImportCandidate<TestEntry>(
                    Array.Empty<TestEntry>());

            TestComparer comparer = new TestComparer();

            Result<DataImportDiff<TestEntry>> result =
                DataImportDiffBuilder.Build(
                    new[] { current },
                    candidate,
                    comparer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(
                result.Value.Items[0].Type,
                Is.EqualTo(DataImportDiffType.Removed));

            Assert.That(
                result.Value.Items[0].CurrentEntry,
                Is.SameAs(current));

            Assert.That(result.Value.Items[0].IncomingEntry, Is.Null);
            Assert.That(comparer.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void Build_BothEmpty_ReturnsEmptyDiff()
        {
            DataImportCandidate<TestEntry> candidate =
                new DataImportCandidate<TestEntry>(
                    Array.Empty<TestEntry>());

            TestComparer comparer = new TestComparer();

            Result<DataImportDiff<TestEntry>> result =
                DataImportDiffBuilder.Build(
                    Array.Empty<TestEntry>(),
                    candidate,
                    comparer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(0));
            Assert.That(result.Value.HasChanges, Is.False);
            Assert.That(comparer.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void Build_InvalidCurrentEntries_ReturnsValidationFailed()
        {
            TestEntry first = new TestEntry("item_001", 10);
            TestEntry duplicate = new TestEntry("item_001", 20);

            DataImportCandidate<TestEntry> candidate =
                new DataImportCandidate<TestEntry>(
                    Array.Empty<TestEntry>());

            TestComparer comparer = new TestComparer();

            Result<DataImportDiff<TestEntry>> result =
                DataImportDiffBuilder.Build(
                    new[]
                    {
                        first,
                        duplicate
                    },
                    candidate,
                    comparer);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ValidationFailed));

            Assert.That(comparer.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void Build_InvalidCandidate_ReturnsValidationFailed()
        {
            DataImportCandidate<TestEntry> candidate =
                new DataImportCandidate<TestEntry>(new[]
                {
                    new TestEntry("item_001", 10),
                    new TestEntry("item_001", 20)
                });

            TestComparer comparer = new TestComparer();

            Result<DataImportDiff<TestEntry>> result =
                DataImportDiffBuilder.Build(
                    Array.Empty<TestEntry>(),
                    candidate,
                    comparer);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ValidationFailed));

            Assert.That(comparer.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void Build_NullCurrentEntries_ThrowsArgumentNullException()
        {
            DataImportCandidate<TestEntry> candidate =
                new DataImportCandidate<TestEntry>(
                    Array.Empty<TestEntry>());

            TestComparer comparer = new TestComparer();

            Assert.Throws<ArgumentNullException>(() =>
                DataImportDiffBuilder.Build<TestEntry>(
                    null,
                    candidate,
                    comparer));
        }

        [Test]
        public void Build_NullCandidate_ThrowsArgumentNullException()
        {
            TestComparer comparer = new TestComparer();

            Assert.Throws<ArgumentNullException>(() =>
                DataImportDiffBuilder.Build<TestEntry>(
                    Array.Empty<TestEntry>(),
                    null,
                    comparer));
        }

        [Test]
        public void Build_NullComparer_ThrowsArgumentNullException()
        {
            DataImportCandidate<TestEntry> candidate =
                new DataImportCandidate<TestEntry>(
                    Array.Empty<TestEntry>());

            Assert.Throws<ArgumentNullException>(() =>
                DataImportDiffBuilder.Build<TestEntry>(
                    Array.Empty<TestEntry>(),
                    candidate,
                    null));
        }

        private sealed class TestComparer :
            IDataImportEntryComparer<TestEntry>
        {
            public int CallCount { get; private set; }

            public bool AreEquivalent(
                TestEntry current,
                TestEntry incoming)
            {
                CallCount++;

                return current.Value == incoming.Value;
            }
        }

        private sealed class TestEntry : IDataEntry
        {
            public string Id { get; }
            public int Value { get; }

            public TestEntry(string id, int value)
            {
                Id = id;
                Value = value;
            }
        }
    }
}