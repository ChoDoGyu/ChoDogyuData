using NUnit.Framework;

namespace CDG.Data.Tests.Runtime
{
    public sealed class DataIdValidatorTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("\t")]
        [TestCase(" sword_001")]
        [TestCase("sword_001 ")]
        [TestCase("\tsword_001")]
        [TestCase("sword_001\n")]
        public void IsValid_InvalidId_ReturnsFalse(string id)
        {
            bool result = DataIdValidator.IsValid(id);

            Assert.That(result, Is.False);
        }

        [TestCase("sword_001")]
        [TestCase("Sword_001")]
        [TestCase("enemy-boss-01")]
        [TestCase("weapon.sword.001")]
        [TestCase("Boss Phase 1")]
        [TestCase("æ∆¿Ã≈€_001")]
        public void IsValid_ValidId_ReturnsTrue(string id)
        {
            bool result = DataIdValidator.IsValid(id);

            Assert.That(result, Is.True);
        }
    }
}