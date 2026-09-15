using System;
using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Editor.Importing;
using CDG.Data.Validation;
using NUnit.Framework;

namespace CDG.Data.Tests.Editor
{
    public sealed class DataImportDiffBoundaryTests
    {
        [Test]
        public void Build_IdsWithDifferentCase_TreatsAsAddedAndRemoved()
        {
            TestEntry current = new TestEntry("item_001", 10);
            TestEntry incoming = new TestEntry("Item_001", 10);

            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                incoming
            });

            TestComparer comparer = new TestComparer();

            Result<DataImportDiff<TestEntry>> result = DataImportDiffBuilder.Build(
                new[] { current },
                candidate,
                comparer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(2));

            Assert.That(result.Value.Items[0].Id, Is.EqualTo("Item_001"));
            Assert.That(result.Value.Items[0].Type, Is.EqualTo(DataImportDiffType.Added));

            Assert.That(result.Value.Items[1].Id, Is.EqualTo("item_001"));
            Assert.That(result.Value.Items[1].Type, Is.EqualTo(DataImportDiffType.Removed));

            Assert.That(result.Value.AddedCount, Is.EqualTo(1));
            Assert.That(result.Value.RemovedCount, Is.EqualTo(1));
            Assert.That(result.Value.ModifiedCount, Is.EqualTo(0));
            Assert.That(result.Value.UnchangedCount, Is.EqualTo(0));

            Assert.That(comparer.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void Build_ReorderedEquivalentEntries_PreservesIncomingOrder()
        {
            TestEntry currentA = new TestEntry("item_a", 10);
            TestEntry currentB = new TestEntry("item_b", 20);
            TestEntry currentC = new TestEntry("item_c", 30);

            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                new TestEntry("item_c", 30),
                new TestEntry("item_a", 10),
                new TestEntry("item_b", 20)
            });

            TestComparer comparer = new TestComparer();

            Result<DataImportDiff<TestEntry>> result = DataImportDiffBuilder.Build(
                new[]
                {
                    currentA,
                    currentB,
                    currentC
                },
                candidate,
                comparer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(3));

            Assert.That(result.Value.Items[0].Id, Is.EqualTo("item_c"));
            Assert.That(result.Value.Items[1].Id, Is.EqualTo("item_a"));
            Assert.That(result.Value.Items[2].Id, Is.EqualTo("item_b"));

            Assert.That(result.Value.Items[0].Type, Is.EqualTo(DataImportDiffType.Unchanged));
            Assert.That(result.Value.Items[1].Type, Is.EqualTo(DataImportDiffType.Unchanged));
            Assert.That(result.Value.Items[2].Type, Is.EqualTo(DataImportDiffType.Unchanged));

