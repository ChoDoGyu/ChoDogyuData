using System;
using System.IO;
using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Editor;
using NUnit.Framework;

namespace CDG.Data.Tests.Editor
{
    public sealed class DataImportSourceFileReaderTests
    {
        private string temporaryDirectory;

        [SetUp]
        public void SetUp()
        {
            temporaryDirectory = Path.Combine(
                Path.GetTempPath(),
                "CDG.Data.Tests",
                Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(temporaryDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            if (!string.IsNullOrEmpty(temporaryDirectory) &&
                Directory.Exists(temporaryDirectory))
            {
                Directory.Delete(
                    temporaryDirectory,
                    true);
            }
        }

        [Test]
        public void Read_ValidCsv_ReturnsOriginalText()
        {
            string text =
                "id,value\nitem_001,10";

            string path = CreateFile(
                "data.csv",
                text);

            Result<string> result =
                DataImportSourceFileReader.Read(
                    path,
                    DataImportFormat.Csv);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(text));
        }

        [Test]
        public void Read_ValidJsonWithUppercaseExtension_ReturnsOriginalText()
        {
            string text =
                "[{\"id\":\"item_001\"}]";

            string path = CreateFile(
                "data.JSON",
                text);

            Result<string> result =
                DataImportSourceFileReader.Read(
                    path,
                    DataImportFormat.Json);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(text));
        }

        [Test]
        public void Read_WhitespaceContent_PreservesOriginalText()
        {
            string text = "   \n\t";

            string path = CreateFile(
                "data.csv",
                text);

            Result<string> result =
                DataImportSourceFileReader.Read(
                    path,
                    DataImportFormat.Csv);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(text));
        }

        [Test]
        public void Read_WrongExtension_ReturnsImportFailed()
        {
            string path = CreateFile(
                "data.json",
                "[]");

            Result<string> result =
                DataImportSourceFileReader.Read(
                    path,
                    DataImportFormat.Csv);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Read_MissingFile_ReturnsImportFailed()
        {
            string path = Path.Combine(
                temporaryDirectory,
                "missing.csv");

            Result<string> result =
                DataImportSourceFileReader.Read(
                    path,
                    DataImportFormat.Csv);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Read_EmptyPath_ReturnsImportFailed()
        {
            Result<string> result =
                DataImportSourceFileReader.Read(
                    string.Empty,
                    DataImportFormat.Csv);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Read_UnsupportedFormat_ReturnsImportFailed()
        {
            string path = CreateFile(
                "data.csv",
                "id");

            Result<string> result =
                DataImportSourceFileReader.Read(
                    path,
                    (DataImportFormat)999);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));
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
    }
}