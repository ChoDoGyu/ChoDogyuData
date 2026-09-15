using System;
using System.Collections.Generic;
using System.Reflection;
using CDG.Data;
using CDG.Data.Editor.Importing;
using NUnit.Framework;
using UnityEngine;

namespace CDG.Data.Tests.Editor
{
    public sealed class DataImportStateSnapshotTests
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
        public void Capture_UnchangedState_Matches()
        {
            TestDataTableAsset target = CreateAsset();
            SetEntries(target, new List<TestEntry> { new TestEntry("item_001", 10) });

            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                new TestEntry("item_002", 20)
            });

            DataImportStateSnapshot<TestEntry> snapshot = DataImportStateSnapshot<TestEntry>.Capture(target, candidate);

            Assert.That(snapshot.Matches(target, candidate), Is.True);
        }

        [Test]
        public void Matches_TargetChangedAfterCapture_ReturnsFalse()
        {
            TestDataTableAsset target = CreateAsset();
            TestEntry current = new TestEntry("item_001", 10);

            SetEntries(target, new List<TestEntry> { current });

            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(
                Array.Empty<TestEntry>());

            DataImportStateSnapshot<TestEntry> snapshot = DataImportStateSnapshot<TestEntry>.Capture(target, candidate);

            current.ChangeValue(99);

            Assert.That(snapshot.Matches(target, candidate), Is.False);
        }

        [Test]
        public void Matches_CandidateChangedAfterCapture_ReturnsFalse()
        {
            TestDataTableAsset target = CreateAsset();
            TestEntry incoming = new TestEntry("item_001", 10);

            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                incoming
            });

            DataImportStateSnapshot<TestEntry> snapshot = DataImportStateSnapshot<TestEntry>.Capture(target, candidate);

            incoming.ChangeValue(99);

            Assert.That(snapshot.Matches(target, candidate), Is.False);
        }

        [Test]
        public void Matches_DifferentTargetWithSameData_ReturnsFalse()
        {
            TestDataTableAsset firstTarget = CreateAsset();
            TestDataTableAsset secondTarget = CreateAsset();

            SetEntries(firstTarget, new List<TestEntry> { new TestEntry("item_001", 10) });
            SetEntries(secondTarget, new List<TestEntry> { new TestEntry("item_001", 10) });

            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(
                Array.Empty<TestEntry>());

            DataImportStateSnapshot<TestEntry> snapshot = DataImportStateSnapshot<TestEntry>.Capture(
                firstTarget,
                candidate);

            Assert.That(snapshot.Matches(secondTarget, candidate), Is.False);
        }

        [Test]
        public void HasSameState_EquivalentCaptures_ReturnsTrue()
        {
            TestDataTableAsset target = CreateAsset();

            SetEntries(target, new List<TestEntry> { new TestEntry("item_001", 10) });

            DataImportCandidate<TestEntry> candidate = new DataImportCandidate<TestEntry>(new[]
            {
                new TestEntry("item_002", 20)
            });

            DataImportStateSnapshot<TestEntry> first = DataImportStateSnapshot<TestEntry>.Capture(target, candidate);
            DataImportStateSnapshot<TestEntry> second = DataImportStateSnapshot<TestEntry>.Capture(target, candidate);

            Assert.That(first.HasSameState(second), Is.True);
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