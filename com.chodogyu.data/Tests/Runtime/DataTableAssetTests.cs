using System;
using System.Collections.Generic;
using System.Reflection;
using CDG.Core.Results;
using CDG.Data.Validation;
using NUnit.Framework;
using UnityEngine;

namespace CDG.Data.Tests.Runtime
{
    public sealed class DataTableAssetTests
    {
        private readonly List<TestDataTableAsset> createdAssets = new List<TestDataTableAsset>();

        [TearDown]
        public void TearDown()
        {
            foreach (TestDataTableAsset asset in createdAssets)
            {
                if (asset != null)
                {
                    UnityEngine.Object.DestroyImmediate(asset);
                }
            }

            createdAssets.Clear();
        }

        [Test]
        public void CreateInstance_EmptyAsset_HasExpectedInitialState()
        {
            TestDataTableAsset asset = CreateAsset();

            Assert.That(asset.Count, Is.EqualTo(0));
            Assert.That(asset.Entries, Is.Empty);
        }

        [Test]
        public void Validate_EmptyAsset_ReturnsValidReport()
        {
            TestDataTableAsset asset = CreateAsset();

            DataValidationReport report = asset.Validate();

            Assert.That(report.IsValid, Is.True);
            Assert.That(report.Count, Is.EqualTo(0));
        }

        [Test]
        public void Build_EmptyAsset_ReturnsSuccessfulEmptyTable()
        {
            TestDataTableAsset asset = CreateAsset();

            Result<DataTable<TestEntry>> result = asset.Build();

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(0));
            Assert.That(result.Value.Entries, Is.Empty);
        }

