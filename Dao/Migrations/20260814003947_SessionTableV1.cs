using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class SessionTableV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "F1_SESSION",
                columns: table => new
                {
                    SessionKey = table.Column<int>(type: "int", nullable: false),
                    CircuitKey = table.Column<int>(type: "int", nullable: false),
                    CircuitShortName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CountryCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CountryKey = table.Column<int>(type: "int", nullable: false),
                    CountryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateStart = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    GmtOffset = table.Column<TimeSpan>(type: "time", nullable: false),
                    IsCancelled = table.Column<bool>(type: "bit", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MeetingKey = table.Column<int>(type: "int", nullable: false),
                    SessionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SessionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_F1_SESSION", x => x.SessionKey);
                });

            migrationBuilder.CreateIndex(
                name: "IX_F1_SESSION_CircuitKey",
                table: "F1_SESSION",
                column: "CircuitKey");

            migrationBuilder.CreateIndex(
                name: "IX_F1_SESSION_MeetingKey",
                table: "F1_SESSION",
                column: "MeetingKey");

            migrationBuilder.CreateIndex(
                name: "IX_F1_SESSION_Year",
                table: "F1_SESSION",
                column: "Year");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "F1_SESSION");
        }
    }
}
