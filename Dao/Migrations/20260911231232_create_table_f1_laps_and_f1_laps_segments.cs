using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class create_table_f1_laps_and_f1_laps_segments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "F1_LAPS",
                columns: table => new
                {
                    DriverNumber = table.Column<int>(type: "int", nullable: false),
                    LapNumber = table.Column<int>(type: "int", nullable: false),
                    MeetingKey = table.Column<int>(type: "int", nullable: false),
                    SessionKey = table.Column<int>(type: "int", nullable: false),
                    DateStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationSector1 = table.Column<double>(type: "float", nullable: false),
                    DurationSector2 = table.Column<double>(type: "float", nullable: false),
                    DurationSector3 = table.Column<double>(type: "float", nullable: false),
                    I1Speed = table.Column<int>(type: "int", nullable: false),
                    I2Speed = table.Column<int>(type: "int", nullable: false),
                    IsPitOutLap = table.Column<bool>(type: "bit", nullable: false),
                    LapDuration = table.Column<double>(type: "float", nullable: false),
                    StSpeed = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_F1_LAPS", x => new { x.MeetingKey, x.SessionKey, x.DriverNumber, x.LapNumber });
                });

            migrationBuilder.CreateTable(
                name: "F1_LAPS_SEGMENTS",
                columns: table => new
                {
                    MeetingKey = table.Column<int>(type: "int", nullable: false),
                    SessionKey = table.Column<int>(type: "int", nullable: false),
                    DriverNumber = table.Column<int>(type: "int", nullable: false),
                    LapNumber = table.Column<int>(type: "int", nullable: false),
                    Sector = table.Column<int>(type: "int", nullable: false),
                    SegmentIndex = table.Column<int>(type: "int", nullable: false),
                    SegmentStatus = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_F1_LAPS_SEGMENTS", x => new { x.MeetingKey, x.SessionKey, x.DriverNumber, x.LapNumber, x.Sector, x.SegmentIndex });
                    table.ForeignKey(
                        name: "FK_F1_LAPS_SEGMENTS_F1_LAPS_MeetingKey_SessionKey_DriverNumber_LapNumber",
                        columns: x => new { x.MeetingKey, x.SessionKey, x.DriverNumber, x.LapNumber },
                        principalTable: "F1_LAPS",
                        principalColumns: new[] { "MeetingKey", "SessionKey", "DriverNumber", "LapNumber" },
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "F1_LAPS_SEGMENTS");

            migrationBuilder.DropTable(
                name: "F1_LAPS");
        }
    }
}
