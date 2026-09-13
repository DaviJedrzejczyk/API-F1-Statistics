#nullable enable
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using NUnit.Framework;
using Moq;
using Dao.Migrations;

namespace UnitTests
{
    [TestFixture]
    public class SessionTableV1Tests
    {
        [Test]
        public void Up_WithMigrationBuilder_CreatesF1SessionTableAndIndexes()
        {
            // Arrange
            var builder = new MigrationBuilder(activeProvider: null);
            var sut = new TestableSessionTableV1();

            // Act
            sut.InvokeUp(builder);

            // Assert - one CreateTableOperation
            var createTable = builder.Operations.OfType<CreateTableOperation>().Single();
            Assert.That(createTable.Name, Is.EqualTo("F1_SESSION"));

            // Assert - primary key
            Assert.That(createTable.PrimaryKey, Is.Not.Null);
            Assert.That(createTable.PrimaryKey!.Name, Is.EqualTo("PK_F1_SESSION"));

            // Assert - expected columns present
            var expectedColumns = new[]
            {
                "SessionKey",
                "CircuitKey",
                "CircuitShortName",
                "CountryCode",
                "CountryKey",
                "CountryName",
                "DateStart",
                "DateEnd",
                "GmtOffset",
                "IsCancelled",
                "Location",
                "MeetingKey",
                "SessionName",
                "SessionType",
                "Year"
            };

            var actualColumnNames = createTable.Columns.Select(c => c.Name).ToArray();
            CollectionAssert.AreEquivalent(expectedColumns, actualColumnNames);

            // Assert - indexes
            var indexes = builder.Operations.OfType<CreateIndexOperation>().ToArray();
            Assert.That(indexes.Length, Is.EqualTo(3));

            Assert.That(indexes.Any(i => i.Name == "IX_F1_SESSION_CircuitKey" && i.Table == "F1_SESSION"), Is.True);
            Assert.That(indexes.Any(i => i.Name == "IX_F1_SESSION_MeetingKey" && i.Table == "F1_SESSION"), Is.True);
            Assert.That(indexes.Any(i => i.Name == "IX_F1_SESSION_Year" && i.Table == "F1_SESSION"), Is.True);
        }

        [Test]
        public void Down_WithMigrationBuilder_DropsF1SessionTable()
        {
            // Arrange
            var builder = new MigrationBuilder(activeProvider: null);
            var sut = new TestableSessionTableV1();

            // Act
            sut.InvokeDown(builder);

            // Assert - one DropTableOperation
            var drop = builder.Operations.OfType<DropTableOperation>().Single();
            Assert.That(drop.Name, Is.EqualTo("F1_SESSION"));
        }
        // Helper to expose protected methods for testing
        private class TestableSessionTableV1 : SessionTableV1
        {
            public void InvokeUp(MigrationBuilder builder) => base.Up(builder);
            public void InvokeDown(MigrationBuilder builder) => base.Down(builder);
        }

    }
}
