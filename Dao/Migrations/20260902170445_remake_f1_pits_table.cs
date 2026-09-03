using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class remake_f1_pits_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "F1_PITS",
                columns: table => new
                {
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DriverNumber = table.Column<int>(type: "int", nullable: false),
                    MeetingKey = table.Column<int>(type: "int", nullable: false),
                    SessionKey = table.Column<int>(type: "int", nullable: false),
                    LaneDuration = table.Column<double>(type: "float", nullable: false),
                    LapNumber = table.Column<int>(type: "int", nullable: false),
                    PitDuration = table.Column<double>(type: "float", nullable: false),
                    StopDuration = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_F1_PITS", x => new { x.Date, x.DriverNumber, x.SessionKey, x.MeetingKey });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "F1_PITS");
        }
    }
}
