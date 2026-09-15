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
    public sealed class DataImportTargetAwareProcessorTests
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
        public void Import_ValidCandidateWithTarget_ReturnsDiffAndStateSnapshot()
        {
            TestDataTableAsset target = CreateAsset();

            SetEntries(target, new List<TestEntry>
            {
                new TestEntry("item_001", 10)
            });

            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                new TestEntry("item_001", 20)
            });

            StubImporter importer = new StubImporter(
                Result<DataImportCandidate<TestEntry>>.Success(candidate));

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import(
                "source",
                importer,
                target,
                new TestComparer());

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.IsValid, Is.True);
            Assert.That(result.Value.HasDiff, Is.True);
            Assert.That(result.Value.HasStateSnapshot, Is.True);
            Assert.That(result.Value.StateSnapshot.Matches(target, candidate), Is.True);
        }

        [Test]
        public void Import_InvalidCandidate_ReturnsPreviewWithoutDiffOrStateSnapshot()
        {
            TestDataTableAsset target = CreateAsset();

            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                new TestEntry("item_001", 10),
                new TestEntry("item_001", 20)
            });

            StubImporter importer = new StubImporter(
                Result<DataImportCandidate<TestEntry>>.Success(candidate));

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import(
                "source",
                importer,
                target,
                new TestComparer());

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.IsValid, Is.False);
            Assert.That(result.Value.HasDiff, Is.False);
            Assert.That(result.Value.HasStateSnapshot, Is.False);
        }

        [Test]
        public void Import_ComparerChangesTargetDuringDiff_ReturnsImportFailed()
        {
            TestDataTableAsset target = CreateAsset();
            TestEntry current = new TestEntry("item_001", 10);

            SetEntries(target, new List<TestEntry>
            {
                current
            });

            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                new TestEntry("item_001", 20)
            });

            StubImporter importer = new StubImporter(
                Result<DataImportCandidate<TestEntry>>.Success(candidate));

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import(
                "source",
                importer,
                target,
                new MutatingComparer());

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Import_NullTarget_ThrowsArgumentNullException()
        {
            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(
                Array.Empty<TestEntry>());

            StubImporter importer = new StubImporter(
                Result<DataImportCandidate<TestEntry>>.Success(candidate));

            Assert.Throws<ArgumentNullException>(() =>
                DataImportProcessor.Import<TestEntry>(
                    "source",
                    importer,
                    (DataTableAsset<TestEntry>)null,
                    new TestComparer()));
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

        private sealed class TestComparer : IDataImportEntryComparer<TestEntry>
        {
            public bool AreEquivalent(TestEntry current, TestEntry incoming)
            {
                return current.Value == incoming.Value;
            }
        }

        private sealed class MutatingComparer : IDataImportEntryComparer<TestEntry>
        {
            public bool AreEquivalent(TestEntry current, TestEntry incoming)
            {
                current.ChangeValue(current.Value + 1);
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