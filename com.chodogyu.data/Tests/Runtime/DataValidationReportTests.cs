using System;
using System.Collections.Generic;
using CDG.Data.Validation;
using NUnit.Framework;

namespace CDG.Data.Tests.Runtime
{
    public sealed class DataValidationReportTests
    {
        [Test]
        public void Constructor_EmptyIssues_CreatesValidReport()
        {
            DataValidationReport report = new DataValidationReport(Array.Empty<DataValidationIssue>());

            Assert.That(report.IsValid, Is.True);
            Assert.That(report.Count, Is.EqualTo(0));
            Assert.That(report.Issues, Is.Empty);
        }

        [Test]
        public void Constructor_WithIssues_CreatesInvalidReport()
        {
            DataValidationIssue first = new DataValidationIssue(DataValidationIssueType.InvalidId, "잘못된 ID입니다.", 1, " item_001");
            DataValidationIssue second = new DataValidationIssue(DataValidationIssueType.DuplicateId, "중복된 ID입니다.", 3, "item_002");

            DataValidationReport report = new DataValidationReport(new[]
            {
                first,
                second
            });

            Assert.That(report.IsValid, Is.False);
            Assert.That(report.Count, Is.EqualTo(2));
            Assert.That(report.Issues[0], Is.SameAs(first));
            Assert.That(report.Issues[1], Is.SameAs(second));
        }

        [Test]
        public void Constructor_SourceModifiedAfterCreation_DoesNotChangeReport()
        {
            DataValidationIssue first = new DataValidationIssue(DataValidationIssueType.InvalidId, "잘못된 ID입니다.", 1, " item_001");

            List<DataValidationIssue> source = new List<DataValidationIssue>
            {
                first
            };

            DataValidationReport report = new DataValidationReport(source);

            source.Clear();

            Assert.That(report.Count, Is.EqualTo(1));
            Assert.That(report.Issues[0], Is.SameAs(first));
        }

        [Test]
        public void Issues_ModificationThroughIList_ThrowsNotSupportedException()
        {
            DataValidationIssue issue = new DataValidationIssue(DataValidationIssueType.InvalidId, "잘못된 ID입니다.", 1, " item_001");

            DataValidationReport report = new DataValidationReport(new[]
            {
                issue
            });

            IList<DataValidationIssue> issues = (IList<DataValidationIssue>)report.Issues;

            Assert.Throws<NotSupportedException>(() => issues[0] = new DataValidationIssue(DataValidationIssueType.NullEntry, "null 항목입니다."));
            Assert.That(report.Issues[0], Is.SameAs(issue));
        }

        [Test]
        public void Constructor_NullIssues_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DataValidationReport(null));
        }
    }
}