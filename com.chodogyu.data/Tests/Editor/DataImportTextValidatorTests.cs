using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Editor.Importing;
using NUnit.Framework;

namespace CDG.Data.Tests.Editor
{
    public sealed class DataImportTextValidatorTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("\t\r\n")]
        public void Validate_InvalidText_ReturnsImportFailed(string text)
        {
            Result result = DataImportTextValidator.Validate(text);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
            Assert.That(result.Error.Message, Is.Not.Empty);
        }

        [TestCase("data")]
        [TestCase("  data  ")]
        [TestCase("\n{}\n")]
        [TestCase("id,name\nitem_001,Sword")]
        public void Validate_NonWhitespaceText_ReturnsSuccess(string text)
        {
            Result result = DataImportTextValidator.Validate(text);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.SameAs(ResultError.None));
        }
    }
}