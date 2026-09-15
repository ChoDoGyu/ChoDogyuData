using System;
using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Editor.Importing;
using NUnit.Framework;
using UnityEngine;

namespace CDG.Data.Tests.Editor
{
    public sealed class UnityCsvRowMapperTests
    {
        [Test]
        public void Map_SupportedScalarFields_ReturnsMappedEntry()
        {
            CsvHeader header = CreateHeader(
                "id",
                "value",
                "enabled",
                "ratio",
                "kind");

            CsvRow row = new CsvRow(
                new[]
                {
                    "item_001",
                    "25",
                    "true",
                    "1.5",
                    "Rare"
                },
                2);

            UnityCsvRowMapper<TestEntry> mapper =
                new UnityCsvRowMapper<TestEntry>();

            Result<TestEntry> result = mapper.Map(
                header,
                row);

            Assert.That(result.IsSuccess, Is.True);

            Assert.That(result.Value.Id, Is.EqualTo("item_001"));
            Assert.That(result.Value.Value, Is.EqualTo(25));
            Assert.That(result.Value.Enabled, Is.True);
            Assert.That(result.Value.Ratio, Is.EqualTo(1.5f));
            Assert.That(result.Value.Kind, Is.EqualTo(TestKind.Rare));
        }

        [Test]
        public void Map_InvalidInteger_ReturnsImportFailed()
        {
            CsvHeader header = CreateHeader(
                "id",
                "value",
                "enabled",
                "ratio",
                "kind");

            CsvRow row = new CsvRow(
                new[]
                {
                    "item_001",
                    "invalid",
                    "true",
                    "1.5",
                    "Rare"
                },
                2);

            UnityCsvRowMapper<TestEntry> mapper =
                new UnityCsvRowMapper<TestEntry>();

            Result<TestEntry> result = mapper.Map(
                header,
                row);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void ValidateHeader_UnknownColumn_ReturnsImportFailed()
        {
            CsvHeader header = CreateHeader(
                "id",
                "Value",
                "enabled",
                "ratio",
                "kind");

            UnityCsvRowMapper<TestEntry> mapper =
                new UnityCsvRowMapper<TestEntry>();

            Result result = mapper.ValidateHeader(header);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void ValidateHeader_MissingColumn_ReturnsImportFailed()
        {
            CsvHeader header = CreateHeader(
                "id",
                "value",
                "enabled",
                "ratio");

            UnityCsvRowMapper<TestEntry> mapper =
                new UnityCsvRowMapper<TestEntry>();

            Result result = mapper.ValidateHeader(header);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void ValidateHeader_ComplexField_ReturnsImportFailed()
        {
            CsvHeader header = CreateHeader(
                "id",
                "stats");

            UnityCsvRowMapper<ComplexEntry> mapper =
                new UnityCsvRowMapper<ComplexEntry>();

            Result result = mapper.ValidateHeader(header);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Import_HeaderOnlyWithUnsupportedComplexField_ReturnsImportFailed()
        {
            CsvDataImporter<ComplexEntry> importer =
                new CsvDataImporter<ComplexEntry>(
                    new UnityCsvRowMapper<ComplexEntry>());

            Result<DataImportCandidate<ComplexEntry>> result =
                importer.Import("id,stats");

            Assert.That(result.IsFailure, Is.True);
            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Map_NullHeader_ThrowsArgumentNullException()
        {
            UnityCsvRowMapper<TestEntry> mapper =
                new UnityCsvRowMapper<TestEntry>();

            CsvRow row = new CsvRow(
                new[]
                {
                    "item_001",
                    "25",
                    "true",
                    "1.5",
                    "Rare"
                },
                2);

            Assert.Throws<ArgumentNullException>(() =>
                mapper.Map(null, row));
        }

        [Test]
        public void Map_NullRow_ThrowsArgumentNullException()
        {
            UnityCsvRowMapper<TestEntry> mapper =
                new UnityCsvRowMapper<TestEntry>();

            CsvHeader header = CreateHeader(
                "id",
                "value",
                "enabled",
                "ratio",
                "kind");

            Assert.Throws<ArgumentNullException>(() =>
                mapper.Map(header, null));
        }

        private static CsvHeader CreateHeader(params string[] columns)
        {
            Result<CsvHeader> result = CsvHeader.Create(
                new CsvRow(columns, 1));

            Assert.That(result.IsSuccess, Is.True);

            return result.Value;
        }

        private enum TestKind
        {
            Normal = 0,
            Rare = 1
        }

        [Serializable]
        private sealed class TestEntry : IDataEntry
        {
            [SerializeField]
            private string id;

            [SerializeField]
            private int value;

            [SerializeField]
            private bool enabled;

            [SerializeField]
            private float ratio;

            [SerializeField]
            private TestKind kind;

            public string Id => id;
            public int Value => value;
            public bool Enabled => enabled;
            public float Ratio => ratio;
            public TestKind Kind => kind;
        }

        [Serializable]
        private sealed class ComplexEntry : IDataEntry
        {
            [SerializeField]
            private string id;

            [SerializeField]
            private Stats stats;

            public string Id => id;
        }

        [Serializable]
        private struct Stats
        {
            public int attack;
        }
    }
}