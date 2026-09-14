using System;
using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Editor.Importing;
using CDG.Data.Validation;
using NUnit.Framework;

namespace CDG.Data.Tests.Editor
{
    public sealed class DataImportProcessorTests
    {
        [Test]
        public void Import_ValidTextAndCandidate_ReturnsValidPreview()
        {
            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                new TestEntry("item_001"),
                new TestEntry("item_002")
            });

            StubImporter importer = new StubImporter(Result<DataImportCandidate<TestEntry>>.Success(candidate));

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import("source", importer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(importer.CallCount, Is.EqualTo(1));
            Assert.That(importer.LastText, Is.EqualTo("source"));
            Assert.That(result.Value.Candidate, Is.SameAs(candidate));
            Assert.That(result.Value.IsValid, Is.True);
            Assert.That(result.Value.ValidationReport.Count, Is.EqualTo(0));
        }

        [Test]
        public void Import_InvalidText_ReturnsImportFailedWithoutCallingImporter()
        {
            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(Array.Empty<TestEntry>());
            StubImporter importer = new StubImporter(Result<DataImportCandidate<TestEntry>>.Success(candidate));

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import("   ", importer);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
            Assert.That(importer.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void Import_ImporterFailure_ReturnsOriginalFailure()
        {
            ResultError error = new ResultError(DataErrorCodes.ImportFailed, "테스트 Import 실패");
            StubImporter importer = new StubImporter(Result<DataImportCandidate<TestEntry>>.Failure(error));

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import("source", importer);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.SameAs(error));
            Assert.That(importer.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void Import_InvalidCandidate_ReturnsSuccessfulPreviewWithValidationIssues()
        {
            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                new TestEntry("item_001"),
                new TestEntry("item_001")
            });

            StubImporter importer = new StubImporter(Result<DataImportCandidate<TestEntry>>.Success(candidate));

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import("source", importer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.IsValid, Is.False);
            Assert.That(result.Value.ValidationReport.Count, Is.EqualTo(1));
            Assert.That(result.Value.ValidationReport.Issues[0].Type, Is.EqualTo(DataValidationIssueType.DuplicateId));
        }

        [Test]
        public void Import_SuccessWithNullCandidate_ReturnsImportFailed()
        {
            StubImporter importer = new StubImporter(Result<DataImportCandidate<TestEntry>>.Success(null));

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import("source", importer);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Import_TextWithOuterWhitespace_PassesOriginalTextToImporter()
        {
            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(Array.Empty<TestEntry>());
            StubImporter importer = new StubImporter(Result<DataImportCandidate<TestEntry>>.Success(candidate));

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import("  source  ", importer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(importer.LastText, Is.EqualTo("  source  "));
        }

        [Test]
        public void Import_NullImporter_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => DataImportProcessor.Import<TestEntry>("source", null));
        }

        private sealed class StubImporter : IDataTextImporter<TestEntry>
        {
            private readonly Result<DataImportCandidate<TestEntry>> result;

            public int CallCount { get; private set; }
            public string LastText { get; private set; }

            public StubImporter(Result<DataImportCandidate<TestEntry>> result)
            {
                this.result = result;
            }

            public Result<DataImportCandidate<TestEntry>> Import(string text)
            {
                CallCount++;
                LastText = text;
                return result;
            }
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