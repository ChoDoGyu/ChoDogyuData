using System;
using System.Collections.Generic;
using CDG.Core.Results;
using NUnit.Framework;

namespace CDG.Data.Tests.Runtime
{
    public sealed class DataTableTests
    {
        [Test]
        public void Create_ValidEntries_ReturnsSuccessfulTable()
        {
            TestEntry first = new TestEntry("item_001");
            TestEntry second = new TestEntry("item_002");

            Result<DataTable<TestEntry>> result = DataTable<TestEntry>.Create(new[]
            {
                first,
                second
            });

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(2));
            Assert.That(result.Value.Entries[0], Is.SameAs(first));
            Assert.That(result.Value.Entries[1], Is.SameAs(second));
        }

        [Test]
        public void Create_EmptyEntries_ReturnsEmptyTable()
        {
            Result<DataTable<TestEntry>> result = DataTable<TestEntry>.Create(new TestEntry[0]);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(0));
            Assert.That(result.Value.Entries, Is.Empty);
        }

        [Test]
        public void Create_NullSource_ReturnsValidationFailed()
        {
            Result<DataTable<TestEntry>> result = DataTable<TestEntry>.Create(null);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ValidationFailed));
        }

        [Test]
        public void Create_NullEntry_ReturnsValidationFailed()
        {
            TestEntry[] entries =
            {
                new TestEntry("item_001"),
                null
            };

            Result<DataTable<TestEntry>> result = DataTable<TestEntry>.Create(entries);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ValidationFailed));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase(" item_001")]
        [TestCase("item_001 ")]
        public void Create_InvalidId_ReturnsInvalidId(string id)
        {
            TestEntry[] entries =
            {
                new TestEntry(id)
            };

            Result<DataTable<TestEntry>> result = DataTable<TestEntry>.Create(entries);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.InvalidId));
        }

        [Test]
        public void Create_DuplicateId_ReturnsValidationFailed()
        {
            TestEntry[] entries =
            {
                new TestEntry("item_001"),
                new TestEntry("item_001")
            };

            Result<DataTable<TestEntry>> result = DataTable<TestEntry>.Create(entries);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ValidationFailed));
        }

        [Test]
        public void Create_IdsWithDifferentCase_ReturnsSuccessfulTable()
        {
            TestEntry[] entries =
            {
                new TestEntry("item_001"),
                new TestEntry("Item_001")
            };

            Result<DataTable<TestEntry>> result = DataTable<TestEntry>.Create(entries);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(2));
        }

        [Test]
        public void Contains_ExistingId_ReturnsTrue()
        {
            Result<DataTable<TestEntry>> result = DataTable<TestEntry>.Create(new[]
            {
                new TestEntry("item_001")
            });

            bool contains = result.Value.Contains("item_001");

            Assert.That(contains, Is.True);
        }

        [Test]
        public void Contains_MissingId_ReturnsFalse()
        {
            Result<DataTable<TestEntry>> result = DataTable<TestEntry>.Create(new[]
            {
                new TestEntry("item_001")
            });

            bool contains = result.Value.Contains("item_999");

            Assert.That(contains, Is.False);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase(" item_001")]
        public void Contains_InvalidId_ReturnsFalse(string id)
        {
            Result<DataTable<TestEntry>> result = DataTable<TestEntry>.Create(new[]
            {
                new TestEntry("item_001")
            });

            bool contains = result.Value.Contains(id);

            Assert.That(contains, Is.False);
        }

        [Test]
        public void TryGet_ExistingId_ReturnsEntry()
        {
            TestEntry expected = new TestEntry("item_001");

            Result<DataTable<TestEntry>> result = DataTable<TestEntry>.Create(new[]
            {
                expected
            });

            bool found = result.Value.TryGet("item_001", out TestEntry entry);

            Assert.That(found, Is.True);
            Assert.That(entry, Is.SameAs(expected));
        }

        [Test]
        public void TryGet_MissingId_ReturnsFalse()
        {
            Result<DataTable<TestEntry>> result = DataTable<TestEntry>.Create(new[]
            {
                new TestEntry("item_001")
            });

            bool found = result.Value.TryGet("item_999", out TestEntry entry);

            Assert.That(found, Is.False);
            Assert.That(entry, Is.Null);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("item_001 ")]
        public void TryGet_InvalidId_ReturnsFalse(string id)
        {
            Result<DataTable<TestEntry>> result = DataTable<TestEntry>.Create(new[]
            {
                new TestEntry("item_001")
            });

            bool found = result.Value.TryGet(id, out TestEntry entry);

            Assert.That(found, Is.False);
            Assert.That(entry, Is.Null);
        }

        [Test]
        public void Lookup_IdComparison_IsCaseSensitive()
        {
            TestEntry lowerCaseEntry = new TestEntry("item_001");
            TestEntry upperCaseEntry = new TestEntry("Item_001");

            Result<DataTable<TestEntry>> result = DataTable<TestEntry>.Create(new[]
            {
                lowerCaseEntry,
                upperCaseEntry
            });

            bool lowerFound = result.Value.TryGet("item_001", out TestEntry lowerResult);
            bool upperFound = result.Value.TryGet("Item_001", out TestEntry upperResult);

            Assert.That(lowerFound, Is.True);
            Assert.That(upperFound, Is.True);
            Assert.That(lowerResult, Is.SameAs(lowerCaseEntry));
            Assert.That(upperResult, Is.SameAs(upperCaseEntry));
        }

        [Test]
        public void Get_ExistingId_ReturnsSuccessfulResult()
        {
            TestEntry expected = new TestEntry("item_001");

            Result<DataTable<TestEntry>> tableResult = DataTable<TestEntry>.Create(new[]
            {
                expected
            });

            Result<TestEntry> result = tableResult.Value.Get("item_001");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.SameAs(expected));
        }

        [Test]
        public void Get_MissingId_ReturnsNotFound()
        {
            Result<DataTable<TestEntry>> tableResult = DataTable<TestEntry>.Create(new[]
            {
                new TestEntry("item_001")
            });

            Result<TestEntry> result = tableResult.Value.Get("item_999");

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.NotFound));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase(" item_001")]
        [TestCase("item_001 ")]
        public void Get_InvalidId_ReturnsInvalidId(string id)
        {
            Result<DataTable<TestEntry>> tableResult = DataTable<TestEntry>.Create(new[]
            {
                new TestEntry("item_001")
            });

            Result<TestEntry> result = tableResult.Value.Get(id);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.InvalidId));
        }

        [Test]
        public void Create_SourceListModifiedAfterCreation_DoesNotChangeTable()
        {
            TestEntry first = new TestEntry("item_001");
            TestEntry second = new TestEntry("item_002");

            List<TestEntry> source = new List<TestEntry>
            {
                first,
                second
            };

            Result<DataTable<TestEntry>> result = DataTable<TestEntry>.Create(source);

            source.Clear();
            source.Add(new TestEntry("item_999"));

            Assert.That(result.Value.Count, Is.EqualTo(2));
            Assert.That(result.Value.Entries[0], Is.SameAs(first));
            Assert.That(result.Value.Entries[1], Is.SameAs(second));
        }

        [Test]
        public void Create_SourceArrayModifiedAfterCreation_DoesNotChangeTable()
        {
            TestEntry first = new TestEntry("item_001");
            TestEntry second = new TestEntry("item_002");

            TestEntry[] source =
            {
                first,
                second
            };

            Result<DataTable<TestEntry>> result = DataTable<TestEntry>.Create(source);

            source[0] = new TestEntry("item_999");

            Assert.That(result.Value.Count, Is.EqualTo(2));
            Assert.That(result.Value.Entries[0], Is.SameAs(first));
            Assert.That(result.Value.Entries[1], Is.SameAs(second));
        }

        [Test]
        public void Entries_ModificationThroughIList_ThrowsNotSupportedException()
        {
            TestEntry original = new TestEntry("item_001");

            Result<DataTable<TestEntry>> result = DataTable<TestEntry>.Create(new[]
            {
                original
            });

            IList<TestEntry> entries = (IList<TestEntry>)result.Value.Entries;

            Assert.Throws<NotSupportedException>(() => entries[0] = new TestEntry("item_999"));
            Assert.That(result.Value.Entries[0], Is.SameAs(original));
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