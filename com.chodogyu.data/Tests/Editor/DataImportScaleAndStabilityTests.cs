using System;
using System.Collections.Generic;
using System.Reflection;
using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Editor.Importing;
using NUnit.Framework;
using UnityEngine;

namespace CDG.Data.Tests.Editor
{
    public sealed class DataImportScaleAndStabilityTests
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
        public void Build_ThousandEquivalentEntriesInReverseOrder_PreservesCandidateOrder()
        {
            const int count = 1000;

            List<TestEntry> currentEntries = new List<TestEntry>(count);
            List<TestEntry> incomingEntries = new List<TestEntry>(count);

            for (int index = 0; index < count; index++)
            {
                currentEntries.Add(
                    new TestEntry(
                        CreateId(index),
                        index));
            }

            for (int index = count - 1; index >= 0; index--)
            {
                incomingEntries.Add(
                    new TestEntry(
                        CreateId(index),
                        index));
            }

            DataImportCandidate<TestEntry> candidate =
                new DataImportCandidate<TestEntry>(
                    incomingEntries);

            CountingComparer comparer =
                new CountingComparer();

            Result<DataImportDiff<TestEntry>> result =
                DataImportDiffBuilder.Build(
                    currentEntries,
                    candidate,
                    comparer);

            Assert.That(result.IsSuccess, Is.True);

            DataImportDiff<TestEntry> diff =
                result.Value;

            Assert.That(diff.Count, Is.EqualTo(count));
            Assert.That(diff.AddedCount, Is.EqualTo(0));
            Assert.That(diff.RemovedCount, Is.EqualTo(0));
            Assert.That(diff.ModifiedCount, Is.EqualTo(0));
            Assert.That(diff.UnchangedCount, Is.EqualTo(count));
            Assert.That(diff.HasChanges, Is.False);

            Assert.That(
                diff.Items[0].Id,
                Is.EqualTo(CreateId(999)));

            Assert.That(
                diff.Items[count - 1].Id,
                Is.EqualTo(CreateId(0)));

            for (int index = 0; index < count; index++)
            {
                Assert.That(
                    diff.Items[index].Id,
                    Is.EqualTo(incomingEntries[index].Id));

                Assert.That(
                    diff.Items[index].Type,
                    Is.EqualTo(DataImportDiffType.Unchanged));
            }

            Assert.That(
                comparer.CallCount,
                Is.EqualTo(count));
        }

        [Test]
        public void Build_LargeMixedData_ReturnsExpectedCountsAndStableOrder()
        {
            const int currentCount = 1000;

            List<TestEntry> currentEntries =
                new List<TestEntry>(currentCount);

            for (int index = 0; index < currentCount; index++)
            {
                currentEntries.Add(
                    new TestEntry(
                        CreateId(index),
                        index));
            }

            List<TestEntry> incomingEntries =
                new List<TestEntry>(1000);

            for (int index = 999; index >= 500; index--)
            {
                incomingEntries.Add(
                    new TestEntry(
                        CreateId(index),
                        index + 1));
            }

            for (int index = 1000; index < 1500; index++)
            {
                incomingEntries.Add(
                    new TestEntry(
                        CreateId(index),
                        index));
            }

            DataImportCandidate<TestEntry> candidate =
                new DataImportCandidate<TestEntry>(
                    incomingEntries);

            CountingComparer comparer =
                new CountingComparer();

            Result<DataImportDiff<TestEntry>> result =
                DataImportDiffBuilder.Build(
                    currentEntries,
                    candidate,
                    comparer);

            Assert.That(result.IsSuccess, Is.True);

            DataImportDiff<TestEntry> diff =
                result.Value;

            Assert.That(diff.Count, Is.EqualTo(1500));

            Assert.That(
                diff.ModifiedCount,
                Is.EqualTo(500));

            Assert.That(
                diff.AddedCount,
                Is.EqualTo(500));

            Assert.That(
                diff.RemovedCount,
                Is.EqualTo(500));

            Assert.That(
                diff.UnchangedCount,
                Is.EqualTo(0));

            Assert.That(diff.HasChanges, Is.True);

            Assert.That(
                comparer.CallCount,
                Is.EqualTo(500));

            Assert.That(
                diff.Items[0].Id,
                Is.EqualTo(CreateId(999)));

            Assert.That(
                diff.Items[0].Type,
                Is.EqualTo(DataImportDiffType.Modified));

            Assert.That(
                diff.Items[499].Id,
                Is.EqualTo(CreateId(500)));

            Assert.That(
                diff.Items[499].Type,
                Is.EqualTo(DataImportDiffType.Modified));

            Assert.That(
                diff.Items[500].Id,
                Is.EqualTo(CreateId(1000)));

            Assert.That(
                diff.Items[500].Type,
                Is.EqualTo(DataImportDiffType.Added));

            Assert.That(
                diff.Items[999].Id,
                Is.EqualTo(CreateId(1499)));

            Assert.That(
                diff.Items[999].Type,
                Is.EqualTo(DataImportDiffType.Added));

            Assert.That(
                diff.Items[1000].Id,
                Is.EqualTo(CreateId(0)));

            Assert.That(
                diff.Items[1000].Type,
                Is.EqualTo(DataImportDiffType.Removed));

            Assert.That(
                diff.Items[1499].Id,
                Is.EqualTo(CreateId(499)));

            Assert.That(
                diff.Items[1499].Type,
                Is.EqualTo(DataImportDiffType.Removed));
        }

