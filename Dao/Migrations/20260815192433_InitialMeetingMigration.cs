using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class InitialMeetingMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "F1_MEETINGS",
                columns: table => new
                {
                    MeetingKey = table.Column<int>(type: "int", nullable: false),
                    CircuitKey = table.Column<int>(type: "int", nullable: false),
                    CircuitInfoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CircuitImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CircuitShortName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CircuitType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CountryCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CountryFlag = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CountryKey = table.Column<int>(type: "int", nullable: false),
                    CountryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateStart = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    GmtOffset = table.Column<TimeSpan>(type: "time", nullable: false),
                    IsCancelled = table.Column<bool>(type: "bit", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MeetingName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MeetingOfficialName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_F1_MEETINGS", x => x.MeetingKey);
                });

            migrationBuilder.CreateIndex(
                name: "IX_F1_MEETINGS_CircuitKey",
                table: "F1_MEETINGS",
                column: "CircuitKey");

            migrationBuilder.CreateIndex(
                name: "IX_F1_MEETINGS_Year",
                table: "F1_MEETINGS",
                column: "Year");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "F1_MEETINGS");
        }
    }
}