            Assert.That(result.Value.RemovedCount, Is.EqualTo(0));
            Assert.That(result.Value.HasChanges, Is.False);
            Assert.That(comparer.CallCount, Is.EqualTo(3));
        }

        [Test]
        public void Build_EmptyCandidate_RemovesAllCurrentEntriesInCurrentOrder()
        {
            TestEntry currentA = new TestEntry("item_a", 10);
            TestEntry currentB = new TestEntry("item_b", 20);

            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(
                Array.Empty<TestEntry>());

            TestComparer comparer = new TestComparer();

            Result<DataImportDiff<TestEntry>> result = DataImportDiffBuilder.Build(
                new[]
                {
                    currentA,
                    currentB
                },
                candidate,
                comparer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(2));

            Assert.That(result.Value.Items[0].Id, Is.EqualTo("item_a"));
            Assert.That(result.Value.Items[0].Type, Is.EqualTo(DataImportDiffType.Removed));

            Assert.That(result.Value.Items[1].Id, Is.EqualTo("item_b"));
            Assert.That(result.Value.Items[1].Type, Is.EqualTo(DataImportDiffType.Removed));

            Assert.That(result.Value.RemovedCount, Is.EqualTo(2));
            Assert.That(result.Value.HasChanges, Is.True);
            Assert.That(comparer.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void Build_EmptyCurrent_AddsAllCandidateEntriesInIncomingOrder()
        {
            TestEntry incomingB = new TestEntry("item_b", 20);
            TestEntry incomingA = new TestEntry("item_a", 10);

            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                incomingB,
                incomingA
            });

            TestComparer comparer = new TestComparer();

            Result<DataImportDiff<TestEntry>> result = DataImportDiffBuilder.Build(
                Array.Empty<TestEntry>(),
                candidate,
                comparer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(2));

            Assert.That(result.Value.Items[0].Id, Is.EqualTo("item_b"));
            Assert.That(result.Value.Items[0].Type, Is.EqualTo(DataImportDiffType.Added));

            Assert.That(result.Value.Items[1].Id, Is.EqualTo("item_a"));
            Assert.That(result.Value.Items[1].Type, Is.EqualTo(DataImportDiffType.Added));

            Assert.That(result.Value.AddedCount, Is.EqualTo(2));
            Assert.That(result.Value.HasChanges, Is.True);
            Assert.That(comparer.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void Import_ValidEmptyCandidateAndEmptyCurrent_ReturnsCalculatedEmptyDiff()
        {
            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(
                Array.Empty<TestEntry>());

            StubImporter importer = new StubImporter(
                Result<DataImportCandidate<TestEntry>>.Success(candidate));

            TestComparer comparer = new TestComparer();

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import(
                "source",
                importer,
                Array.Empty<TestEntry>(),
                comparer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.IsValid, Is.True);
            Assert.That(result.Value.HasDiff, Is.True);
            Assert.That(result.Value.Diff, Is.Not.Null);
            Assert.That(result.Value.Diff.Count, Is.EqualTo(0));
            Assert.That(result.Value.Diff.HasChanges, Is.False);
            Assert.That(comparer.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void Import_TwoArgumentOverload_ReturnsPreviewWithoutDiff()
        {
            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                new TestEntry("item_001", 10)
            });

            StubImporter importer = new StubImporter(
                Result<DataImportCandidate<TestEntry>>.Success(candidate));

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import(
                "source",
                importer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.IsValid, Is.True);
            Assert.That(result.Value.HasDiff, Is.False);
            Assert.That(result.Value.Diff, Is.Null);
        }

        [Test]
        public void Preview_NullCandidate_ThrowsArgumentNullException()
        {
            DataValidationReport validationReport = DataTableValidator.Validate(
                Array.Empty<TestEntry>());

            Assert.Throws<ArgumentNullException>(() =>
                new DataImportPreview<TestEntry>(
                    null,
                    validationReport));
        }

        [Test]
        public void Preview_NullValidationReport_ThrowsArgumentNullException()
        {
            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(
                Array.Empty<TestEntry>());

            Assert.Throws<ArgumentNullException>(() =>
                new DataImportPreview<TestEntry>(
                    candidate,
                    null));
        }

        [Test]
        public void Preview_WithDiff_StoresSameDiffAndReportsHasDiff()
        {
            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(
                Array.Empty<TestEntry>());

            DataValidationReport validationReport = DataTableValidator.Validate(
                candidate.Entries);

            DataImportDiff<TestEntry> diff = new DataImportDiff<TestEntry>(
                Array.Empty<DataImportDiffItem<TestEntry>>());

            DataImportPreview<TestEntry> preview = new DataImportPreview<TestEntry>(
                candidate,
                validationReport,
                diff);

            Assert.That(preview.Candidate, Is.SameAs(candidate));
            Assert.That(preview.ValidationReport, Is.SameAs(validationReport));
            Assert.That(preview.Diff, Is.SameAs(diff));
            Assert.That(preview.IsValid, Is.True);
            Assert.That(preview.HasDiff, Is.True);
        }

        private sealed class StubImporter : IDataTextImporter<TestEntry>
        {
            private readonly Result<DataImportCandidate<TestEntry>> result;

            public StubImporter(Result<DataImportCandidate<TestEntry>> result)
            {
                this.result = result;
            }

            public Result<DataImportCandidate<TestEntry>> Import(string text)
            {
                return result;
            }
        }

        private sealed class TestComparer : IDataImportEntryComparer<TestEntry>
        {
            public int CallCount { get; private set; }

            public bool AreEquivalent(TestEntry current, TestEntry incoming)
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