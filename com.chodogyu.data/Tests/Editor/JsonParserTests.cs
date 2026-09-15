using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Editor.Importing;
using NUnit.Framework;

namespace CDG.Data.Tests.Editor
{
    public sealed class JsonParserTests
    {
        [Test]
        public void Parse_EmptyArray_ReturnsEmptyDocument()
        {
            Result<JsonDocument> result = JsonParser.Parse("[]");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(0));
        }

        [Test]
        public void Parse_SingleObject_PreservesObjectText()
        {
            Result<JsonDocument> result = JsonParser.Parse(
                "[{\"id\":\"item_001\"}]");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(1));
            Assert.That(result.Value[0].Text, Is.EqualTo(
                "{\"id\":\"item_001\"}"));
            Assert.That(result.Value[0].StartLine, Is.EqualTo(1));
        }

        [Test]
        public void Parse_MultipleObjects_PreservesOriginalOrder()
        {
            Result<JsonDocument> result = JsonParser.Parse(
                "[{\"id\":\"item_001\"},{\"id\":\"item_002\"}]");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(2));
            Assert.That(result.Value[0].Text, Does.Contain("item_001"));
            Assert.That(result.Value[1].Text, Does.Contain("item_002"));
        }

        [Test]
        public void Parse_MultiLineObjects_TracksStartLines()
        {
            Result<JsonDocument> result = JsonParser.Parse(
                "[\n" +
                "  {\"id\":\"item_001\"},\n" +
                "  {\"id\":\"item_002\"}\n" +
                "]");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(2));
            Assert.That(result.Value[0].StartLine, Is.EqualTo(2));
            Assert.That(result.Value[1].StartLine, Is.EqualTo(3));
        }

        [Test]
        public void Parse_NestedObjectAndArray_PreservesWholeObject()
        {
            const string objectText =
                "{\"id\":\"item_001\",\"stats\":{\"attack\":10},\"tags\":[\"weapon\",\"rare\"]}";

            Result<JsonDocument> result = JsonParser.Parse(
                "[" + objectText + "]");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(1));
            Assert.That(result.Value[0].Text, Is.EqualTo(objectText));
        }

        [Test]
        public void Parse_BracketsInsideString_DoesNotChangeObjectBoundary()
        {
            Result<JsonDocument> result = JsonParser.Parse(
                "[{\"id\":\"item_001\",\"text\":\"{ [ ] }\"}]");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(1));
        }

        [Test]
        public void Parse_EscapedQuoteAndUnicodeEscape_Succeeds()
        {
            Result<JsonDocument> result = JsonParser.Parse(
                "[{\"id\":\"item_\\u0031\",\"text\":\"A\\\"B\"}]");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(1));
        }

        [Test]
        public void Parse_RootIsNotArray_ReturnsImportFailed()
        {
            Result<JsonDocument> result = JsonParser.Parse(
                "{\"id\":\"item_001\"}");

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Parse_ArrayElementIsNotObject_ReturnsImportFailed()
        {
            Result<JsonDocument> result = JsonParser.Parse("[1]");

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Parse_TrailingComma_ReturnsImportFailed()
        {
            Result<JsonDocument> result = JsonParser.Parse(
                "[{\"id\":\"item_001\"},]");

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Parse_UnclosedObject_ReturnsImportFailed()
        {
            Result<JsonDocument> result = JsonParser.Parse(
                "[{\"id\":\"item_001\"");

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Parse_UnclosedString_ReturnsImportFailed()
        {
            Result<JsonDocument> result = JsonParser.Parse(
                "[{\"id\":\"item_001}]");

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Parse_InvalidEscape_ReturnsImportFailed()
        {
            Result<JsonDocument> result = JsonParser.Parse(
                "[{\"id\":\"item_\\q\"}]");

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Parse_UnescapedNewLineInsideString_ReturnsImportFailed()
        {
            Result<JsonDocument> result = JsonParser.Parse(
                "[{\"text\":\"First\nSecond\"}]");

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Parse_ExtraTextAfterRootArray_ReturnsImportFailed()
        {
            Result<JsonDocument> result = JsonParser.Parse(
                "[] trailing");

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
        }
    }
}