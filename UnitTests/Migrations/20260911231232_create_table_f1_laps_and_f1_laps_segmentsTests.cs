using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using NUnit.Framework;
using Dao.Migrations;

#nullable enable

namespace UnitTests.Migrations
{
    [TestFixture]
    public class CreateTableF1LapsAndSegmentsTests
    {
        [Test]
        public void Up_WhenCalled_Adds_CreateTable_Operations_With_Expected_Names_And_Columns()
        {
            // Arrange
            var migration = new TestableMigration();
            var builder = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");

            // Act
            migration.Up_public(builder);

            // Assert
            var ops = builder.Operations;
            Assert.That(ops, Is.Not.Null);
            Assert.That(ops.Count, Is.EqualTo(2), "Expected two operations (two CreateTable operations)");

            var createOps = ops.OfType<CreateTableOperation>().ToList();
            Assert.That(createOps.Count, Is.EqualTo(2), "Expected two CreateTableOperation instances");

            var laps = createOps.SingleOrDefault(o => string.Equals(o.Name, "F1_LAPS", StringComparison.Ordinal));
            Assert.That(laps, Is.Not.Null, "F1_LAPS table should be created");
            Assert.That(laps!.Columns, Is.Not.Null);
            Assert.That(laps.Columns.Count, Is.EqualTo(13));
            Assert.That(laps.Columns.Any(c => c.Name == "DriverNumber"));
            Assert.That(laps.Columns.Any(c => c.Name == "LapNumber"));
            Assert.That(laps.Columns.Any(c => c.Name == "MeetingKey"));
            Assert.That(laps.Columns.Any(c => c.Name == "SessionKey"));
            Assert.That(laps.Columns.Any(c => c.Name == "DateStart"));
            Assert.That(laps.Columns.Any(c => c.Name == "DurationSector1"));
            Assert.That(laps.Columns.Any(c => c.Name == "DurationSector2"));
            Assert.That(laps.Columns.Any(c => c.Name == "DurationSector3"));
            Assert.That(laps.Columns.Any(c => c.Name == "I1Speed"));
            Assert.That(laps.Columns.Any(c => c.Name == "I2Speed"));
            Assert.That(laps.Columns.Any(c => c.Name == "IsPitOutLap"));
            Assert.That(laps.Columns.Any(c => c.Name == "LapDuration"));
            Assert.That(laps.Columns.Any(c => c.Name == "StSpeed"));

            Assert.That(laps.PrimaryKey, Is.Not.Null);
            Assert.That(laps.PrimaryKey!.Name, Is.EqualTo("PK_F1_LAPS"));
            Assert.That(laps.PrimaryKey.Columns, Is.EquivalentTo(new[] { "MeetingKey", "SessionKey", "DriverNumber", "LapNumber" }));

            var seg = createOps.SingleOrDefault(o => string.Equals(o.Name, "F1_LAPS_SEGMENTS", StringComparison.Ordinal));
            Assert.That(seg, Is.Not.Null, "F1_LAPS_SEGMENTS table should be created");
            Assert.That(seg!.Columns, Is.Not.Null);
            Assert.That(seg.Columns.Count, Is.EqualTo(7));
            Assert.That(seg.Columns.Any(c => c.Name == "MeetingKey"));
            Assert.That(seg.Columns.Any(c => c.Name == "SessionKey"));
            Assert.That(seg.Columns.Any(c => c.Name == "DriverNumber"));
            Assert.That(seg.Columns.Any(c => c.Name == "LapNumber"));
            Assert.That(seg.Columns.Any(c => c.Name == "Sector"));
            Assert.That(seg.Columns.Any(c => c.Name == "SegmentIndex"));
            Assert.That(seg.Columns.Any(c => c.Name == "SegmentStatus"));

            Assert.That(seg.PrimaryKey, Is.Not.Null);
            Assert.That(seg.PrimaryKey!.Name, Is.EqualTo("PK_F1_LAPS_SEGMENTS"));
            Assert.That(seg.PrimaryKey.Columns, Is.EquivalentTo(new[] { "MeetingKey", "SessionKey", "DriverNumber", "LapNumber", "Sector", "SegmentIndex" }));

            // Foreign key check
            Assert.That(seg.ForeignKeys, Is.Not.Null);
            var fk = seg.ForeignKeys.SingleOrDefault();
            Assert.That(fk, Is.Not.Null);
            Assert.That(fk!.PrincipalTable, Is.EqualTo("F1_LAPS"));
            Assert.That(fk.PrincipalColumns, Is.EquivalentTo(new[] { "MeetingKey", "SessionKey", "DriverNumber", "LapNumber" }));
            Assert.That(fk.Columns, Is.EquivalentTo(new[] { "MeetingKey", "SessionKey", "DriverNumber", "LapNumber" }));
        }

        [Test]
        public void Down_WhenCalled_Adds_DropTable_Operations_For_Created_Tables()
        {
            // Arrange
            var migration = new TestableMigration();
            var builder = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");

            // Act
            migration.Down_public(builder);

            // Assert
            var ops = builder.Operations;
            Assert.That(ops, Is.Not.Null);
            var drops = ops.OfType<DropTableOperation>().ToList();
            Assert.That(drops.Count, Is.EqualTo(2));

            Assert.That(drops.Any(d => string.Equals(d.Name, "F1_LAPS_SEGMENTS", StringComparison.Ordinal)));
            Assert.That(drops.Any(d => string.Equals(d.Name, "F1_LAPS", StringComparison.Ordinal)));
        }
        // Helper subclass to expose protected Up/Down methods for testing
        private class TestableMigration : create_table_f1_laps_and_f1_laps_segments
        {
            public void Up_public(MigrationBuilder migrationBuilder) => base.Up(migrationBuilder);
            public void Down_public(MigrationBuilder migrationBuilder) => base.Down(migrationBuilder);
        }
    }
}

