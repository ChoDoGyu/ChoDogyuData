using System;
using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Editor.Importing;
using CDG.Data.Validation;
using NUnit.Framework;

namespace CDG.Data.Tests.Editor
{
    public sealed class CsvImportPipelineTests
    {
        [Test]
        public void Import_ValidCsvThroughProcessor_ReturnsValidPreview()
        {
            StubRowMapper mapper = CreateDefaultMapper();
            CsvDataImporter<TestEntry> importer = new CsvDataImporter<TestEntry>(mapper);

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import(
                "id,name\nitem_001,Sword\nitem_002,Shield",
                importer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.IsValid, Is.True);
            Assert.That(result.Value.ValidationReport.Count, Is.EqualTo(0));
            Assert.That(result.Value.Candidate.Count, Is.EqualTo(2));

            Assert.That(result.Value.Candidate.Entries[0].Id, Is.EqualTo("item_001"));
            Assert.That(result.Value.Candidate.Entries[0].Name, Is.EqualTo("Sword"));

            Assert.That(result.Value.Candidate.Entries[1].Id, Is.EqualTo("item_002"));
            Assert.That(result.Value.Candidate.Entries[1].Name, Is.EqualTo("Shield"));

            Assert.That(mapper.CallCount, Is.EqualTo(2));
        }

        [Test]
        public void Import_DuplicateIds_ReturnsSuccessfulInvalidPreview()
        {
            StubRowMapper mapper = CreateDefaultMapper();
            CsvDataImporter<TestEntry> importer = new CsvDataImporter<TestEntry>(mapper);

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import(
                "id,name\nitem_001,Sword\nitem_001,Shield",
                importer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.IsValid, Is.False);
            Assert.That(result.Value.ValidationReport.Count, Is.EqualTo(1));
            Assert.That(result.Value.ValidationReport.Issues[0].Type, Is.EqualTo(DataValidationIssueType.DuplicateId));
            Assert.That(result.Value.ValidationReport.Issues[0].EntryIndex, Is.EqualTo(1));
            Assert.That(result.Value.ValidationReport.Issues[0].EntryId, Is.EqualTo("item_001"));
        }

        [Test]
        public void Import_NullMappedEntry_ReturnsSuccessfulInvalidPreview()
        {
            StubRowMapper mapper = new StubRowMapper((header, row) =>
                Result<TestEntry>.Success(null));

            CsvDataImporter<TestEntry> importer = new CsvDataImporter<TestEntry>(mapper);

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import(
                "id,name\nitem_001,Sword",
                importer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.IsValid, Is.False);
            Assert.That(result.Value.ValidationReport.Count, Is.EqualTo(1));
            Assert.That(result.Value.ValidationReport.Issues[0].Type, Is.EqualTo(DataValidationIssueType.NullEntry));
            Assert.That(result.Value.ValidationReport.Issues[0].EntryIndex, Is.EqualTo(0));
        }

        [Test]
        public void Import_InvalidHeader_ReturnsImportFailedWithoutCallingMapper()
        {
            StubRowMapper mapper = CreateDefaultMapper();
            CsvDataImporter<TestEntry> importer = new CsvDataImporter<TestEntry>(mapper);

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import(
                "id,id\nitem_001,Sword",
                importer);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
            Assert.That(mapper.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void Import_MapperFailure_ReturnsImportFailed()
        {
            StubRowMapper mapper = new StubRowMapper((header, row) =>
                Result<TestEntry>.Failure(new ResultError(
                    DataErrorCodes.ImportFailed,
                    "테스트 데이터 변환 실패")));

            CsvDataImporter<TestEntry> importer = new CsvDataImporter<TestEntry>(mapper);

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import(
                "id,name\nitem_001,Sword",
                importer);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
            Assert.That(result.Error.Message, Does.Contain("2행"));
            Assert.That(result.Error.Message, Does.Contain("테스트 데이터 변환 실패"));
            Assert.That(mapper.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void Import_InvalidCsv_ReturnsImportFailedWithoutCallingMapper()
        {
            StubRowMapper mapper = CreateDefaultMapper();
            CsvDataImporter<TestEntry> importer = new CsvDataImporter<TestEntry>(mapper);

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import(
                "id,name\nitem_001,\"Sword",
                importer);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
            Assert.That(mapper.CallCount, Is.EqualTo(0));
        }

        private static StubRowMapper CreateDefaultMapper()
        {
            return new StubRowMapper((header, row) =>
            {
                Assert.That(header.TryGetIndex("id", out int idIndex), Is.True);
                Assert.That(header.TryGetIndex("name", out int nameIndex), Is.True);

                return Result<TestEntry>.Success(new TestEntry(
                    row[idIndex],
                    row[nameIndex]));
            });
        }

        private sealed class StubRowMapper : ICsvRowMapper<TestEntry>
        {
            private readonly Func<CsvHeader, CsvRow, Result<TestEntry>> map;

            public int CallCount { get; private set; }

            public StubRowMapper(Func<CsvHeader, CsvRow, Result<TestEntry>> map)
            {
                this.map = map;
            }

            public Result<TestEntry> Map(CsvHeader header, CsvRow row)
            {
                CallCount++;
                return map(header, row);
            }
        }

        private sealed class TestEntry : IDataEntry
        {
            public string Id { get; }
            public string Name { get; }

            public TestEntry(string id, string name)
            {
                Id = id;
                Name = name;
            }
        }
    }
}