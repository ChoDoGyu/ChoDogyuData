using System;
using CDG.Data;
using CDG.Data.Editor.Importing;
using CDG.Data.Validation;
using NUnit.Framework;

namespace CDG.Data.Tests.Editor
{
    public sealed class DataImportCandidateValidatorTests
    {
        [Test]
        public void Validate_ValidCandidate_ReturnsValidReport()
        {
            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                new TestEntry("item_001"),
                new TestEntry("item_002")
            });

            DataValidationReport report = DataImportCandidateValidator.Validate(candidate);

            Assert.That(report.IsValid, Is.True);
            Assert.That(report.Count, Is.EqualTo(0));
        }

        [Test]
        public void Validate_NullEntry_ReturnsNullEntryIssue()
        {
            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new TestEntry[]
            {
                new TestEntry("item_001"),
                null
            });

            DataValidationReport report = DataImportCandidateValidator.Validate(candidate);

            Assert.That(report.IsValid, Is.False);
            Assert.That(report.Count, Is.EqualTo(1));
            Assert.That(report.Issues[0].Type, Is.EqualTo(DataValidationIssueType.NullEntry));
            Assert.That(report.Issues[0].EntryIndex, Is.EqualTo(1));
        }

        [Test]
        public void Validate_InvalidId_ReturnsInvalidIdIssue()
        {
            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                new TestEntry("item_001"),
                new TestEntry(" item_002")
            });

            DataValidationReport report = DataImportCandidateValidator.Validate(candidate);

            Assert.That(report.IsValid, Is.False);
            Assert.That(report.Count, Is.EqualTo(1));
            Assert.That(report.Issues[0].Type, Is.EqualTo(DataValidationIssueType.InvalidId));
            Assert.That(report.Issues[0].EntryIndex, Is.EqualTo(1));
            Assert.That(report.Issues[0].EntryId, Is.EqualTo(" item_002"));
        }

        [Test]
        public void Validate_DuplicateId_ReturnsDuplicateIdIssue()
        {
            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                new TestEntry("item_001"),
                new TestEntry("item_002"),
                new TestEntry("item_001")
            });

            DataValidationReport report = DataImportCandidateValidator.Validate(candidate);

            Assert.That(report.IsValid, Is.False);
            Assert.That(report.Count, Is.EqualTo(1));
            Assert.That(report.Issues[0].Type, Is.EqualTo(DataValidationIssueType.DuplicateId));
            Assert.That(report.Issues[0].EntryIndex, Is.EqualTo(2));
            Assert.That(report.Issues[0].EntryId, Is.EqualTo("item_001"));
        }

        [Test]
        public void Validate_MultipleProblems_CollectsAllIssues()
        {
            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new TestEntry[]
            {
                null,
                new TestEntry(" invalid"),
                new TestEntry("item_001"),
                new TestEntry("item_001")
            });

            DataValidationReport report = DataImportCandidateValidator.Validate(candidate);

            Assert.That(report.IsValid, Is.False);
            Assert.That(report.Count, Is.EqualTo(3));

            Assert.That(report.Issues[0].Type, Is.EqualTo(DataValidationIssueType.NullEntry));
            Assert.That(report.Issues[0].EntryIndex, Is.EqualTo(0));

            Assert.That(report.Issues[1].Type, Is.EqualTo(DataValidationIssueType.InvalidId));
            Assert.That(report.Issues[1].EntryIndex, Is.EqualTo(1));

            Assert.That(report.Issues[2].Type, Is.EqualTo(DataValidationIssueType.DuplicateId));
            Assert.That(report.Issues[2].EntryIndex, Is.EqualTo(3));
        }

        [Test]
        public void Validate_NullCandidate_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => DataImportCandidateValidator.Validate<TestEntry>(null));
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