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
    public sealed class DataImportStateSafetyTests
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
        public void Snapshot_UnchangedTargetAndCandidate_MatchesReturnsTrue()
        {
            TestDataTableAsset target = CreateAsset();

            SetEntries(target, new List<TestEntry>
            {
                new TestEntry("item_a", 10)
            });

            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                new TestEntry("item_b", 20)
            });

            DataImportStateSnapshot<TestEntry> snapshot = DataImportStateSnapshot<TestEntry>.Capture(
                target,
                candidate);

            Assert.That(
                snapshot.Matches(target, candidate),
                Is.True);
        }

        [Test]
        public void Snapshot_DifferentTargetInstanceWithSameData_MatchesReturnsFalse()
        {
            TestDataTableAsset firstTarget = CreateAsset();
            TestDataTableAsset secondTarget = CreateAsset();

            SetEntries(firstTarget, new List<TestEntry>
            {
                new TestEntry("item_a", 10)
            });

            SetEntries(secondTarget, new List<TestEntry>
            {
                new TestEntry("item_a", 10)
            });

            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                new TestEntry("item_b", 20)
            });

            DataImportStateSnapshot<TestEntry> snapshot = DataImportStateSnapshot<TestEntry>.Capture(
                firstTarget,
                candidate);

            Assert.That(
                snapshot.Matches(secondTarget, candidate),
                Is.False);
        }

        [Test]
        public void Snapshot_TargetSerializedStateChanged_MatchesReturnsFalse()
        {
            TestDataTableAsset target = CreateAsset();

            SetEntries(target, new List<TestEntry>
            {
                new TestEntry("item_a", 10)
            });

            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                new TestEntry("item_b", 20)
            });

            DataImportStateSnapshot<TestEntry> snapshot = DataImportStateSnapshot<TestEntry>.Capture(
                target,
                candidate);

            SetEntries(target, new List<TestEntry>
            {
                new TestEntry("item_a", 15)
            });

            Assert.That(
                snapshot.Matches(target, candidate),
                Is.False);
        }

        [Test]
        public void Snapshot_CandidateEntryChanged_MatchesReturnsFalse()
        {
            TestDataTableAsset target = CreateAsset();

            TestEntry candidateEntry = new TestEntry(
                "item_b",
                20);

            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                candidateEntry
            });

            DataImportStateSnapshot<TestEntry> snapshot = DataImportStateSnapshot<TestEntry>.Capture(
                target,
                candidate);

            candidateEntry.ChangeValue(25);

            Assert.That(
                snapshot.Matches(target, candidate),
                Is.False);
        }

        [Test]
        public void Snapshot_CandidateOrderChanged_MatchesReturnsFalse()
        {
            TestDataTableAsset target = CreateAsset();

            TestEntry first = new TestEntry(
                "item_a",
                10);

            TestEntry second = new TestEntry(
                "item_b",
                20);

            DataImportCandidate<TestEntry> originalCandidate = new DataImportCandidate<TestEntry>(new[]
            {
                first,
                second
            });

            DataImportStateSnapshot<TestEntry> snapshot = DataImportStateSnapshot<TestEntry>.Capture(
                target,
                originalCandidate);

            DataImportCandidate<TestEntry> reorderedCandidate = new DataImportCandidate<TestEntry>(new[]
            {
                second,
                first
            });

            Assert.That(
                snapshot.Matches(target, reorderedCandidate),
                Is.False);
        }

        [Test]
        public void Snapshot_EquivalentSerializedState_HasSameStateReturnsTrue()
        {
            TestDataTableAsset target = CreateAsset();

            SetEntries(target, new List<TestEntry>
            {
                new TestEntry("target", 5)
            });

            DataImportCandidate<TestEntry> firstCandidate = new DataImportCandidate<TestEntry>(new[]
            {
                new TestEntry("item_a", 10),
                new TestEntry("item_b", 20)
            });

            DataImportCandidate<TestEntry> secondCandidate = new DataImportCandidate<TestEntry>(new[]
            {
                new TestEntry("item_a", 10),
                new TestEntry("item_b", 20)
            });

            DataImportStateSnapshot<TestEntry> firstSnapshot = DataImportStateSnapshot<TestEntry>.Capture(
                target,
                firstCandidate);

            DataImportStateSnapshot<TestEntry> secondSnapshot = DataImportStateSnapshot<TestEntry>.Capture(
                target,
                secondCandidate);

            Assert.That(
                firstSnapshot.HasSameState(secondSnapshot),
                Is.True);
        }

        [Test]
        public void Snapshot_CandidateChangedBetweenCaptures_HasSameStateReturnsFalse()
        {
            TestDataTableAsset target = CreateAsset();

            TestEntry entry = new TestEntry(
                "item_a",
                10);

            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                entry
            });

            DataImportStateSnapshot<TestEntry> beforeSnapshot = DataImportStateSnapshot<TestEntry>.Capture(
                target,
                candidate);

            entry.ChangeValue(20);

            DataImportStateSnapshot<TestEntry> afterSnapshot = DataImportStateSnapshot<TestEntry>.Capture(
                target,
                candidate);

            Assert.That(
                beforeSnapshot.HasSameState(afterSnapshot),
                Is.False);
        }

        [Test]
        public void Apply_CandidateValueChangedAfterPreview_ReturnsImportFailedAndPreservesTarget()
        {
            TestDataTableAsset target = CreateAsset();
            TestEntry original = new TestEntry(
                "item_original",
                10);

            SetEntries(target, new List<TestEntry>
            {
                original
            });

            TestEntry incoming = new TestEntry(
                "item_new",
                20);

            DataImportPreview<TestEntry> preview = CreateValidPreview(
                target,
                new[]
                {
                    incoming
                });

            Assert.That(preview.IsValid, Is.True);
            Assert.That(preview.HasStateSnapshot, Is.True);

            incoming.ChangeValue(25);

            Result result =
                DataImportApplier.Apply(
                    target,
                    preview);

            Assert.That(result.IsFailure, Is.True);

            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));

            Assert.That(target.Count, Is.EqualTo(1));
            Assert.That(target.Entries[0], Is.SameAs(original));
            Assert.That(target.Entries[0].Id, Is.EqualTo("item_original"));
            Assert.That(target.Entries[0].Value, Is.EqualTo(10));
        }

        [Test]
        public void Apply_PreviewCreatedForDifferentTarget_ReturnsImportFailedAndPreservesTarget()
        {
            TestDataTableAsset previewTarget = CreateAsset();
            TestDataTableAsset applyTarget = CreateAsset();

            SetEntries(previewTarget, new List<TestEntry>
            {
                new TestEntry("same", 10)
            });

            TestEntry applyTargetOriginal = new TestEntry(
                "same",
                10);

            SetEntries(applyTarget, new List<TestEntry>
            {
                applyTargetOriginal
            });

            DataImportPreview<TestEntry> preview = CreateValidPreview(
                previewTarget,
                new[]
                {
                    new TestEntry("item_new", 20)
                });

            Result result =
                DataImportApplier.Apply(
                    applyTarget,
                    preview);

            Assert.That(result.IsFailure, Is.True);

            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));

            Assert.That(applyTarget.Count, Is.EqualTo(1));
            Assert.That(applyTarget.Entries[0], Is.SameAs(applyTargetOriginal));
            Assert.That(applyTarget.Entries[0].Id, Is.EqualTo("same"));
            Assert.That(applyTarget.Entries[0].Value, Is.EqualTo(10));
        }

        [Test]
        public void Apply_PreviewWithoutStateSnapshot_ReturnsImportFailedAndPreservesTarget()
        {
            TestDataTableAsset target = CreateAsset();
            TestEntry original = new TestEntry(
                "item_original",
                10);

            SetEntries(target, new List<TestEntry>
            {
                original
            });

            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                new TestEntry("item_new", 20)
            });

            DataValidationReport validationReport =
                DataImportCandidateValidator.Validate(
                    candidate);

            Assert.That(
                validationReport.IsValid,
                Is.True);

            Result<DataImportDiff<TestEntry>> diffResult =
                DataImportDiffBuilder.Build(
                    target.Entries,
                    candidate,
                    new TestComparer());

            Assert.That(
                diffResult.IsSuccess,
                Is.True);

            DataImportPreview<TestEntry> preview = new DataImportPreview<TestEntry>(
                candidate,
                validationReport,
                diffResult.Value);

            Assert.That(preview.HasDiff, Is.True);
            Assert.That(preview.HasStateSnapshot, Is.False);

            Result result =
                DataImportApplier.Apply(
                    target,
                    preview);

            Assert.That(result.IsFailure, Is.True);

            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));

            Assert.That(target.Count, Is.EqualTo(1));
            Assert.That(target.Entries[0], Is.SameAs(original));
            Assert.That(target.Entries[0].Id, Is.EqualTo("item_original"));
            Assert.That(target.Entries[0].Value, Is.EqualTo(10));
        }

        private TestDataTableAsset CreateAsset()
        {
            TestDataTableAsset asset =
                ScriptableObject.CreateInstance<TestDataTableAsset>();

            createdAssets.Add(asset);

            return asset;
        }

        private static DataImportPreview<TestEntry> CreateValidPreview(TestDataTableAsset target, IEnumerable<TestEntry> incomingEntries)
        {
            DataImportCandidate<TestEntry> candidate =
                new DataImportCandidate<TestEntry>(
                    incomingEntries);

            DataValidationReport validationReport =
                DataImportCandidateValidator.Validate(
                    candidate);

            Assert.That(
                validationReport.IsValid,
                Is.True);

            Result<DataImportDiff<TestEntry>> diffResult =
                DataImportDiffBuilder.Build(
                    target.Entries,
                    candidate,
                    new TestComparer());

            Assert.That(
                diffResult.IsSuccess,
                Is.True);

            DataImportStateSnapshot<TestEntry> snapshot =
                DataImportStateSnapshot<TestEntry>.Capture(
                    target,
                    candidate);

            return new DataImportPreview<TestEntry>(
                candidate,
                validationReport,
                diffResult.Value,
                snapshot);
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