using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Editor.Importing;
using NUnit.Framework;

namespace CDG.Data.Tests.Editor
{
    public sealed class CsvParserTests
    {
        [Test]
        public void Parse_SimpleRow_ReturnsExpectedFields()
        {
            Result<CsvDocument> result = CsvParser.Parse("id,name,itemType");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(1));
            Assert.That(result.Value[0].Count, Is.EqualTo(3));
            Assert.That(result.Value[0][0], Is.EqualTo("id"));
            Assert.That(result.Value[0][1], Is.EqualTo("name"));
            Assert.That(result.Value[0][2], Is.EqualTo("itemType"));
        }

        [Test]
        public void Parse_MultipleRowsWithLf_PreservesRows()
        {
            Result<CsvDocument> result = CsvParser.Parse("id,name\nitem_001,Sword\nitem_002,Shield");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(3));
            Assert.That(result.Value[1][0], Is.EqualTo("item_001"));
            Assert.That(result.Value[2][1], Is.EqualTo("Shield"));
        }

        [Test]
        public void Parse_MultipleRowsWithCrLf_PreservesRows()
        {
            Result<CsvDocument> result = CsvParser.Parse("id,name\r\nitem_001,Sword\r\nitem_002,Shield");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(3));
            Assert.That(result.Value[0].StartLine, Is.EqualTo(1));
            Assert.That(result.Value[1].StartLine, Is.EqualTo(2));
            Assert.That(result.Value[2].StartLine, Is.EqualTo(3));
        }

        [Test]
        public void Parse_EmptyFields_PreservesEmptyValues()
        {
            Result<CsvDocument> result = CsvParser.Parse("item_001,,100,");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value[0].Count, Is.EqualTo(4));
            Assert.That(result.Value[0][0], Is.EqualTo("item_001"));
            Assert.That(result.Value[0][1], Is.EqualTo(""));
            Assert.That(result.Value[0][2], Is.EqualTo("100"));
            Assert.That(result.Value[0][3], Is.EqualTo(""));
        }

        [Test]
        public void Parse_QuotedComma_ReturnsSingleField()
        {
            Result<CsvDocument> result = CsvParser.Parse("item_001,\"Basic, Sword\"");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value[0].Count, Is.EqualTo(2));
            Assert.That(result.Value[0][1], Is.EqualTo("Basic, Sword"));
        }

        [Test]
        public void Parse_EscapedQuote_ReturnsSingleQuote()
        {
            Result<CsvDocument> result = CsvParser.Parse("item_001,\"He said \"\"Block!\"\"\"");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value[0][1], Is.EqualTo("He said \"Block!\""));
        }

        [Test]
        public void Parse_QuotedNewLine_PreservesNewLineInsideField()
        {
            Result<CsvDocument> result = CsvParser.Parse("item_001,\"First Line\nSecond Line\"\nitem_002,Shield");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(2));
            Assert.That(result.Value[0][1], Is.EqualTo("First Line\nSecond Line"));
            Assert.That(result.Value[1].StartLine, Is.EqualTo(3));
        }

        [Test]
        public void Parse_QuotedEmptyField_ReturnsEmptyValue()
        {
            Result<CsvDocument> result = CsvParser.Parse("item_001,\"\"");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value[0][1], Is.EqualTo(""));
        }

        [Test]
        public void Parse_TrailingRowSeparator_DoesNotCreateAdditionalRow()
        {
            Result<CsvDocument> result = CsvParser.Parse("item_001,Sword\n");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(1));
        }

        [Test]
        public void Parse_UnclosedQuotedField_ReturnsImportFailed()
        {
            Result<CsvDocument> result = CsvParser.Parse("item_001,\"Sword");

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Parse_QuoteInsideUnquotedField_ReturnsImportFailed()
        {
            Result<CsvDocument> result = CsvParser.Parse("item_001,Swo\"rd");

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Parse_CharacterAfterClosingQuote_ReturnsImportFailed()
        {
            Result<CsvDocument> result = CsvParser.Parse("item_001,\"Sword\"Invalid");

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
        }
    }
}