using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class add_colum_IsFastLap_f1_lapsTests
    {
        private class TestableMigration : global::Dao.Migrations.add_colum_IsFastLap_f1_laps
        {
            public void InvokeUp(MigrationBuilder builder) => base.Up(builder);
            public void InvokeDown(MigrationBuilder builder) => base.Down(builder);
        }

        [Test]
        public void Up_Adds_IsFastLap_Column()
        {
            // Arrange
            var migration = new TestableMigration();
            var builder = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");

            // Act
            migration.InvokeUp(builder);

            // Assert
            var addColumn = builder.Operations.OfType<AddColumnOperation>().SingleOrDefault();
            Assert.IsNotNull(addColumn, "Expected exactly one AddColumnOperation to be added.");
            var add = addColumn!;
            Assert.AreEqual("IsFastLap", add.Name);
            Assert.AreEqual("F1_LAPS", add.Table);
            Assert.AreEqual("bit", add.ColumnType);
            // Depending on EF Core version the nullable property may be IsNullable
            var prop = add.GetType().GetProperty("IsNullable") ?? add.GetType().GetProperty("Nullable");
            Assert.IsNotNull(prop, "Expected nullable property to exist on AddColumnOperation");
            Assert.AreEqual(false, (bool)prop.GetValue(add)!);
            Assert.AreEqual(false, add.DefaultValue);
        }

        [Test]
        public void Down_Adds_DropColumn_Operation_For_IsFastLap()
        {
            // Arrange
            var migration = new TestableMigration();
            var builder = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");

            // Act
            migration.InvokeDown(builder);

            // Assert
            var dropColumn = builder.Operations.OfType<DropColumnOperation>().SingleOrDefault();
            Assert.IsNotNull(dropColumn, "Expected exactly one DropColumnOperation to be added.");
            var drop = dropColumn!;
            Assert.AreEqual("IsFastLap", drop.Name);
            Assert.AreEqual("F1_LAPS", drop.Table);
        }
    }
}
