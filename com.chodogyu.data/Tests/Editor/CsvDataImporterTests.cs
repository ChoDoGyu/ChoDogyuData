using System;
using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Editor.Importing;
using NUnit.Framework;

namespace CDG.Data.Tests.Editor
{
    public sealed class CsvDataImporterTests
    {
        [Test]
        public void Import_ValidCsv_ReturnsCandidateInOriginalOrder()
        {
            StubRowMapper mapper = new StubRowMapper((header, row) =>
            {
                Assert.That(header.TryGetIndex("id", out int idIndex), Is.True);
                Assert.That(header.TryGetIndex("name", out int nameIndex), Is.True);

                return Result<TestEntry>.Success(new TestEntry(
                    row[idIndex],
                    row[nameIndex]));
            });

            CsvDataImporter<TestEntry> importer = new CsvDataImporter<TestEntry>(mapper);

            Result<DataImportCandidate<TestEntry>> result = importer.Import(
                "id,name\nitem_001,Sword\nitem_002,Shield");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(2));

            Assert.That(result.Value.Entries[0].Id, Is.EqualTo("item_001"));
            Assert.That(result.Value.Entries[0].Name, Is.EqualTo("Sword"));

            Assert.That(result.Value.Entries[1].Id, Is.EqualTo("item_002"));
            Assert.That(result.Value.Entries[1].Name, Is.EqualTo("Shield"));

            Assert.That(mapper.CallCount, Is.EqualTo(2));
        }

        [Test]
        public void Import_HeaderOnly_ReturnsEmptyCandidate()
        {
            StubRowMapper mapper = new StubRowMapper((header, row) =>
                Result<TestEntry>.Success(new TestEntry(row[0], row[1])));

            CsvDataImporter<TestEntry> importer = new CsvDataImporter<TestEntry>(mapper);

            Result<DataImportCandidate<TestEntry>> result = importer.Import("id,name");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(0));
            Assert.That(mapper.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void Import_InvalidCsv_ReturnsImportFailedWithoutCallingMapper()
        {
            StubRowMapper mapper = CreateSuccessfulMapper();
            CsvDataImporter<TestEntry> importer = new CsvDataImporter<TestEntry>(mapper);

            Result<DataImportCandidate<TestEntry>> result = importer.Import(
                "id,name\nitem_001,\"Sword");

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
            Assert.That(mapper.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void Import_InvalidHeader_ReturnsImportFailedWithoutCallingMapper()
        {
            StubRowMapper mapper = CreateSuccessfulMapper();
            CsvDataImporter<TestEntry> importer = new CsvDataImporter<TestEntry>(mapper);

            Result<DataImportCandidate<TestEntry>> result = importer.Import(
                "id,id\nitem_001,Sword");

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
            Assert.That(mapper.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void Import_RowColumnCountMismatch_ReturnsImportFailedWithoutCallingMapper()
        {
            StubRowMapper mapper = CreateSuccessfulMapper();
            CsvDataImporter<TestEntry> importer = new CsvDataImporter<TestEntry>(mapper);

            Result<DataImportCandidate<TestEntry>> result = importer.Import(
                "id,name,price\nitem_001,Sword");

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
            Assert.That(result.Error.Message, Does.Contain("2행"));
            Assert.That(mapper.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void Import_MapperFailure_ReturnsImportFailedAndStops()
        {
            StubRowMapper mapper = new StubRowMapper((header, row) =>
                Result<TestEntry>.Failure(new ResultError(
                    DataErrorCodes.ImportFailed,
                    "테스트 변환 실패")));

            CsvDataImporter<TestEntry> importer = new CsvDataImporter<TestEntry>(mapper);

            Result<DataImportCandidate<TestEntry>> result = importer.Import(
                "id,name\nitem_001,Sword\nitem_002,Shield");

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
            Assert.That(result.Error.Message, Does.Contain("2행"));
            Assert.That(result.Error.Message, Does.Contain("테스트 변환 실패"));
            Assert.That(mapper.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void Import_MapperSuccessWithNullEntry_PreservesNullForLaterValidation()
        {
            StubRowMapper mapper = new StubRowMapper((header, row) =>
                Result<TestEntry>.Success(null));

            CsvDataImporter<TestEntry> importer = new CsvDataImporter<TestEntry>(mapper);

            Result<DataImportCandidate<TestEntry>> result = importer.Import(
                "id,name\nitem_001,Sword");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(1));
            Assert.That(result.Value.Entries[0], Is.Null);
        }

        [Test]
        public void Import_MapperReturnsNullResult_ReturnsImportFailed()
        {
            StubRowMapper mapper = new StubRowMapper((header, row) => null);
            CsvDataImporter<TestEntry> importer = new CsvDataImporter<TestEntry>(mapper);

            Result<DataImportCandidate<TestEntry>> result = importer.Import(
                "id,name\nitem_001,Sword");

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
            Assert.That(result.Error.Message, Does.Contain("2행"));
            Assert.That(mapper.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void Constructor_NullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CsvDataImporter<TestEntry>(null));
        }

        private static StubRowMapper CreateSuccessfulMapper()
        {
            return new StubRowMapper((header, row) =>
            {
                header.TryGetIndex("id", out int idIndex);
                header.TryGetIndex("name", out int nameIndex);

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