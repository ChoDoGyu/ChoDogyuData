using System;
using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Editor.Importing;
using NUnit.Framework;

namespace CDG.Data.Tests.Editor
{
    public sealed class DataImportProcessorDiffTests
    {
        [Test]
        public void Import_ValidCandidateWithCurrentEntries_ReturnsPreviewWithDiff()
        {
            TestEntry currentA = new TestEntry("item_a", 10);
            TestEntry currentB = new TestEntry("item_b", 20);

            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                new TestEntry("item_a", 15),
                new TestEntry("item_c", 30)
            });

            StubImporter importer = new StubImporter(
                Result<DataImportCandidate<TestEntry>>.Success(candidate));

            TestComparer comparer = new TestComparer();

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import(
                "source",
                importer,
                new[]
                {
                    currentA,
                    currentB
                },
                comparer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.IsValid, Is.True);
            Assert.That(result.Value.HasDiff, Is.True);
            Assert.That(result.Value.Diff, Is.Not.Null);

            Assert.That(result.Value.Diff.Count, Is.EqualTo(3));
            Assert.That(result.Value.Diff.AddedCount, Is.EqualTo(1));
            Assert.That(result.Value.Diff.RemovedCount, Is.EqualTo(1));
            Assert.That(result.Value.Diff.ModifiedCount, Is.EqualTo(1));
            Assert.That(result.Value.Diff.UnchangedCount, Is.EqualTo(0));

            Assert.That(result.Value.Diff.Items[0].Id, Is.EqualTo("item_a"));
            Assert.That(result.Value.Diff.Items[0].Type, Is.EqualTo(DataImportDiffType.Modified));

            Assert.That(result.Value.Diff.Items[1].Id, Is.EqualTo("item_c"));
            Assert.That(result.Value.Diff.Items[1].Type, Is.EqualTo(DataImportDiffType.Added));

            Assert.That(result.Value.Diff.Items[2].Id, Is.EqualTo("item_b"));
            Assert.That(result.Value.Diff.Items[2].Type, Is.EqualTo(DataImportDiffType.Removed));

            Assert.That(comparer.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void Import_EquivalentCandidate_ReturnsDiffWithoutChanges()
        {
            TestEntry current = new TestEntry("item_001", 10);

            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                new TestEntry("item_001", 10)
            });

            StubImporter importer = new StubImporter(
                Result<DataImportCandidate<TestEntry>>.Success(candidate));

            TestComparer comparer = new TestComparer();

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import(
                "source",
                importer,
                new[] { current },
                comparer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.IsValid, Is.True);
            Assert.That(result.Value.HasDiff, Is.True);
            Assert.That(result.Value.Diff.HasChanges, Is.False);
            Assert.That(result.Value.Diff.UnchangedCount, Is.EqualTo(1));
            Assert.That(comparer.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void Import_InvalidCandidate_ReturnsPreviewWithoutDiff()
        {
            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                new TestEntry("item_001", 10),
                new TestEntry("item_001", 20)
            });

            StubImporter importer = new StubImporter(
                Result<DataImportCandidate<TestEntry>>.Success(candidate));

            TestComparer comparer = new TestComparer();

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import(
                "source",
                importer,
                Array.Empty<TestEntry>(),
                comparer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.IsValid, Is.False);
            Assert.That(result.Value.HasDiff, Is.False);
            Assert.That(result.Value.Diff, Is.Null);
            Assert.That(result.Value.ValidationReport.Count, Is.EqualTo(1));
            Assert.That(comparer.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void Import_InvalidCurrentEntries_ReturnsValidationFailed()
        {
            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                new TestEntry("item_002", 30)
            });

            StubImporter importer = new StubImporter(
                Result<DataImportCandidate<TestEntry>>.Success(candidate));

            TestComparer comparer = new TestComparer();

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import(
                "source",
                importer,
                new[]
                {
                    new TestEntry("item_001", 10),
                    new TestEntry("item_001", 20)
                },
                comparer);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ValidationFailed));
            Assert.That(comparer.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void Import_ImporterFailure_ReturnsOriginalFailureWithoutComparing()
        {
            ResultError error = new ResultError(
                DataErrorCodes.ImportFailed,
                "테스트 Import 실패");

            StubImporter importer = new StubImporter(
                Result<DataImportCandidate<TestEntry>>.Failure(error));

            TestComparer comparer = new TestComparer();

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import(
                "source",
                importer,
                Array.Empty<TestEntry>(),
                comparer);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.SameAs(error));
            Assert.That(comparer.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void Import_NullCurrentEntries_ThrowsArgumentNullException()
        {
            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(
                Array.Empty<TestEntry>());

            StubImporter importer = new StubImporter(
                Result<DataImportCandidate<TestEntry>>.Success(candidate));

            TestComparer comparer = new TestComparer();

            Assert.Throws<ArgumentNullException>(() =>
                DataImportProcessor.Import<TestEntry>(
                    "source",
                    importer,
                    null,
                    comparer));
        }

        [Test]
        public void Import_NullComparer_ThrowsArgumentNullException()
        {
            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(
                Array.Empty<TestEntry>());

            StubImporter importer = new StubImporter(
                Result<DataImportCandidate<TestEntry>>.Success(candidate));

            Assert.Throws<ArgumentNullException>(() =>
                DataImportProcessor.Import<TestEntry>(
                    "source",
                    importer,
                    Array.Empty<TestEntry>(),
                    null));
        }

        [Test]
        public void Import_NullImporter_ThrowsArgumentNullException()
        {
            TestComparer comparer = new TestComparer();

            Assert.Throws<ArgumentNullException>(() =>
                DataImportProcessor.Import<TestEntry>(
                    "source",
                    null,
                    Array.Empty<TestEntry>(),
                    comparer));
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