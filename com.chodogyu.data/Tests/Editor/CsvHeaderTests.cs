using System;
using System.Collections.Generic;
using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Editor.Importing;
using NUnit.Framework;

namespace CDG.Data.Tests.Editor
{
    public sealed class CsvHeaderTests
    {
        [Test]
        public void Create_ValidRow_PreservesColumnsAndIndexes()
        {
            CsvRow row = new CsvRow(new[]
            {
                "id",
                "name",
                "price"
            }, 1);

            Result<CsvHeader> result = CsvHeader.Create(row);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(3));
            Assert.That(result.Value.Columns[0], Is.EqualTo("id"));
            Assert.That(result.Value.Columns[1], Is.EqualTo("name"));
            Assert.That(result.Value.Columns[2], Is.EqualTo("price"));

            Assert.That(result.Value.TryGetIndex("id", out int idIndex), Is.True);
            Assert.That(idIndex, Is.EqualTo(0));

            Assert.That(result.Value.TryGetIndex("price", out int priceIndex), Is.True);
            Assert.That(priceIndex, Is.EqualTo(2));
        }

        [Test]
        public void Create_EmptyColumn_ReturnsImportFailed()
        {
            CsvRow row = new CsvRow(new[]
            {
                "id",
                "",
                "price"
            }, 1);

            Result<CsvHeader> result = CsvHeader.Create(row);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Create_WhitespaceColumn_ReturnsImportFailed()
        {
            CsvRow row = new CsvRow(new[]
            {
                "id",
                "   ",
                "price"
            }, 1);

            Result<CsvHeader> result = CsvHeader.Create(row);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Create_ColumnWithLeadingWhitespace_ReturnsImportFailed()
        {
            CsvRow row = new CsvRow(new[]
            {
                "id",
                " name",
                "price"
            }, 1);

            Result<CsvHeader> result = CsvHeader.Create(row);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Create_ColumnWithTrailingWhitespace_ReturnsImportFailed()
        {
            CsvRow row = new CsvRow(new[]
            {
                "id",
                "name ",
                "price"
            }, 1);

            Result<CsvHeader> result = CsvHeader.Create(row);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Create_DuplicateColumn_ReturnsImportFailed()
        {
            CsvRow row = new CsvRow(new[]
            {
                "id",
                "name",
                "id"
            }, 1);

            Result<CsvHeader> result = CsvHeader.Create(row);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Create_ColumnsWithDifferentCase_Succeeds()
        {
            CsvRow row = new CsvRow(new[]
            {
                "id",
                "Id"
            }, 1);

            Result<CsvHeader> result = CsvHeader.Create(row);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(2));

            Assert.That(result.Value.TryGetIndex("id", out int lowerIndex), Is.True);
            Assert.That(lowerIndex, Is.EqualTo(0));

            Assert.That(result.Value.TryGetIndex("Id", out int upperIndex), Is.True);
            Assert.That(upperIndex, Is.EqualTo(1));
        }

        [Test]
        public void TryGetIndex_UnknownColumn_ReturnsFalseAndMinusOne()
        {
            CsvRow row = new CsvRow(new[]
            {
                "id",
                "name"
            }, 1);

            Result<CsvHeader> result = CsvHeader.Create(row);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.TryGetIndex("price", out int index), Is.False);
            Assert.That(index, Is.EqualTo(-1));
            Assert.That(result.Value.Contains("price"), Is.False);
        }

        [Test]
        public void TryGetIndex_NullColumn_ReturnsFalseAndMinusOne()
        {
            CsvRow row = new CsvRow(new[]
            {
                "id",
                "name"
            }, 1);

            Result<CsvHeader> result = CsvHeader.Create(row);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.TryGetIndex(null, out int index), Is.False);
            Assert.That(index, Is.EqualTo(-1));
            Assert.That(result.Value.Contains(null), Is.False);
        }

        [Test]
        public void Columns_ModificationThroughIList_ThrowsNotSupportedException()
        {
            CsvRow row = new CsvRow(new[]
            {
                "id",
                "name"
            }, 1);

            Result<CsvHeader> result = CsvHeader.Create(row);

            Assert.That(result.IsSuccess, Is.True);

            IList<string> columns = (IList<string>)result.Value.Columns;

            Assert.Throws<NotSupportedException>(() => columns[0] = "changed");
            Assert.That(result.Value.Columns[0], Is.EqualTo("id"));
        }

        [Test]
        public void Create_NullRow_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => CsvHeader.Create(null));
        }
    }
}