        [Test]
        public void Build_IdCaseDifference_TreatsEntriesAsAddedAndRemoved()
        {
            TestEntry current =
                new TestEntry(
                    "item_sword",
                    10);

            TestEntry incoming =
                new TestEntry(
                    "Item_Sword",
                    10);

            DataImportCandidate<TestEntry> candidate =
                new DataImportCandidate<TestEntry>(
                    new[]
                    {
                        incoming
                    });

            CountingComparer comparer =
                new CountingComparer();

            Result<DataImportDiff<TestEntry>> result =
                DataImportDiffBuilder.Build(
                    new[]
                    {
                        current
                    },
                    candidate,
                    comparer);

            Assert.That(result.IsSuccess, Is.True);

            Assert.That(
                result.Value.Count,
                Is.EqualTo(2));

            Assert.That(
                result.Value.AddedCount,
                Is.EqualTo(1));

            Assert.That(
                result.Value.RemovedCount,
                Is.EqualTo(1));

            Assert.That(
                result.Value.ModifiedCount,
                Is.EqualTo(0));

            Assert.That(
                result.Value.UnchangedCount,
                Is.EqualTo(0));

            Assert.That(
                result.Value.Items[0].Id,
                Is.EqualTo("Item_Sword"));

            Assert.That(
                result.Value.Items[0].Type,
                Is.EqualTo(DataImportDiffType.Added));

            Assert.That(
                result.Value.Items[1].Id,
                Is.EqualTo("item_sword"));

            Assert.That(
                result.Value.Items[1].Type,
                Is.EqualTo(DataImportDiffType.Removed));

            Assert.That(
                comparer.CallCount,
                Is.EqualTo(0));
        }

