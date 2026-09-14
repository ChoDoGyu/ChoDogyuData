using System;
using CDG.Data.Validation;
using NUnit.Framework;

namespace CDG.Data.Tests.Runtime
{
    public sealed class DataValidationIssueTests
    {
        [Test]
        public void Constructor_ValidValues_StoresValues()
        {
            DataValidationIssue issue = new DataValidationIssue(DataValidationIssueType.InvalidId, "유효하지 않은 ID입니다.", 3, " item_001");

            Assert.That(issue.Type, Is.EqualTo(DataValidationIssueType.InvalidId));
            Assert.That(issue.Message, Is.EqualTo("유효하지 않은 ID입니다."));
            Assert.That(issue.EntryIndex, Is.EqualTo(3));
            Assert.That(issue.EntryId, Is.EqualTo(" item_001"));
        }

        [Test]
        public void Constructor_NullMessage_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DataValidationIssue(DataValidationIssueType.NullSource, null));
        }
    }
}