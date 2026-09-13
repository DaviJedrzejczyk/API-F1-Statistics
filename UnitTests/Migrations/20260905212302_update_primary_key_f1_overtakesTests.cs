using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using NUnit.Framework;

#nullable enable

namespace UnitTests.Migrations
{
    [TestFixture]
    public class UpdatePrimaryKeyF1OvertakesTests
    {
        private class TestableMigration : global::Dao.Migrations.update_primary_key_f1_overtakes
        {
            public void InvokeUp(MigrationBuilder builder) => base.Up(builder);
            public void InvokeDown(MigrationBuilder builder) => base.Down(builder);
        }

        [Test]
        public void Up_WithMigrationBuilder_AddsDropAndAddPrimaryKey()
        {
            // Arrange
            var migration = new TestableMigration();
            var builder = new MigrationBuilder("Test");

            // Act
            migration.InvokeUp(builder);

            // Assert
            Assert.That(builder.Operations, Is.Not.Null);
            Assert.That(builder.Operations.Count, Is.EqualTo(2));

            var first = builder.Operations[0] as DropPrimaryKeyOperation;
            Assert.That(first, Is.Not.Null);
            Assert.That(first!.Name, Is.EqualTo("PK_F1_OVERTAKES"));
            Assert.That(first.Table, Is.EqualTo("F1_OVERTAKES"));

            var second = builder.Operations[1] as AddPrimaryKeyOperation;
            Assert.That(second, Is.Not.Null);
            Assert.That(second!.Name, Is.EqualTo("PK_F1_OVERTAKES"));
            Assert.That(second.Table, Is.EqualTo("F1_OVERTAKES"));
            Assert.That(second.Columns, Is.Not.Null);
            Assert.That(second.Columns.Count, Is.EqualTo(6));
            var expected = new[] { "MeetingKey", "SessionKey", "OvertakingDriverNumber", "OvertakedDriverNumber", "Date", "Position" };
            CollectionAssert.AreEqual(expected, second.Columns);
        }

        [Test]
        public void Down_WithMigrationBuilder_AddsDropAndAddPrimaryKey_WithOriginalColumns()
        {
            // Arrange
            var migration = new TestableMigration();
            var builder = new MigrationBuilder("Test");

            // Act
            migration.InvokeDown(builder);

            // Assert
            Assert.That(builder.Operations, Is.Not.Null);
            Assert.That(builder.Operations.Count, Is.EqualTo(2));

            var first = builder.Operations[0] as DropPrimaryKeyOperation;
            Assert.That(first, Is.Not.Null);
            Assert.That(first!.Name, Is.EqualTo("PK_F1_OVERTAKES"));
            Assert.That(first.Table, Is.EqualTo("F1_OVERTAKES"));

            var second = builder.Operations[1] as AddPrimaryKeyOperation;
            Assert.That(second, Is.Not.Null);
            Assert.That(second!.Name, Is.EqualTo("PK_F1_OVERTAKES"));
            Assert.That(second.Table, Is.EqualTo("F1_OVERTAKES"));
            Assert.That(second.Columns, Is.Not.Null);
            Assert.That(second.Columns.Count, Is.EqualTo(2));
            var expected = new[] { "MeetingKey", "SessionKey" };
            CollectionAssert.AreEqual(expected, second.Columns);
        }
    }
}