        [Test]
        public void Import_CandidateChangedDuringDiff_ReturnsImportFailed()
        {
            TestDataTableAsset target =
                CreateAsset();

            SetEntries(
                target,
                new List<TestEntry>
                {
                    new TestEntry(
                        "item_001",
                        10)
                });

            TestEntry incoming =
                new TestEntry(
                    "item_001",
                    20);

            DataImportCandidate<TestEntry> candidate =
                new DataImportCandidate<TestEntry>(
                    new[]
                    {
                        incoming
                    });

            StubImporter importer =
                new StubImporter(
                    Result<DataImportCandidate<TestEntry>>.Success(
                        candidate));

            CandidateMutatingComparer comparer =
                new CandidateMutatingComparer();

            Result<DataImportPreview<TestEntry>> result =
                DataImportProcessor.Import(
                    "source",
                    importer,
                    target,
                    comparer);

            Assert.That(result.IsFailure, Is.True);

            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));

            Assert.That(
                incoming.Value,
                Is.EqualTo(21));

            Assert.That(
                target.Count,
                Is.EqualTo(1));

            Assert.That(
                target.Entries[0].Value,
                Is.EqualTo(10));

            Assert.That(
                comparer.CallCount,
                Is.EqualTo(1));
        }

        [Test]
        public void Import_TargetChangedDuringDiff_ReturnsImportFailed()
        {
            TestDataTableAsset target =
                CreateAsset();

            TestEntry current =
                new TestEntry(
                    "item_001",
                    10);

            SetEntries(
                target,
                new List<TestEntry>
                {
                    current
                });

            DataImportCandidate<TestEntry> candidate =
                new DataImportCandidate<TestEntry>(
                    new[]
                    {
                        new TestEntry(
                            "item_001",
                            20)
                    });

            StubImporter importer =
                new StubImporter(
                    Result<DataImportCandidate<TestEntry>>.Success(
                        candidate));

            TargetMutatingComparer comparer =
                new TargetMutatingComparer();

            Result<DataImportPreview<TestEntry>> result =
                DataImportProcessor.Import(
                    "source",
                    importer,
                    target,
                    comparer);

            Assert.That(result.IsFailure, Is.True);

            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));

            Assert.That(
                current.Value,
                Is.EqualTo(11));

            Assert.That(
                comparer.CallCount,
                Is.EqualTo(1));
        }

        private TestDataTableAsset CreateAsset()
        {
            TestDataTableAsset asset =
                ScriptableObject.CreateInstance<TestDataTableAsset>();

            createdAssets.Add(asset);

            return asset;
        }

        private static string CreateId(int index)
        {
            return $"item_{index:D4}";
        }

        private static void SetEntries(TestDataTableAsset asset, List<TestEntry> entries)
        {
            FieldInfo field =
                typeof(DataTableAsset<TestEntry>).GetField(
                    "entries",
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

            Assert.That(
                field,
                Is.Not.Null);

            field.SetValue(
                asset,
                entries);
        }

        private sealed class StubImporter : IDataTextImporter<TestEntry>
        {
            private readonly Result<DataImportCandidate<TestEntry>> result;

            public StubImporter(Result<DataImportCandidate<TestEntry>> result)
            {
                this.result = result;
            }

            public Result<DataImportCandidate<TestEntry>> Import(string text)
            {
                return result;
            }
        }

        private sealed class CountingComparer : IDataImportEntryComparer<TestEntry>
        {
            public int CallCount { get; private set; }

            public bool AreEquivalent(TestEntry current, TestEntry incoming)
            {
                CallCount++;

                return current.Value == incoming.Value;
            }
        }

        private sealed class CandidateMutatingComparer : IDataImportEntryComparer<TestEntry>
        {
            public int CallCount { get; private set; }

            public bool AreEquivalent(TestEntry current, TestEntry incoming)
            {
                CallCount++;

                incoming.ChangeValue(
                    incoming.Value + 1);

                return false;
            }
        }

        private sealed class TargetMutatingComparer : IDataImportEntryComparer<TestEntry>
        {
            public int CallCount { get; private set; }

            public bool AreEquivalent(TestEntry current, TestEntry incoming)
            {
                CallCount++;

                current.ChangeValue(
                    current.Value + 1);

                return false;
            }
        }

        [Serializable]
        private sealed class TestEntry : IDataEntry
        {
            [SerializeField]
            private string id;

            [SerializeField]
            private int value;

            public string Id => id;
            public int Value => value;

            public TestEntry(string id, int value)
            {
                this.id = id;
                this.value = value;
            }

            public void ChangeValue(int newValue)
            {
                value = newValue;
            }
        }

        private sealed class TestDataTableAsset : DataTableAsset<TestEntry>
        {
        }
    }
}