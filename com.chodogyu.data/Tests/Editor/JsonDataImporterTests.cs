using System;
using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Editor.Importing;
using NUnit.Framework;

namespace CDG.Data.Tests.Editor
{
    public sealed class JsonDataImporterTests
    {
        [Test]
        public void Import_ValidJson_ReturnsCandidateInOriginalOrder()
        {
            StubObjectMapper mapper = new StubObjectMapper(source =>
            {
                if (source.Text.Contains("item_001"))
                {
                    return Result<TestEntry>.Success(
                        new TestEntry("item_001", "Sword"));
                }

                return Result<TestEntry>.Success(
                    new TestEntry("item_002", "Shield"));
            });

            JsonDataImporter<TestEntry> importer =
                new JsonDataImporter<TestEntry>(mapper);

            Result<DataImportCandidate<TestEntry>> result = importer.Import(
                "[" +
                "{\"id\":\"item_001\",\"name\":\"Sword\"}," +
                "{\"id\":\"item_002\",\"name\":\"Shield\"}" +
                "]");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(2));

            Assert.That(result.Value.Entries[0].Id, Is.EqualTo("item_001"));
            Assert.That(result.Value.Entries[0].Name, Is.EqualTo("Sword"));

            Assert.That(result.Value.Entries[1].Id, Is.EqualTo("item_002"));
            Assert.That(result.Value.Entries[1].Name, Is.EqualTo("Shield"));

            Assert.That(mapper.CallCount, Is.EqualTo(2));
        }

        [Test]
        public void Import_EmptyArray_ReturnsEmptyCandidateWithoutCallingMapper()
        {
            StubObjectMapper mapper = CreateSuccessfulMapper();

            JsonDataImporter<TestEntry> importer =
                new JsonDataImporter<TestEntry>(mapper);

            Result<DataImportCandidate<TestEntry>> result =
                importer.Import("[]");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(0));
            Assert.That(mapper.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void Import_InvalidJson_ReturnsImportFailedWithoutCallingMapper()
        {
            StubObjectMapper mapper = CreateSuccessfulMapper();

            JsonDataImporter<TestEntry> importer =
                new JsonDataImporter<TestEntry>(mapper);

            Result<DataImportCandidate<TestEntry>> result =
                importer.Import("{\"id\":\"item_001\"}");

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
            Assert.That(mapper.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void Import_PassesOriginalObjectTextToMapper()
        {
            const string objectText =
                "{\"id\":\"item_001\",\"stats\":{\"attack\":10}}";

            string receivedText = null;

            StubObjectMapper mapper = new StubObjectMapper(source =>
            {
                receivedText = source.Text;

                return Result<TestEntry>.Success(
                    new TestEntry("item_001", "Sword"));
            });

            JsonDataImporter<TestEntry> importer =
                new JsonDataImporter<TestEntry>(mapper);

            Result<DataImportCandidate<TestEntry>> result =
                importer.Import("[" + objectText + "]");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(receivedText, Is.EqualTo(objectText));
        }

        [Test]
        public void Import_MapperFailure_ReturnsImportFailedAndStops()
        {
            StubObjectMapper mapper = new StubObjectMapper(source =>
                Result<TestEntry>.Failure(new ResultError(
                    DataErrorCodes.ImportFailed,
                    "테스트 JSON 변환 실패")));

            JsonDataImporter<TestEntry> importer =
                new JsonDataImporter<TestEntry>(mapper);

            Result<DataImportCandidate<TestEntry>> result = importer.Import(
                "[" +
                "{\"id\":\"item_001\"}," +
                "{\"id\":\"item_002\"}" +
                "]");

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
            Assert.That(result.Error.Message, Does.Contain("1행"));
            Assert.That(result.Error.Message, Does.Contain("테스트 JSON 변환 실패"));
            Assert.That(mapper.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void Import_MultiLineMapperFailure_ContainsObjectStartLine()
        {
            StubObjectMapper mapper = new StubObjectMapper(source =>
                Result<TestEntry>.Failure(new ResultError(
                    DataErrorCodes.ImportFailed,
                    "테스트 JSON 변환 실패")));

            JsonDataImporter<TestEntry> importer =
                new JsonDataImporter<TestEntry>(mapper);

            Result<DataImportCandidate<TestEntry>> result = importer.Import(
                "[\n" +
                "  {\n" +
                "    \"id\":\"item_001\"\n" +
                "  }\n" +
                "]");

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Message, Does.Contain("2행"));
        }

        [Test]
        public void Import_MapperSuccessWithNullEntry_PreservesNullForLaterValidation()
        {
            StubObjectMapper mapper = new StubObjectMapper(source =>
                Result<TestEntry>.Success(null));

            JsonDataImporter<TestEntry> importer =
                new JsonDataImporter<TestEntry>(mapper);

            Result<DataImportCandidate<TestEntry>> result =
                importer.Import("[{\"id\":\"item_001\"}]");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(1));
            Assert.That(result.Value.Entries[0], Is.Null);
        }

        [Test]
        public void Import_MapperReturnsNullResult_ReturnsImportFailed()
        {
            StubObjectMapper mapper =
                new StubObjectMapper(source => null);

            JsonDataImporter<TestEntry> importer =
                new JsonDataImporter<TestEntry>(mapper);

            Result<DataImportCandidate<TestEntry>> result =
                importer.Import("[{\"id\":\"item_001\"}]");

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
            Assert.That(result.Error.Message, Does.Contain("1행"));
            Assert.That(mapper.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void Constructor_NullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new JsonDataImporter<TestEntry>(null));
        }

        private static StubObjectMapper CreateSuccessfulMapper()
        {
            return new StubObjectMapper(source =>
                Result<TestEntry>.Success(
                    new TestEntry("item_001", "Sword")));
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