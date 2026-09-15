using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CDG.Data.Tests.Editor
{
    public sealed class DataImportEditorEndToEndTests
    {
        private readonly List<TestDataTableAsset> createdAssets = new List<TestDataTableAsset>();
        private string temporaryDirectory;

        [SetUp]
        public void SetUp()
        {
            temporaryDirectory = Path.Combine(
                Path.GetTempPath(),
                "CDG.Data.EndToEndTests",
                Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(temporaryDirectory);
        }

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

            if (!string.IsNullOrEmpty(temporaryDirectory) &&
                Directory.Exists(temporaryDirectory))
            {
                Directory.Delete(
                    temporaryDirectory,
                    true);
            }
        }

        [Test]
        public void CsvFile_ReadPreviewApplyUndoRedo_CompletesFullWorkflow()
        {
            TestDataTableAsset target = CreateAsset();

            SetEntries(target, new List<TestEntry>
            {
                new TestEntry("item_a", 10),
                new TestEntry("item_removed", 99)
            });

            string path = CreateFile(
                "data.csv",
                "id,value\nitem_a,20\nitem_b,30");

            Result<string> readResult = DataImportSourceFileReader.Read(
                path,
                DataImportFormat.Csv);

            Assert.That(readResult.IsSuccess, Is.True);

            IDataImportSession session = CreateSession(target);

            Result<DataImportSessionPreview> previewResult = session.CreatePreview(
                readResult.Value,
                DataImportFormat.Csv);

            Assert.That(previewResult.IsSuccess, Is.True);
            Assert.That(previewResult.Value.CanApply, Is.True);

            Assert.That(previewResult.Value.CandidateCount, Is.EqualTo(2));
            Assert.That(previewResult.Value.AddedCount, Is.EqualTo(1));
            Assert.That(previewResult.Value.RemovedCount, Is.EqualTo(1));
            Assert.That(previewResult.Value.ModifiedCount, Is.EqualTo(1));
            Assert.That(previewResult.Value.UnchangedCount, Is.EqualTo(0));

            Undo.ClearUndo(target);

            Result applyResult = session.ApplyPreview();

            Assert.That(applyResult.IsSuccess, Is.True);
            Assert.That(target.Count, Is.EqualTo(2));

            Assert.That(target.Entries[0].Id, Is.EqualTo("item_a"));
            Assert.That(target.Entries[0].Value, Is.EqualTo(20));

            Assert.That(target.Entries[1].Id, Is.EqualTo("item_b"));
            Assert.That(target.Entries[1].Value, Is.EqualTo(30));

            Assert.That(session.HasPreview, Is.False);

            Undo.PerformUndo();

            Assert.That(target.Count, Is.EqualTo(2));
            Assert.That(target.Entries[0].Id, Is.EqualTo("item_a"));
            Assert.That(target.Entries[0].Value, Is.EqualTo(10));
            Assert.That(target.Entries[1].Id, Is.EqualTo("item_removed"));
            Assert.That(target.Entries[1].Value, Is.EqualTo(99));

            Undo.PerformRedo();

            Assert.That(target.Count, Is.EqualTo(2));
            Assert.That(target.Entries[0].Id, Is.EqualTo("item_a"));
            Assert.That(target.Entries[0].Value, Is.EqualTo(20));
            Assert.That(target.Entries[1].Id, Is.EqualTo("item_b"));
            Assert.That(target.Entries[1].Value, Is.EqualTo(30));
        }

        [Test]
        public void JsonFile_ReadPreviewApply_CompletesFullWorkflow()
        {
            TestDataTableAsset target = CreateAsset();

            SetEntries(target, new List<TestEntry>
            {
                new TestEntry("item_001", 10)
            });

            string path = CreateFile(
                "data.json",
                "[{\"id\":\"item_001\",\"value\":25},{\"id\":\"item_002\",\"value\":40}]");

            Result<string> readResult = DataImportSourceFileReader.Read(
                path,
                DataImportFormat.Json);

            Assert.That(readResult.IsSuccess, Is.True);

            IDataImportSession session = CreateSession(target);

            Result<DataImportSessionPreview> previewResult = session.CreatePreview(
                readResult.Value,
                DataImportFormat.Json);

            Assert.That(previewResult.IsSuccess, Is.True);
            Assert.That(previewResult.Value.IsValid, Is.True);
            Assert.That(previewResult.Value.CanApply, Is.True);

            Assert.That(previewResult.Value.AddedCount, Is.EqualTo(1));
            Assert.That(previewResult.Value.ModifiedCount, Is.EqualTo(1));

            Result applyResult = session.ApplyPreview();

            Assert.That(applyResult.IsSuccess, Is.True);

            Assert.That(target.Count, Is.EqualTo(2));

            Assert.That(target.Entries[0].Id, Is.EqualTo("item_001"));
            Assert.That(target.Entries[0].Value, Is.EqualTo(25));

            Assert.That(target.Entries[1].Id, Is.EqualTo("item_002"));
            Assert.That(target.Entries[1].Value, Is.EqualTo(40));
        }

        [Test]
        public void CsvFile_DuplicateIds_ReturnsInvalidPreviewAndPreservesTarget()
        {
            TestDataTableAsset target = CreateAsset();
            TestEntry original = new TestEntry("item_original", 10);

            SetEntries(target, new List<TestEntry>
            {
                original
            });

            string path = CreateFile(
                "duplicate.csv",
                "id,value\nitem_001,10\nitem_001,20");

            Result<string> readResult = DataImportSourceFileReader.Read(
                path,
                DataImportFormat.Csv);

            Assert.That(readResult.IsSuccess, Is.True);

            IDataImportSession session = CreateSession(target);

            Result<DataImportSessionPreview> previewResult = session.CreatePreview(
                readResult.Value,
                DataImportFormat.Csv);

            Assert.That(previewResult.IsSuccess, Is.True);
            Assert.That(previewResult.Value.IsValid, Is.False);
            Assert.That(previewResult.Value.CanApply, Is.False);
            Assert.That(previewResult.Value.ValidationReport.Count, Is.EqualTo(1));

            Result applyResult = session.ApplyPreview();

            Assert.That(applyResult.IsFailure, Is.True);
            Assert.That(applyResult.Error.Code, Is.EqualTo(DataErrorCodes.ValidationFailed));

            Assert.That(target.Count, Is.EqualTo(1));
            Assert.That(target.Entries[0], Is.SameAs(original));

            Assert.That(session.HasPreview, Is.False);
        }

        [Test]
        public void SourceFile_WrongExtension_ReturnsImportFailedWithoutCreatingPreview()
        {
            TestDataTableAsset target = CreateAsset();

            SetEntries(target, new List<TestEntry>
            {
                new TestEntry("item_original", 10)
            });

            string path = CreateFile(
                "data.json",
                "[]");

            Result<string> readResult = DataImportSourceFileReader.Read(
                path,
                DataImportFormat.Csv);

            Assert.That(readResult.IsFailure, Is.True);
            Assert.That(readResult.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));

            IDataImportSession session = CreateSession(target);

            Assert.That(session.HasPreview, Is.False);
            Assert.That(session.CurrentPreview, Is.Null);

            Assert.That(target.Count, Is.EqualTo(1));
            Assert.That(target.Entries[0].Id, Is.EqualTo("item_original"));
            Assert.That(target.Entries[0].Value, Is.EqualTo(10));
        }

        [Test]
        public void ApplyPreview_TargetChangedAfterFilePreview_ReturnsImportFailedAndPreservesLatestTarget()
        {
            TestDataTableAsset target = CreateAsset();

            SetEntries(target, new List<TestEntry>
            {
                new TestEntry("item_001", 10)
            });

            string path = CreateFile(
                "data.csv",
                "id,value\nitem_001,20");

            Result<string> readResult = DataImportSourceFileReader.Read(
                path,
                DataImportFormat.Csv);

            Assert.That(readResult.IsSuccess, Is.True);

            IDataImportSession session = CreateSession(target);

            Result<DataImportSessionPreview> previewResult = session.CreatePreview(
                readResult.Value,
                DataImportFormat.Csv);

            Assert.That(previewResult.IsSuccess, Is.True);
            Assert.That(previewResult.Value.CanApply, Is.True);

            SetEntries(target, new List<TestEntry>
            {
                new TestEntry("item_001", 15)
            });

            Result applyResult = session.ApplyPreview();

            Assert.That(applyResult.IsFailure, Is.True);
            Assert.That(applyResult.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));

            Assert.That(target.Count, Is.EqualTo(1));
            Assert.That(target.Entries[0].Id, Is.EqualTo("item_001"));
            Assert.That(target.Entries[0].Value, Is.EqualTo(15));

            Assert.That(session.HasPreview, Is.False);
            Assert.That(session.CurrentPreview, Is.Null);
        }

        [Test]
        public void CsvFile_HeaderOnly_AppliesEmptySourceOfTruth()
        {
            TestDataTableAsset target = CreateAsset();

            SetEntries(target, new List<TestEntry>
            {
                new TestEntry("item_a", 10),
                new TestEntry("item_b", 20)
            });

            string path = CreateFile(
                "empty.csv",
                "id,value");

            Result<string> readResult = DataImportSourceFileReader.Read(
                path,
                DataImportFormat.Csv);

            Assert.That(readResult.IsSuccess, Is.True);

            IDataImportSession session = CreateSession(target);

            Result<DataImportSessionPreview> previewResult = session.CreatePreview(
                readResult.Value,
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

        private TestDataTableAsset CreateAsset()
        {
            TestDataTableAsset asset = ScriptableObject.CreateInstance<TestDataTableAsset>();

            createdAssets.Add(asset);

            return asset;
        }

        private static IDataImportSession CreateSession(TestDataTableAsset target)
        {
            Result<IDataImportSession> result = DataImportSessionFactory.Create(target);

            Assert.That(result.IsSuccess, Is.True);

            return result.Value;
        }

        private string CreateFile(string fileName, string text)
        {
            string path = Path.Combine(
                temporaryDirectory,
                fileName);

            File.WriteAllText(
                path,
                text);

            return path;
        }

        private static void SetEntries(TestDataTableAsset asset, List<TestEntry> entries)
        {
            FieldInfo field = typeof(DataTableAsset<TestEntry>).GetField(
                "entries",
                BindingFlags.Instance | BindingFlags.NonPublic);

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