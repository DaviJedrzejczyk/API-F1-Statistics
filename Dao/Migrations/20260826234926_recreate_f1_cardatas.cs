using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class recreate_f1_cardatas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "F1_CARDATAS",
                columns: table => new
                {
                    SessionKey = table.Column<int>(type: "int", nullable: false),
                    DriverNumber = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Brake = table.Column<int>(type: "int", nullable: false),
                    Drs = table.Column<int>(type: "int", nullable: true),
                    MeetingKey = table.Column<int>(type: "int", nullable: false),
                    Gear = table.Column<int>(type: "int", nullable: false),
                    Rpm = table.Column<int>(type: "int", nullable: false),
                    Speed = table.Column<int>(type: "int", nullable: false),
                    Throttle = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_F1_CARDATAS", x => new { x.SessionKey, x.DriverNumber, x.Date });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "F1_CARDATAS");
        }
    }
}
