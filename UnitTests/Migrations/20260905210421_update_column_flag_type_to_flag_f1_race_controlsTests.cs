using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using NUnit.Framework;
using Dao.Migrations;
using Moq;

namespace UnitTests.Migrations
{
    [TestFixture]
    public class update_column_flag_type_to_flag_f1_race_controlsTests
    {
        private sealed class TestableMigration : update_column_flag_type_to_flag_f1_race_controls
        {
            public void PublicUp(MigrationBuilder builder) => base.Up(builder);
            public void PublicDown(MigrationBuilder builder) => base.Down(builder);
        }

        [Test]
        public void Up_WhenCalled_Adds_DropColumn_And_AddColumn_Operations()
        {
            // Arrange
            var migration = new TestableMigration();
            var builder = new MigrationBuilder("Test");

            // Act
            migration.PublicUp(builder);

            // Assert
            var drop = builder.Operations.OfType<DropColumnOperation>()
                .SingleOrDefault(o => o.Name == "FlagType" && o.Table == "F1_RACE_CONTROLS");
            Assert.That(drop, Is.Not.Null, "Expected a DropColumnOperation for FlagType on F1_RACE_CONTROLS");

            var add = builder.Operations.OfType<AddColumnOperation>()
                .SingleOrDefault(o => o.Name == "Flag" && o.Table == "F1_RACE_CONTROLS");
            Assert.That(add, Is.Not.Null, "Expected an AddColumnOperation for Flag on F1_RACE_CONTROLS");

            // verify column specifics if available
            var addNonNull = add!;
            Assert.That(addNonNull.ColumnType, Is.EqualTo("nvarchar(25)"));
            Assert.That(addNonNull.MaxLength, Is.EqualTo(25));
            Assert.That(addNonNull.IsNullable, Is.True);
        }

        [Test]
        public void Down_WhenCalled_Adds_DropColumn_And_AddColumn_Back_To_FlagType()
        {
            // Arrange
            var migration = new TestableMigration();
            var builder = new MigrationBuilder("Test");

            // Act
            migration.PublicDown(builder);

            // Assert
            var drop = builder.Operations.OfType<DropColumnOperation>()
                .SingleOrDefault(o => o.Name == "Flag" && o.Table == "F1_RACE_CONTROLS");
            Assert.That(drop, Is.Not.Null, "Expected a DropColumnOperation for Flag on F1_RACE_CONTROLS");

            var add = builder.Operations.OfType<AddColumnOperation>()
                .SingleOrDefault(o => o.Name == "FlagType" && o.Table == "F1_RACE_CONTROLS");
            Assert.That(add, Is.Not.Null, "Expected an AddColumnOperation for FlagType on F1_RACE_CONTROLS");

            // verify column specifics if available
            var addNonNull = add!;
            Assert.That(addNonNull.ColumnType, Is.EqualTo("nvarchar(50)"));
            Assert.That(addNonNull.MaxLength, Is.EqualTo(50));
            Assert.That(addNonNull.IsNullable, Is.True);
        }
    }
}
