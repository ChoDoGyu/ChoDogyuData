using System;
using System.Collections.Generic;
using System.Reflection;
using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Editor.Importing;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CDG.Data.Tests.Editor
{
    public sealed class DataImportApplyStaleStateTests
    {
        private readonly List<TestDataTableAsset> createdAssets = new List<TestDataTableAsset>();

        [TearDown]
        public void TearDown()
        {
            foreach (TestDataTableAsset asset in createdAssets)
            {
                if (asset != null)
                {
                    Undo.ClearUndo(asset);
                    UnityEngine.Object.DestroyImmediate(asset);
                }
            }

            createdAssets.Clear();
        }

        [Test]
        public void Apply_TargetChangedAfterPreview_ReturnsImportFailedAndPreservesLatestTarget()
        {
            TestDataTableAsset target = CreateAsset();
            TestEntry current = new TestEntry("item_001", 10);

            SetEntries(target, new List<TestEntry>
            {
                current
            });

            DataImportPreview<TestEntry> preview = CreatePreview(
                target,
                new TestEntry("item_001", 20));

            current.ChangeValue(15);

            Result result = DataImportApplier.Apply(target, preview);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
            Assert.That(target.Entries[0].Value, Is.EqualTo(15));
        }

        [Test]
        public void Apply_CandidateChangedButStillValid_ReturnsImportFailedAndPreservesTarget()
        {
            TestDataTableAsset target = CreateAsset();

            SetEntries(target, new List<TestEntry>
            {
                new TestEntry("item_001", 10)
            });

            TestEntry incoming = new TestEntry("item_001", 20);
            DataImportPreview<TestEntry> preview = CreatePreview(target, incoming);

            incoming.ChangeValue(25);

            Result result = DataImportApplier.Apply(target, preview);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
            Assert.That(target.Entries[0].Value, Is.EqualTo(10));
        }

        [Test]
        public void Apply_ValidPreviewWithoutStateSnapshot_ReturnsImportFailed()
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

            Result<DataImportDiff<TestEntry>> diffResult = DataImportDiffBuilder.Build(
                target.Entries,
                candidate,
                new TestComparer());

            Assert.That(diffResult.IsSuccess, Is.True);

            DataImportPreview<TestEntry> preview = new DataImportPreview<TestEntry>(
                candidate,
                DataImportCandidateValidator.Validate(candidate),
                diffResult.Value);

            Result result = DataImportApplier.Apply(target, preview);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
            Assert.That(target.Entries[0].Value, Is.EqualTo(10));
        }

        [Test]
        public void Apply_UndoThenRedo_RestoresBothStates()
        {
            TestDataTableAsset target = CreateAsset();

            SetEntries(target, new List<TestEntry>
            {
                new TestEntry("item_001", 10)
            });

            DataImportPreview<TestEntry> preview = CreatePreview(
                target,
                new TestEntry("item_001", 20));

            Undo.ClearUndo(target);

            Result result = DataImportApplier.Apply(target, preview);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(target.Entries[0].Value, Is.EqualTo(20));

            Undo.PerformUndo();

            Assert.That(target.Entries[0].Value, Is.EqualTo(10));

            Undo.PerformRedo();

            Assert.That(target.Entries[0].Value, Is.EqualTo(20));
        }

        private TestDataTableAsset CreateAsset()
        {
            TestDataTableAsset asset = ScriptableObject.CreateInstance<TestDataTableAsset>();
            createdAssets.Add(asset);
            return asset;
        }

        private static DataImportPreview<TestEntry> CreatePreview(TestDataTableAsset target, TestEntry incoming)
        {
            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                incoming
            });

            StubImporter importer = new StubImporter(
                Result<DataImportCandidate<TestEntry>>.Success(candidate));

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import(
                "source",
                importer,
                target,
                new TestComparer());

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.HasStateSnapshot, Is.True);

            return result.Value;
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