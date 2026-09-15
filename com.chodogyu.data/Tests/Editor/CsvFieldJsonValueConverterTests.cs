using System;
using System.Globalization;
using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Editor.Importing;
using NUnit.Framework;

namespace CDG.Data.Tests.Editor
{
    public sealed class CsvFieldJsonValueConverterTests
    {
        [TestCase("-128", typeof(sbyte), "-128")]
        [TestCase("255", typeof(byte), "255")]
        [TestCase("-32768", typeof(short), "-32768")]
        [TestCase("65535", typeof(ushort), "65535")]
        [TestCase("-2147483648", typeof(int), "-2147483648")]
        [TestCase("4294967295", typeof(uint), "4294967295")]
        [TestCase("-9223372036854775808", typeof(long), "-9223372036854775808")]
        [TestCase("18446744073709551615", typeof(ulong), "18446744073709551615")]
        [TestCase("1.25", typeof(float), "1.25")]
        [TestCase("1.25", typeof(double), "1.25")]
        public void ConvertToJsonLiteral_SupportedNumber_ReturnsInvariantLiteral(string value, Type fieldType, string expected)
        {
            Result<string> result =
                CsvFieldJsonValueConverter.ConvertToJsonLiteral(
                    value,
                    fieldType);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToJsonLiteral_BoolMixedCase_ReturnsLowercaseJsonLiteral()
        {
            Result<string> result =
                CsvFieldJsonValueConverter.ConvertToJsonLiteral(
                    "TrUe",
                    typeof(bool));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo("true"));
        }

        [Test]
        public void ConvertToJsonLiteral_StringWithOuterWhitespace_PreservesWhitespace()
        {
            Result<string> result =
                CsvFieldJsonValueConverter.ConvertToJsonLiteral(
                    "  item_001  ",
                    typeof(string));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo("\"  item_001  \""));
        }

        [Test]
        public void ToJsonString_SpecialCharacters_EscapesJsonCharacters()
        {
            string value = "a\"b\\c\n\r\t\b\f";

            string result =
                CsvFieldJsonValueConverter.ToJsonString(value);

            Assert.That(
                result,
                Is.EqualTo("\"a\\\"b\\\\c\\n\\r\\t\\b\\f\""));
        }

        [Test]
        public void ToJsonString_ControlCharacter_UsesUnicodeEscape()
        {
            string result =
                CsvFieldJsonValueConverter.ToJsonString(
                    "\u0001");

            Assert.That(
                result,
                Is.EqualTo("\"\\u0001\""));
        }

        [Test]
        public void ConvertToJsonLiteral_EnumName_ReturnsUnderlyingNumericValue()
        {
            Result<string> result =
                CsvFieldJsonValueConverter.ConvertToJsonLiteral(
                    "Rare",
                    typeof(TestKind));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo("1"));
        }

        [Test]
        public void ConvertToJsonLiteral_UnsignedEnum_ReturnsUnsignedNumericValue()
        {
            Result<string> result =
                CsvFieldJsonValueConverter.ConvertToJsonLiteral(
                    "Huge",
                    typeof(UnsignedKind));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(
                result.Value,
                Is.EqualTo("4000000000"));
        }

        [Test]
        public void ConvertToJsonLiteral_EnumNameWithWrongCase_ReturnsImportFailed()
        {
            Result<string> result =
                CsvFieldJsonValueConverter.ConvertToJsonLiteral(
                    "rare",
                    typeof(TestKind));

            Assert.That(result.IsFailure, Is.True);
            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void ConvertToJsonLiteral_NumberWithOuterWhitespace_ReturnsImportFailed()
        {
            Result<string> result =
                CsvFieldJsonValueConverter.ConvertToJsonLiteral(
                    " 10 ",
                    typeof(int));

            Assert.That(result.IsFailure, Is.True);
            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [TestCase("NaN", typeof(float))]
        [TestCase("Infinity", typeof(double))]
        public void ConvertToJsonLiteral_NonFiniteFloatingPoint_ReturnsImportFailed(string value, Type fieldType)
        {
            Result<string> result =
                CsvFieldJsonValueConverter.ConvertToJsonLiteral(
                    value,
                    fieldType);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [TestCase(typeof(decimal))]
        [TestCase(typeof(char))]
        public void ValidateSupportedType_UnsupportedScalarType_ReturnsImportFailed(Type fieldType)
        {
            Result result =
                CsvFieldJsonValueConverter.ValidateSupportedType(
                    fieldType);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void ConvertToJsonLiteral_NullValue_ReturnsImportFailed()
        {
            Result<string> result =
                CsvFieldJsonValueConverter.ConvertToJsonLiteral(
                    null,
                    typeof(string));

            Assert.That(result.IsFailure, Is.True);
            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void ValidateSupportedType_NullType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                CsvFieldJsonValueConverter.ValidateSupportedType(
                    null));
        }

        [Test]
        public void ConvertToJsonLiteral_NullType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                CsvFieldJsonValueConverter.ConvertToJsonLiteral(
                    "10",
                    null));
        }

        [Test]
        public void ToJsonString_NullValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                CsvFieldJsonValueConverter.ToJsonString(
                    null));
        }

        [Test]
        public void ConvertToJsonLiteral_FloatUsesInvariantCulture()
        {
            CultureInfo originalCulture =
                CultureInfo.CurrentCulture;

            try
            {
                CultureInfo.CurrentCulture =
                    new CultureInfo("de-DE");

                Result<string> result =
                    CsvFieldJsonValueConverter.ConvertToJsonLiteral(
                        "1.5",
                        typeof(float));

                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.EqualTo("1.5"));
            }
            finally
            {
                CultureInfo.CurrentCulture =
                    originalCulture;
            }
        }

        [Test]
        public void ConvertToJsonLiteral_IntegerOverflow_ReturnsImportFailed()
        {
            Result<string> result =
                CsvFieldJsonValueConverter.ConvertToJsonLiteral(
                    "2147483648",
                    typeof(int));

            Assert.That(result.IsFailure, Is.True);
            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void ConvertToJsonLiteral_InvalidBool_ReturnsImportFailed()
        {
            Result<string> result =
                CsvFieldJsonValueConverter.ConvertToJsonLiteral(
                    "yes",
                    typeof(bool));

            Assert.That(result.IsFailure, Is.True);
            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        private enum TestKind
        {
            Normal = 0,
            Rare = 1
        }

        private enum UnsignedKind : uint
        {
            Normal = 0,
            Huge = 4000000000U
        }
    }
}