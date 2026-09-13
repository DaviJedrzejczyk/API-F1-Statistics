using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using NUnit.Framework;
using Dao.Migrations;

#nullable enable

namespace UnitTests.Migrations
{
    [TestFixture]
    public class Tests_20260909234056_add_column_Compound_f1_Session_Results
    {
        private class TestableMigration : add_column_Compound_f1_Session_Results
        {
            public void CallUp(MigrationBuilder mb) => base.Up(mb);
            public void CallDown(MigrationBuilder mb) => base.Down(mb);
        }

        [Test]
        public void Up_Adds_Compound_Column()
        {
            // Arrange
            var migration = new TestableMigration();
            var migrationBuilder = new MigrationBuilder("TestProvider");

            // Act
            migration.CallUp(migrationBuilder);

            // Assert
            var addOp = migrationBuilder.Operations.OfType<AddColumnOperation>().FirstOrDefault();
            Assert.IsNotNull(addOp, "Expected an AddColumnOperation to be added.");
            Assert.That(addOp, Is.Not.Null);
            Assert.AreEqual("Compound", addOp!.Name);
            Assert.AreEqual("F1_SESSION_RESULTS", addOp.Table);
            Assert.AreEqual("nvarchar(10)", addOp.ColumnType);
            Assert.AreEqual(10, addOp.MaxLength);
            Assert.IsFalse(addOp.IsNullable);
            Assert.AreEqual("", addOp.DefaultValue);
        }

        [Test]
        public void Down_Removes_Compound_Column()
        {
            // Arrange
            var migration = new TestableMigration();
            var migrationBuilder = new MigrationBuilder("TestProvider");

            // Act
            migration.CallDown(migrationBuilder);

            // Assert
            var dropOp = migrationBuilder.Operations.OfType<DropColumnOperation>().FirstOrDefault();
            Assert.IsNotNull(dropOp, "Expected a DropColumnOperation to be added.");
            Assert.AreEqual("Compound", dropOp!.Name);
            Assert.AreEqual("F1_SESSION_RESULTS", dropOp.Table);
        }
    }
}
