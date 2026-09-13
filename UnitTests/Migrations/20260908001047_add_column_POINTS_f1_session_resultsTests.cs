using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using NUnit.Framework;

#nullable enable

namespace UnitTests.Migrations
{
    [TestFixture]
    public class add_column_POINTS_f1_session_resultsTests
    {
        private sealed class TestableMigration : global::Dao.Migrations.add_column_POINTS_f1_session_results
        {
            public void InvokeUp(MigrationBuilder builder) => base.Up(builder);
            public void InvokeDown(MigrationBuilder builder) => base.Down(builder);
        }

        [Test]
        public void Up_WhenCalled_AddsIntPointsColumnToF1SessionResults()
        {
            // Arrange
            var migration = new TestableMigration();
            var builder = new MigrationBuilder("TestProvider");

            // Act
            migration.InvokeUp(builder);

            // Assert
            var addOp = builder.Operations.OfType<AddColumnOperation>().Single();
            Assert.That(addOp.Name, Is.EqualTo("Points"));
            Assert.That(addOp.Table, Is.EqualTo("F1_SESSION_RESULTS"));
            // ColumnType corresponds to the SQL type passed in migration
            Assert.That(addOp.ColumnType, Is.EqualTo("int"));
            Assert.That(addOp.IsNullable, Is.False);
            Assert.That(addOp.DefaultValue, Is.EqualTo(0));
        }

        [Test]
        public void Down_WhenCalled_DropsPointsColumnFromF1SessionResults()
        {
            // Arrange
            var migration = new TestableMigration();
            var builder = new MigrationBuilder("TestProvider");

            // Act
            migration.InvokeDown(builder);

            // Assert
            var dropOp = builder.Operations.OfType<DropColumnOperation>().Single();
            Assert.That(dropOp.Name, Is.EqualTo("Points"));
            Assert.That(dropOp.Table, Is.EqualTo("F1_SESSION_RESULTS"));
        }
    }
}
