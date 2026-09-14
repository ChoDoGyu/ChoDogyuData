using CDG.Data.Validation;
using NUnit.Framework;

namespace CDG.Data.Tests.Runtime
{
    public sealed class DataTableValidatorTests
    {
        [Test]
        public void Validate_NullSource_ReturnsNullSourceIssue()
        {
            DataValidationReport report = DataTableValidator.Validate<TestEntry>(null);

            Assert.That(report.IsValid, Is.False);
            Assert.That(report.Count, Is.EqualTo(1));
            Assert.That(report.Issues[0].Type, Is.EqualTo(DataValidationIssueType.NullSource));
            Assert.That(report.Issues[0].EntryIndex, Is.EqualTo(-1));
            Assert.That(report.Issues[0].EntryId, Is.Null);
        }

        [Test]
        public void Validate_EmptySource_ReturnsValidReport()
        {
            DataValidationReport report = DataTableValidator.Validate(new TestEntry[0]);

            Assert.That(report.IsValid, Is.True);
            Assert.That(report.Count, Is.EqualTo(0));
        }

        [Test]
        public void Validate_ValidEntries_ReturnsValidReport()
        {
            TestEntry[] entries =
            {
                new TestEntry("item_001"),
                new TestEntry("item_002")
            };

            DataValidationReport report = DataTableValidator.Validate(entries);

            Assert.That(report.IsValid, Is.True);
            Assert.That(report.Count, Is.EqualTo(0));
        }

        [Test]
        public void Validate_NullEntry_ReturnsNullEntryIssue()
        {
            TestEntry[] entries =
            {
                new TestEntry("item_001"),
                null
            };

            DataValidationReport report = DataTableValidator.Validate(entries);

            Assert.That(report.IsValid, Is.False);
            Assert.That(report.Count, Is.EqualTo(1));
            Assert.That(report.Issues[0].Type, Is.EqualTo(DataValidationIssueType.NullEntry));
            Assert.That(report.Issues[0].EntryIndex, Is.EqualTo(1));
            Assert.That(report.Issues[0].EntryId, Is.Null);
        }

        [Test]
        public void Validate_InvalidId_ReturnsInvalidIdIssue()
        {
            TestEntry[] entries =
            {
                new TestEntry("item_001"),
                new TestEntry(" item_002")
            };

            DataValidationReport report = DataTableValidator.Validate(entries);

            Assert.That(report.IsValid, Is.False);
            Assert.That(report.Count, Is.EqualTo(1));
            Assert.That(report.Issues[0].Type, Is.EqualTo(DataValidationIssueType.InvalidId));
            Assert.That(report.Issues[0].EntryIndex, Is.EqualTo(1));
            Assert.That(report.Issues[0].EntryId, Is.EqualTo(" item_002"));
        }

        [Test]
        public void Validate_DuplicateId_ReturnsDuplicateIdIssue()
        {
            TestEntry[] entries =
            {
                new TestEntry("item_001"),
                new TestEntry("item_002"),
                new TestEntry("item_001")
            };

            DataValidationReport report = DataTableValidator.Validate(entries);

            Assert.That(report.IsValid, Is.False);
            Assert.That(report.Count, Is.EqualTo(1));
            Assert.That(report.Issues[0].Type, Is.EqualTo(DataValidationIssueType.DuplicateId));
            Assert.That(report.Issues[0].EntryIndex, Is.EqualTo(2));
            Assert.That(report.Issues[0].EntryId, Is.EqualTo("item_001"));
        }

        [Test]
        public void Validate_MultipleProblems_CollectsAllIssues()
        {
            TestEntry[] entries =
            {
                null,
                new TestEntry(" invalid"),
                new TestEntry("item_001"),
                new TestEntry("item_001")
            };

            DataValidationReport report = DataTableValidator.Validate(entries);

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
        public void Validate_IdsWithDifferentCase_ReturnsValidReport()
        {
            TestEntry[] entries =
            {
                new TestEntry("item_001"),
                new TestEntry("Item_001")
            };

            DataValidationReport report = DataTableValidator.Validate(entries);

            Assert.That(report.IsValid, Is.True);
            Assert.That(report.Count, Is.EqualTo(0));
        }

        private sealed class TestEntry : IDataEntry
        {
            public string Id { get; }

            public TestEntry(string id)
            {
                Id = id;
            }
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("\t")]
        [TestCase(" item_001")]
        [TestCase("item_001 ")]
        [TestCase("\titem_001")]
        [TestCase("item_001\n")]
        public void Validate_InvalidIdVariants_ReturnsInvalidIdIssue(string id)
        {
            TestEntry[] entries =
            {
        new TestEntry(id)
    };

            DataValidationReport report = DataTableValidator.Validate(entries);

            Assert.That(report.IsValid, Is.False);
            Assert.That(report.Count, Is.EqualTo(1));
            Assert.That(report.Issues[0].Type, Is.EqualTo(DataValidationIssueType.InvalidId));
            Assert.That(report.Issues[0].EntryIndex, Is.EqualTo(0));
            Assert.That(report.Issues[0].EntryId, Is.EqualTo(id));
        }

        [Test]
        public void Validate_RepeatedDuplicateId_ReportsEachAdditionalOccurrence()
        {
            TestEntry[] entries =
            {
                new TestEntry("item_001"),
                new TestEntry("item_001"),
                new TestEntry("item_001")
            };

            DataValidationReport report = DataTableValidator.Validate(entries);

            Assert.That(report.IsValid, Is.False);
            Assert.That(report.Count, Is.EqualTo(2));

            Assert.That(report.Issues[0].Type, Is.EqualTo(DataValidationIssueType.DuplicateId));
            Assert.That(report.Issues[0].EntryIndex, Is.EqualTo(1));
            Assert.That(report.Issues[0].EntryId, Is.EqualTo("item_001"));

            Assert.That(report.Issues[1].Type, Is.EqualTo(DataValidationIssueType.DuplicateId));
            Assert.That(report.Issues[1].EntryIndex, Is.EqualTo(2));
            Assert.That(report.Issues[1].EntryId, Is.EqualTo("item_001"));
        }

        [Test]
        public void Validate_SameInvalidIds_DoesNotReportDuplicateIssue()
        {
            TestEntry[] entries =
            {
                new TestEntry(" invalid"),
                new TestEntry(" invalid")
            };

            DataValidationReport report = DataTableValidator.Validate(entries);

            Assert.That(report.IsValid, Is.False);
            Assert.That(report.Count, Is.EqualTo(2));
            Assert.That(report.Issues[0].Type, Is.EqualTo(DataValidationIssueType.InvalidId));
            Assert.That(report.Issues[1].Type, Is.EqualTo(DataValidationIssueType.InvalidId));
        }

        [TestCase("Boss Phase 1")]
        [TestCase("æ∆¿Ã≈€_001")]
        [TestCase("enemy-boss-01")]
        public void Validate_AllowedIdFormats_ReturnsValidReport(string id)
        {
            TestEntry[] entries =
            {
                new TestEntry(id)
            };

            DataValidationReport report = DataTableValidator.Validate(entries);

            Assert.That(report.IsValid, Is.True);
            Assert.That(report.Count, Is.EqualTo(0));
        }
    }
}