        [Test]
        public void Build_ValidEntries_ReturnsTableWithOriginalOrder()
        {
            TestDataTableAsset asset = CreateAsset();

            TestEntry first = new TestEntry("item_001");
            TestEntry second = new TestEntry("item_002");

            SetEntries(asset, new List<TestEntry>
            {
                first,
                second
            });

            Result<DataTable<TestEntry>> result = asset.Build();

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(2));
            Assert.That(result.Value.Entries[0], Is.SameAs(first));
            Assert.That(result.Value.Entries[1], Is.SameAs(second));
        }

        [Test]
        public void Validate_InvalidEntries_CollectsAllIssues()
        {
            TestDataTableAsset asset = CreateAsset();

            SetEntries(asset, new List<TestEntry>
            {
                null,
                new TestEntry(" invalid"),
                new TestEntry("item_001"),
                new TestEntry("item_001")
            });

            DataValidationReport report = asset.Validate();

            Assert.That(report.IsValid, Is.False);
            Assert.That(report.Count, Is.EqualTo(3));
            Assert.That(report.Issues[0].Type, Is.EqualTo(DataValidationIssueType.NullEntry));
            Assert.That(report.Issues[1].Type, Is.EqualTo(DataValidationIssueType.InvalidId));
            Assert.That(report.Issues[2].Type, Is.EqualTo(DataValidationIssueType.DuplicateId));
        }

        [Test]
        public void Build_InvalidEntries_ReturnsValidationFailed()
        {
            TestDataTableAsset asset = CreateAsset();

            SetEntries(asset, new List<TestEntry>
            {
                new TestEntry("item_001"),
                new TestEntry("item_001")
            });

            Result<DataTable<TestEntry>> result = asset.Build();

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ValidationFailed));
        }

        [Test]
        public void Entries_ModificationThroughIList_ThrowsNotSupportedException()
        {
            TestDataTableAsset asset = CreateAsset();
            TestEntry original = new TestEntry("item_001");

            SetEntries(asset, new List<TestEntry>
            {
                original
            });

            IList<TestEntry> entries = (IList<TestEntry>)asset.Entries;

            Assert.Throws<NotSupportedException>(() => entries[0] = new TestEntry("item_999"));
            Assert.That(asset.Entries[0], Is.SameAs(original));
        }

        private TestDataTableAsset CreateAsset()
        {
            TestDataTableAsset asset = ScriptableObject.CreateInstance<TestDataTableAsset>();
            createdAssets.Add(asset);
            return asset;
        }

        private static void SetEntries(TestDataTableAsset asset, List<TestEntry> entries)
        {
            FieldInfo field = typeof(DataTableAsset<TestEntry>).GetField("entries", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(field, Is.Not.Null);

            field.SetValue(asset, entries);
        }

        [Serializable]
        private sealed class TestEntry : IDataEntry
        {
            [SerializeField]
            private string id;

            public string Id => id;

            public TestEntry(string id)
            {
                this.id = id;
            }
        }

        private sealed class TestDataTableAsset : DataTableAsset<TestEntry>
        {
        }

        [Test]
        public void Entries_InternalListReplacedAfterFirstAccess_ReturnsLatestEntries()
        {
            TestDataTableAsset asset = CreateAsset();

            TestEntry first = new TestEntry("item_001");

            SetEntries(asset, new List<TestEntry>
            {
                first
            });

            IReadOnlyList<TestEntry> firstView = asset.Entries;

            TestEntry second = new TestEntry("item_002");

            SetEntries(asset, new List<TestEntry>
            {
                second
            });

            IReadOnlyList<TestEntry> secondView = asset.Entries;

            Assert.That(firstView.Count, Is.EqualTo(1));
            Assert.That(firstView[0], Is.SameAs(first));

            Assert.That(secondView.Count, Is.EqualTo(1));
            Assert.That(secondView[0], Is.SameAs(second));
        }

        [Test]
        public void Build_ValidEntries_CreatesSnapshotIndependentFromLaterAssetChanges()
        {
            TestDataTableAsset asset = CreateAsset();

            TestEntry first = new TestEntry("item_001");

            SetEntries(asset, new List<TestEntry>
            {
                first
            });

            Result<DataTable<TestEntry>> result = asset.Build();

            Assert.That(result.IsSuccess, Is.True);

            TestEntry second = new TestEntry("item_002");

            SetEntries(asset, new List<TestEntry>
            {
                second
            });

            Assert.That(asset.Count, Is.EqualTo(1));
            Assert.That(asset.Entries[0], Is.SameAs(second));

            Assert.That(result.Value.Count, Is.EqualTo(1));
            Assert.That(result.Value.Entries[0], Is.SameAs(first));
            Assert.That(result.Value.Contains("item_001"), Is.True);
            Assert.That(result.Value.Contains("item_002"), Is.False);
        }

        [Test]
        public void Build_InvalidEntriesReplacedWithValidEntries_Succeeds()
        {
            TestDataTableAsset asset = CreateAsset();

            SetEntries(asset, new List<TestEntry>
            {
                new TestEntry("item_001"),
                new TestEntry("item_001")
            });

            Result<DataTable<TestEntry>> invalidResult = asset.Build();

            Assert.That(invalidResult.IsFailure, Is.True);
            Assert.That(invalidResult.Error.Code, Is.EqualTo(DataErrorCodes.ValidationFailed));

            SetEntries(asset, new List<TestEntry>
            {
                new TestEntry("item_001"),
                new TestEntry("item_002")
            });

            Result<DataTable<TestEntry>> validResult = asset.Build();

            Assert.That(validResult.IsSuccess, Is.True);
            Assert.That(validResult.Value.Count, Is.EqualTo(2));
            Assert.That(validResult.Value.Contains("item_001"), Is.True);
            Assert.That(validResult.Value.Contains("item_002"), Is.True);
        }

        [Test]
        public void Build_ValidEntriesReplacedWithInvalidEntries_ReturnsValidationFailed()
        {
            TestDataTableAsset asset = CreateAsset();

            SetEntries(asset, new List<TestEntry>
            {
                new TestEntry("item_001"),
                new TestEntry("item_002")
            });

            Result<DataTable<TestEntry>> validResult = asset.Build();

            Assert.That(validResult.IsSuccess, Is.True);

            SetEntries(asset, new List<TestEntry>
            {
                new TestEntry("item_001"),
                new TestEntry("item_001")
            });

            Result<DataTable<TestEntry>> invalidResult = asset.Build();

            Assert.That(invalidResult.IsFailure, Is.True);
            Assert.That(invalidResult.Error.Code, Is.EqualTo(DataErrorCodes.ValidationFailed));
        }
    }
}