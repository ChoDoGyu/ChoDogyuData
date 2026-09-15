using System;
using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Editor.Importing;
using CDG.Data.Validation;
using NUnit.Framework;

namespace CDG.Data.Tests.Editor
{
    public sealed class JsonImportPipelineTests
    {
        [Test]
        public void Import_ValidJsonThroughProcessor_ReturnsValidPreview()
        {
            StubObjectMapper mapper = CreateDefaultMapper();
            JsonDataImporter<TestEntry> importer = new JsonDataImporter<TestEntry>(mapper);

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import(
                "[" +
                "{\"id\":\"item_001\",\"name\":\"Sword\"}," +
                "{\"id\":\"item_002\",\"name\":\"Shield\"}" +
                "]",
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
            StubObjectMapper mapper = new StubObjectMapper(source =>
            {
                if (source.Text.Contains("Sword"))
                {
                    return Result<TestEntry>.Success(new TestEntry("item_001", "Sword"));
                }

                return Result<TestEntry>.Success(new TestEntry("item_001", "Shield"));
            });

            JsonDataImporter<TestEntry> importer = new JsonDataImporter<TestEntry>(mapper);

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import(
                "[" +
                "{\"id\":\"item_001\",\"name\":\"Sword\"}," +
                "{\"id\":\"item_001\",\"name\":\"Shield\"}" +
                "]",
                importer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.IsValid, Is.False);
            Assert.That(result.Value.ValidationReport.Count, Is.EqualTo(1));

            Assert.That(
                result.Value.ValidationReport.Issues[0].Type,
                Is.EqualTo(DataValidationIssueType.DuplicateId));

            Assert.That(
                result.Value.ValidationReport.Issues[0].EntryIndex,
                Is.EqualTo(1));

            Assert.That(
                result.Value.ValidationReport.Issues[0].EntryId,
                Is.EqualTo("item_001"));
        }

        [Test]
        public void Import_InvalidId_ReturnsSuccessfulInvalidPreview()
        {
            StubObjectMapper mapper = new StubObjectMapper(source =>
                Result<TestEntry>.Success(
                    new TestEntry(" item_001", "Sword")));

            JsonDataImporter<TestEntry> importer = new JsonDataImporter<TestEntry>(mapper);

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import(
                "[{\"id\":\" item_001\",\"name\":\"Sword\"}]",
                importer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.IsValid, Is.False);
            Assert.That(result.Value.ValidationReport.Count, Is.EqualTo(1));

            Assert.That(
                result.Value.ValidationReport.Issues[0].Type,
                Is.EqualTo(DataValidationIssueType.InvalidId));

            Assert.That(
                result.Value.ValidationReport.Issues[0].EntryIndex,
                Is.EqualTo(0));

            Assert.That(
                result.Value.ValidationReport.Issues[0].EntryId,
                Is.EqualTo(" item_001"));
        }

        [Test]
        public void Import_NullMappedEntry_ReturnsSuccessfulInvalidPreview()
        {
            StubObjectMapper mapper = new StubObjectMapper(source =>
                Result<TestEntry>.Success(null));

            JsonDataImporter<TestEntry> importer = new JsonDataImporter<TestEntry>(mapper);

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import(
                "[{\"id\":\"item_001\",\"name\":\"Sword\"}]",
                importer);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.IsValid, Is.False);
            Assert.That(result.Value.ValidationReport.Count, Is.EqualTo(1));

            Assert.That(
                result.Value.ValidationReport.Issues[0].Type,
                Is.EqualTo(DataValidationIssueType.NullEntry));

            Assert.That(
                result.Value.ValidationReport.Issues[0].EntryIndex,
                Is.EqualTo(0));
        }

        [Test]
        public void Import_InvalidJson_ReturnsImportFailedWithoutCallingMapper()
        {
            StubObjectMapper mapper = CreateDefaultMapper();
            JsonDataImporter<TestEntry> importer = new JsonDataImporter<TestEntry>(mapper);

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import(
                "{\"id\":\"item_001\"}",
                importer);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
            Assert.That(mapper.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void Import_MapperFailure_ReturnsImportFailed()
        {
            StubObjectMapper mapper = new StubObjectMapper(source =>
                Result<TestEntry>.Failure(new ResultError(
                    DataErrorCodes.ImportFailed,
                    "테스트 JSON 데이터 변환 실패")));

            JsonDataImporter<TestEntry> importer = new JsonDataImporter<TestEntry>(mapper);

            Result<DataImportPreview<TestEntry>> result = DataImportProcessor.Import(
                "[\n" +
                "  {\"id\":\"item_001\"}\n" +
                "]",
                importer);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
            Assert.That(result.Error.Message, Does.Contain("2행"));
            Assert.That(result.Error.Message, Does.Contain("테스트 JSON 데이터 변환 실패"));
            Assert.That(mapper.CallCount, Is.EqualTo(1));
        }

        private static StubObjectMapper CreateDefaultMapper()
        {
            return new StubObjectMapper(source =>
            {
                if (source.Text.Contains("item_001"))
                {
                    return Result<TestEntry>.Success(
                        new TestEntry("item_001", "Sword"));
                }

                return Result<TestEntry>.Success(
                    new TestEntry("item_002", "Shield"));
            });
        }

        private sealed class StubObjectMapper : IJsonObjectMapper<TestEntry>
        {
            private readonly Func<JsonObjectSource, Result<TestEntry>> map;

            public int CallCount { get; private set; }

            public StubObjectMapper(Func<JsonObjectSource, Result<TestEntry>> map)
            {
                this.map = map;
            }

            public Result<TestEntry> Map(JsonObjectSource source)
            {
                CallCount++;
                return map(source);
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