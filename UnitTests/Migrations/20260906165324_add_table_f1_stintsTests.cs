#nullable enable
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using NUnit.Framework;
using Dao.Migrations;

namespace UnitTests
{
    [TestFixture]
    public class add_table_f1_stintsTests
    {
        [Test]
        public void Up_WithMigrationBuilder_CreatesF1StintsTableWithExpectedColumnsAndPrimaryKey()
        {
            // Arrange
            var builder = new MigrationBuilder(activeProvider: null);
            var sut = new TestableAddTableF1Stints();

            // Act
            sut.InvokeUp(builder);

            // Assert - one CreateTableOperation
            var createTable = builder.Operations.OfType<CreateTableOperation>().Single();
            Assert.That(createTable.Name, Is.EqualTo("F1_STINTS"));

            // Assert - primary key
            Assert.That(createTable.PrimaryKey, Is.Not.Null);
            Assert.That(createTable.PrimaryKey!.Name, Is.EqualTo("PK_F1_STINTS"));

            // Assert - expected columns present
            var expectedColumns = new[]
            {
                "DriverNumber",
                "MeetingKey",
                "SessionKey",
                "StintNumber",
                "Compound",
                "LapEnd",
                "LapStart",
                "TyreAgeAtStart"
            };

            var actualColumnNames = createTable.Columns.Select(c => c.Name).ToArray();
            CollectionAssert.AreEquivalent(expectedColumns, actualColumnNames);
        }

        [Test]
        public void Down_WithMigrationBuilder_DropsF1StintsTable()
        {
            // Arrange
            var builder = new MigrationBuilder(activeProvider: null);
            var sut = new TestableAddTableF1Stints();

            // Act
            sut.InvokeDown(builder);

            // Assert - one DropTableOperation
            var drop = builder.Operations.OfType<DropTableOperation>().Single();
            Assert.That(drop.Name, Is.EqualTo("F1_STINTS"));
        }

        // Helper to expose protected methods for testing
        private class TestableAddTableF1Stints : add_table_f1_stints
        {
            public void InvokeUp(MigrationBuilder builder) => base.Up(builder);
            public void InvokeDown(MigrationBuilder builder) => base.Down(builder);
        }
    }
}
