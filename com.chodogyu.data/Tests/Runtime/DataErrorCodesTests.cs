using NUnit.Framework;

namespace CDG.Data.Tests.Runtime
{
    public sealed class DataErrorCodesTests
    {
        [Test]
        public void ErrorCodes_HaveExpectedValues()
        {
            Assert.That(DataErrorCodes.InvalidId, Is.EqualTo("DATA_INVALID_ID"));
            Assert.That(DataErrorCodes.NotFound, Is.EqualTo("DATA_NOT_FOUND"));
            Assert.That(DataErrorCodes.ValidationFailed, Is.EqualTo("DATA_VALIDATION_FAILED"));
            Assert.That(DataErrorCodes.ImportFailed, Is.EqualTo("DATA_IMPORT_FAILED"));
        }
    }
}