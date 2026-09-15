using System;
using System.Collections.Generic;
using System.Reflection;
using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Editor;
using CDG.Data.Editor.Importing;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CDG.Data.Tests.Editor
{
    public sealed class DataImportSessionTests
    {
        private readonly List<TestDataTableAsset> createdAssets =
            new List<TestDataTableAsset>();

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
        public void CreatePreview_Csv_ReturnsModifiedDiffAndApplyablePreview()
        {
            TestDataTableAsset target = CreateAsset();

            SetEntries(
                target,
                new List<TestEntry>
                {
                    new TestEntry("item_001", 10)
                });

            IDataImportSession session =
                CreateSession(target);

            Result<DataImportSessionPreview> result =
                session.CreatePreview(
                    "id,value\nitem_001,20",
                    DataImportFormat.Csv);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(session.HasPreview, Is.True);
            Assert.That(session.CurrentPreview, Is.SameAs(result.Value));

            Assert.That(result.Value.IsValid, Is.True);
            Assert.That(result.Value.HasDiff, Is.True);
            Assert.That(result.Value.HasStateSnapshot, Is.True);
            Assert.That(result.Value.CanApply, Is.True);

            Assert.That(result.Value.CandidateCount, Is.EqualTo(1));
            Assert.That(result.Value.AddedCount, Is.EqualTo(0));
            Assert.That(result.Value.RemovedCount, Is.EqualTo(0));
            Assert.That(result.Value.ModifiedCount, Is.EqualTo(1));
            Assert.That(result.Value.UnchangedCount, Is.EqualTo(0));

            Assert.That(result.Value.DiffItems.Count, Is.EqualTo(1));
            Assert.That(
                result.Value.DiffItems[0].Id,
                Is.EqualTo("item_001"));

            Assert.That(
                result.Value.DiffItems[0].Type,
                Is.EqualTo(DataImportDiffType.Modified));
        }

        [Test]
        public void CreatePreview_Json_ReturnsModifiedDiffAndApplyablePreview()
        {
            TestDataTableAsset target = CreateAsset();

            SetEntries(
                target,
                new List<TestEntry>
                {
                    new TestEntry("item_001", 10)
                });

            IDataImportSession session =
                CreateSession(target);

            Result<DataImportSessionPreview> result =
                session.CreatePreview(
                    "[{\"id\":\"item_001\",\"value\":20}]",
                    DataImportFormat.Json);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.IsValid, Is.True);
            Assert.That(result.Value.CanApply, Is.True);
            Assert.That(result.Value.ModifiedCount, Is.EqualTo(1));
        }

        [Test]
        public void CreatePreview_DuplicateIds_ReturnsSuccessfulInvalidPreview()
        {
            TestDataTableAsset target = CreateAsset();
            IDataImportSession session = CreateSession(target);

            Result<DataImportSessionPreview> result =
                session.CreatePreview(
                    "id,value\nitem_001,10\nitem_001,20",
                    DataImportFormat.Csv);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(session.HasPreview, Is.True);

            Assert.That(result.Value.IsValid, Is.False);
            Assert.That(result.Value.ValidationReport.Count, Is.EqualTo(1));
            Assert.That(result.Value.HasDiff, Is.False);
            Assert.That(result.Value.HasStateSnapshot, Is.False);
            Assert.That(result.Value.CanApply, Is.False);
        }

        [Test]
        public void CreatePreview_FailureAfterExistingPreview_ClearsOldPreview()
        {
            TestDataTableAsset target = CreateAsset();
            IDataImportSession session = CreateSession(target);

            Result<DataImportSessionPreview> firstResult =
                session.CreatePreview(
                    "id,value\nitem_001,10",
                    DataImportFormat.Csv);

            Assert.That(firstResult.IsSuccess, Is.True);
            Assert.That(session.HasPreview, Is.True);

            Result<DataImportSessionPreview> secondResult =
                session.CreatePreview(
                    "   ",
                    DataImportFormat.Csv);

            Assert.That(secondResult.IsFailure, Is.True);
            Assert.That(session.HasPreview, Is.False);
            Assert.That(session.CurrentPreview, Is.Null);
        }

        [Test]
        public void ClearPreview_AfterSuccessfulPreview_RemovesPreview()
        {
            TestDataTableAsset target = CreateAsset();
            IDataImportSession session = CreateSession(target);

            Result<DataImportSessionPreview> result =
                session.CreatePreview(
                    "id,value\nitem_001,10",
                    DataImportFormat.Csv);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(session.HasPreview, Is.True);

            session.ClearPreview();

            Assert.That(session.HasPreview, Is.False);
            Assert.That(session.CurrentPreview, Is.Null);
        }

        [Test]
        public void CreatePreview_UnsupportedFormat_ReturnsImportFailed()
        {
            TestDataTableAsset target = CreateAsset();
            IDataImportSession session = CreateSession(target);

            Result<DataImportSessionPreview> result =
                session.CreatePreview(
                    "source",
                    (DataImportFormat)999);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));

            Assert.That(session.HasPreview, Is.False);
        }

        [Test]
        public void ApplyPreview_ValidPreview_AppliesEntriesAndClearsPreview()
        {
            TestDataTableAsset target = CreateAsset();

            SetEntries(
                target,
                new List<TestEntry>
                {
                    new TestEntry("item_001", 10)
                });

            IDataImportSession session = CreateSession(target);

            Result<DataImportSessionPreview> previewResult =
                session.CreatePreview(
                    "id,value\nitem_001,20\nitem_002,30",
                    DataImportFormat.Csv);

            Assert.That(previewResult.IsSuccess, Is.True);
            Assert.That(previewResult.Value.CanApply, Is.True);

            Result applyResult = session.ApplyPreview();

            Assert.That(applyResult.IsSuccess, Is.True);

            Assert.That(target.Count, Is.EqualTo(2));

            Assert.That(
                target.Entries[0].Id,
                Is.EqualTo("item_001"));

            Assert.That(
                target.Entries[0].Value,
                Is.EqualTo(20));

            Assert.That(
                target.Entries[1].Id,
                Is.EqualTo("item_002"));

            Assert.That(
                target.Entries[1].Value,
                Is.EqualTo(30));

            Assert.That(session.HasPreview, Is.False);
            Assert.That(session.CurrentPreview, Is.Null);
        }

        [Test]
        public void ApplyPreview_WithoutPreview_ReturnsImportFailed()
        {
            TestDataTableAsset target = CreateAsset();
            IDataImportSession session = CreateSession(target);

            Result result = session.ApplyPreview();

            Assert.That(result.IsFailure, Is.True);

            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));

            Assert.That(session.HasPreview, Is.False);
            Assert.That(session.CurrentPreview, Is.Null);
        }

        [Test]
        public void ApplyPreview_InvalidPreview_ReturnsValidationFailedAndClearsPreview()
        {
            TestDataTableAsset target = CreateAsset();
            IDataImportSession session = CreateSession(target);

            Result<DataImportSessionPreview> previewResult =
                session.CreatePreview(
                    "id,value\nitem_001,10\nitem_001,20",
                    DataImportFormat.Csv);

            Assert.That(previewResult.IsSuccess, Is.True);
            Assert.That(previewResult.Value.IsValid, Is.False);
            Assert.That(session.HasPreview, Is.True);

            Result applyResult = session.ApplyPreview();

            Assert.That(applyResult.IsFailure, Is.True);

            Assert.That(
                applyResult.Error.Code,
                Is.EqualTo(DataErrorCodes.ValidationFailed));

            Assert.That(target.Count, Is.EqualTo(0));
            Assert.That(session.HasPreview, Is.False);
            Assert.That(session.CurrentPreview, Is.Null);
        }

        [Test]
        public void ApplyPreview_TargetChangedAfterPreview_ReturnsImportFailedAndClearsPreview()
        {
            TestDataTableAsset target = CreateAsset();

            SetEntries(
                target,
                new List<TestEntry>
                {
                    new TestEntry("item_001", 10)
                });

            IDataImportSession session = CreateSession(target);

            Result<DataImportSessionPreview> previewResult =
                session.CreatePreview(
                    "id,value\nitem_001,20",
                    DataImportFormat.Csv);

            Assert.That(previewResult.IsSuccess, Is.True);
            Assert.That(previewResult.Value.CanApply, Is.True);

            SetEntries(
                target,
                new List<TestEntry>
                {
                    new TestEntry("item_001", 15)
                });

            Result applyResult = session.ApplyPreview();

            Assert.That(applyResult.IsFailure, Is.True);

            Assert.That(
                applyResult.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));

            Assert.That(target.Count, Is.EqualTo(1));
            Assert.That(target.Entries[0].Value, Is.EqualTo(15));

            Assert.That(session.HasPreview, Is.False);
            Assert.That(session.CurrentPreview, Is.Null);
        }

        [Test]
        public void ApplyPreview_ReorderedEquivalentEntries_AppliesCandidateOrder()
        {
            TestDataTableAsset target = CreateAsset();

            SetEntries(
                target,
                new List<TestEntry>
                {
                    new TestEntry("item_a", 10),
                    new TestEntry("item_b", 20)
                });

            IDataImportSession session = CreateSession(target);

            Result<DataImportSessionPreview> previewResult =
                session.CreatePreview(
                    "id,value\nitem_b,20\nitem_a,10",
                    DataImportFormat.Csv);

            Assert.That(previewResult.IsSuccess, Is.True);
            Assert.That(previewResult.Value.IsValid, Is.True);
            Assert.That(previewResult.Value.HasChanges, Is.False);
            Assert.That(previewResult.Value.CanApply, Is.True);
            Assert.That(previewResult.Value.UnchangedCount, Is.EqualTo(2));

            Result applyResult = session.ApplyPreview();

            Assert.That(applyResult.IsSuccess, Is.True);
            Assert.That(target.Count, Is.EqualTo(2));

            Assert.That(
                target.Entries[0].Id,
                Is.EqualTo("item_b"));

            Assert.That(
                target.Entries[0].Value,
                Is.EqualTo(20));

            Assert.That(
                target.Entries[1].Id,
                Is.EqualTo("item_a"));

            Assert.That(
                target.Entries[1].Value,
                Is.EqualTo(10));
        }

        [Test]
        public void ApplyPreview_EmptyCandidate_ClearsTargetEntries()
        {
            TestDataTableAsset target = CreateAsset();

            SetEntries(
                target,
                new List<TestEntry>
                {
                    new TestEntry("item_a", 10),
                    new TestEntry("item_b", 20)
                });

            IDataImportSession session = CreateSession(target);

            Result<DataImportSessionPreview> previewResult =
                session.CreatePreview(
                    "id,value",
                    DataImportFormat.Csv);

            Assert.That(previewResult.IsSuccess, Is.True);
            Assert.That(previewResult.Value.IsValid, Is.True);
            Assert.That(previewResult.Value.CandidateCount, Is.EqualTo(0));
            Assert.That(previewResult.Value.RemovedCount, Is.EqualTo(2));
            Assert.That(previewResult.Value.CanApply, Is.True);

            Result applyResult = session.ApplyPreview();

            Assert.That(applyResult.IsSuccess, Is.True);
            Assert.That(target.Count, Is.EqualTo(0));
            Assert.That(target.Entries, Is.Empty);
        }

        [Test]
        public void ApplyPreview_ValidPreview_UndoRedoRestoresBothStates()
        {
            TestDataTableAsset target = CreateAsset();

            SetEntries(
                target,
                new List<TestEntry>
                {
                    new TestEntry("item_original", 10)
                });

            IDataImportSession session = CreateSession(target);

            Undo.ClearUndo(target);

            Result<DataImportSessionPreview> previewResult =
                session.CreatePreview(
                    "id,value\nitem_new,20",
                    DataImportFormat.Csv);

            Assert.That(previewResult.IsSuccess, Is.True);

            Result applyResult = session.ApplyPreview();

            Assert.That(applyResult.IsSuccess, Is.True);
            Assert.That(target.Count, Is.EqualTo(1));
            Assert.That(target.Entries[0].Id, Is.EqualTo("item_new"));
            Assert.That(target.Entries[0].Value, Is.EqualTo(20));

            Undo.PerformUndo();

            Assert.That(target.Count, Is.EqualTo(1));
            Assert.That(target.Entries[0].Id, Is.EqualTo("item_original"));
            Assert.That(target.Entries[0].Value, Is.EqualTo(10));

            Undo.PerformRedo();

            Assert.That(target.Count, Is.EqualTo(1));
            Assert.That(target.Entries[0].Id, Is.EqualTo("item_new"));
            Assert.That(target.Entries[0].Value, Is.EqualTo(20));
        }

        private TestDataTableAsset CreateAsset()
        {
            TestDataTableAsset asset =
                ScriptableObject.CreateInstance<TestDataTableAsset>();

            createdAssets.Add(asset);
            return asset;
        }

        private static IDataImportSession CreateSession(TestDataTableAsset target)
        {
            Result<IDataImportSession> result =
                DataImportSessionFactory.Create(target);

            Assert.That(result.IsSuccess, Is.True);

            return result.Value;
        }

        private static void SetEntries(TestDataTableAsset asset, List<TestEntry> entries)
        {
            FieldInfo field =
                typeof(DataTableAsset<TestEntry>).GetField(
                    "entries",
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

            Assert.That(field, Is.Not.Null);

            field.SetValue(
                asset,
                entries);
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
        }

        private sealed class TestDataTableAsset : DataTableAsset<TestEntry>
        {
        }
    }
}