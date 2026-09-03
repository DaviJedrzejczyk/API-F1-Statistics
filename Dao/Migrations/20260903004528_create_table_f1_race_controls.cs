using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class create_table_f1_race_controls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "F1_RACE_CONTROLS",
                columns: table => new
                {
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MeetingKey = table.Column<int>(type: "int", nullable: false),
                    SessionKey = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DriverNumber = table.Column<int>(type: "int", nullable: true),
                    FlagType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LapNumber = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    QualifyingPhase = table.Column<int>(type: "int", nullable: true),
                    Scope = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Sector = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_F1_RACE_CONTROLS", x => new { x.Date, x.SessionKey, x.MeetingKey });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "F1_RACE_CONTROLS");
        }
    }
}
