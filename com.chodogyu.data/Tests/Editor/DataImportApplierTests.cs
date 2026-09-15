using System;
using System.Collections.Generic;
using System.Reflection;
using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Editor.Importing;
using CDG.Data.Validation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CDG.Data.Tests.Editor
{
    public sealed class DataImportApplierTests
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
        public void Apply_ValidPreview_ReplacesTargetEntriesInCandidateOrder()
        {
            TestDataTableAsset target = CreateAsset();

            SetEntries(target, new List<TestEntry>
            {
                new TestEntry("item_a", 10),
                new TestEntry("item_b", 20)
            });

            TestEntry incomingB = new TestEntry("item_b", 25);
            TestEntry incomingC = new TestEntry("item_c", 30);

            DataImportPreview<TestEntry> preview = CreateValidPreview(
                target,
                new[]
                {
                    incomingB,
                    incomingC
                });

            Result result = DataImportApplier.Apply(target, preview);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(target.Count, Is.EqualTo(2));

            Assert.That(target.Entries[0], Is.SameAs(incomingB));
            Assert.That(target.Entries[1], Is.SameAs(incomingC));

            Assert.That(target.Entries[0].Id, Is.EqualTo("item_b"));
            Assert.That(target.Entries[1].Id, Is.EqualTo("item_c"));
        }

        [Test]
        public void Apply_EmptyCandidate_ClearsTargetEntries()
        {
            TestDataTableAsset target = CreateAsset();

            SetEntries(target, new List<TestEntry>
            {
                new TestEntry("item_001", 10)
            });

            DataImportPreview<TestEntry> preview = CreateValidPreview(
                target,
                Array.Empty<TestEntry>());

            Result result = DataImportApplier.Apply(target, preview);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(target.Count, Is.EqualTo(0));
            Assert.That(target.Entries, Is.Empty);
        }

        [Test]
        public void Apply_InvalidPreview_ReturnsValidationFailedAndPreservesTarget()
        {
            TestDataTableAsset target = CreateAsset();
            TestEntry original = new TestEntry("item_original", 10);

            SetEntries(target, new List<TestEntry>
            {
                original
            });

            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                new TestEntry("item_001", 20),
                new TestEntry("item_001", 30)
            });

            DataValidationReport validationReport = DataImportCandidateValidator.Validate(candidate);
            DataImportPreview<TestEntry> preview = new DataImportPreview<TestEntry>(candidate, validationReport);

            Result result = DataImportApplier.Apply(target, preview);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ValidationFailed));

            Assert.That(target.Count, Is.EqualTo(1));
            Assert.That(target.Entries[0], Is.SameAs(original));
        }

        [Test]
        public void Apply_PreviewWithoutDiff_ReturnsImportFailedAndPreservesTarget()
        {
            TestDataTableAsset target = CreateAsset();
            TestEntry original = new TestEntry("item_original", 10);

            SetEntries(target, new List<TestEntry>
            {
                original
            });

            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                new TestEntry("item_new", 20)
            });

            DataValidationReport validationReport = DataImportCandidateValidator.Validate(candidate);

            Assert.That(validationReport.IsValid, Is.True);

            DataImportPreview<TestEntry> preview = new DataImportPreview<TestEntry>(
                candidate,
                validationReport);

            Result result = DataImportApplier.Apply(target, preview);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));

            Assert.That(target.Count, Is.EqualTo(1));
            Assert.That(target.Entries[0], Is.SameAs(original));
        }

        [Test]
        public void Apply_CandidateBecameInvalidAfterPreview_ReturnsValidationFailedAndPreservesTarget()
        {
            TestDataTableAsset target = CreateAsset();
            TestEntry original = new TestEntry("item_original", 10);

            SetEntries(target, new List<TestEntry>
            {
                original
            });

            TestEntry first = new TestEntry("item_001", 20);
            TestEntry second = new TestEntry("item_002", 30);

            DataImportPreview<TestEntry> preview = CreateValidPreview(
                target,
                new[]
                {
                    first,
                    second
                });

            Assert.That(preview.IsValid, Is.True);
            Assert.That(preview.HasDiff, Is.True);
            Assert.That(preview.HasStateSnapshot, Is.True);

            second.ChangeId("item_001");

            Result result = DataImportApplier.Apply(target, preview);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ValidationFailed));

            Assert.That(target.Count, Is.EqualTo(1));
            Assert.That(target.Entries[0], Is.SameAs(original));
        }

        [Test]
        public void Apply_ValidPreview_MarksTargetDirty()
        {
            TestDataTableAsset target = CreateAsset();

            SetEntries(target, new List<TestEntry>
            {
                new TestEntry("item_original", 10)
            });

            DataImportPreview<TestEntry> preview = CreateValidPreview(
                target,
                new[]
                {
                    new TestEntry("item_new", 20)
                });

            EditorUtility.ClearDirty(target);

            Assert.That(EditorUtility.IsDirty(target), Is.False);

            Result result = DataImportApplier.Apply(target, preview);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(EditorUtility.IsDirty(target), Is.True);
        }

        [Test]
        public void Apply_ValidPreview_UndoRestoresPreviousEntries()
        {
            TestDataTableAsset target = CreateAsset();
            TestEntry original = new TestEntry("item_original", 10);

            SetEntries(target, new List<TestEntry>
            {
                original
            });

            DataImportPreview<TestEntry> preview = CreateValidPreview(
                target,
                new[]
                {
                    new TestEntry("item_new", 20)
                });

            Undo.ClearUndo(target);

            Result result = DataImportApplier.Apply(target, preview);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(target.Count, Is.EqualTo(1));
            Assert.That(target.Entries[0].Id, Is.EqualTo("item_new"));

            Undo.PerformUndo();

            Assert.That(target.Count, Is.EqualTo(1));
            Assert.That(target.Entries[0].Id, Is.EqualTo("item_original"));
            Assert.That(target.Entries[0].Value, Is.EqualTo(10));
        }

        [Test]
        public void Apply_NullTarget_ThrowsArgumentNullException()
        {
            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(
                Array.Empty<TestEntry>());

            DataValidationReport validationReport = DataImportCandidateValidator.Validate(candidate);

            DataImportDiff<TestEntry> diff = new DataImportDiff<TestEntry>(
                Array.Empty<DataImportDiffItem<TestEntry>>());

            DataImportPreview<TestEntry> preview = new DataImportPreview<TestEntry>(
                candidate,
                validationReport,
                diff);

            Assert.Throws<ArgumentNullException>(() =>
                DataImportApplier.Apply<TestEntry>(
                    null,
                    preview));
        }

        [Test]
        public void Apply_NullPreview_ThrowsArgumentNullException()
        {
            TestDataTableAsset target = CreateAsset();

            Assert.Throws<ArgumentNullException>(() =>
                DataImportApplier.Apply<TestEntry>(
                    target,
                    null));
        }

        private TestDataTableAsset CreateAsset()
        {
            TestDataTableAsset asset = ScriptableObject.CreateInstance<TestDataTableAsset>();
            createdAssets.Add(asset);
            return asset;
        }

        private static DataImportPreview<TestEntry> CreateValidPreview(TestDataTableAsset target, IEnumerable<TestEntry> incomingEntries)
        {
            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(incomingEntries);
            DataValidationReport validationReport = DataImportCandidateValidator.Validate(candidate);

            Assert.That(validationReport.IsValid, Is.True);

            Result<DataImportDiff<TestEntry>> diffResult = DataImportDiffBuilder.Build(
                target.Entries,
                candidate,
                new TestComparer());

            Assert.That(diffResult.IsSuccess, Is.True);

            DataImportStateSnapshot<TestEntry> stateSnapshot = DataImportStateSnapshot<TestEntry>.Capture(
                target,
                candidate);

            return new DataImportPreview<TestEntry>(
                candidate,
                validationReport,
                diffResult.Value,
                stateSnapshot);
        }

        private static void SetEntries(TestDataTableAsset asset, List<TestEntry> entries)
        {
            FieldInfo field = typeof(DataTableAsset<TestEntry>).GetField(
                "entries",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(field, Is.Not.Null);

            field.SetValue(asset, entries);
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

            public void ChangeId(string newId)
            {
                id = newId;
            }
        }

        private sealed class TestDataTableAsset : DataTableAsset<TestEntry>
        {
        }
    }
}