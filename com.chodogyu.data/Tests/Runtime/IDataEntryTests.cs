using System.Reflection;
using NUnit.Framework;

namespace CDG.Data.Tests.Runtime
{
    public sealed class IDataEntryTests
    {
        [Test]
        public void IDataEntry_IdProperty_HasExpectedContract()
        {
            PropertyInfo[] properties = typeof(IDataEntry).GetProperties();

            Assert.That(properties, Has.Length.EqualTo(1));

            PropertyInfo idProperty = properties[0];

            Assert.That(idProperty.Name, Is.EqualTo(nameof(IDataEntry.Id)));
            Assert.That(idProperty.PropertyType, Is.EqualTo(typeof(string)));
            Assert.That(idProperty.CanRead, Is.True);
            Assert.That(idProperty.CanWrite, Is.False);
        }

        [Test]
        public void IDataEntry_Implementation_ReturnsId()
        {
            IDataEntry entry = new TestEntry("item_001");

            Assert.That(entry.Id, Is.EqualTo("item_001"));
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