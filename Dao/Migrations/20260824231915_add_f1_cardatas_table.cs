using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class add_f1_cardatas_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "F1_CARDATAS",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Brake = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DriveNumber = table.Column<int>(type: "int", nullable: false),
                    Drs = table.Column<int>(type: "int", nullable: false),
                    MeetingKey = table.Column<int>(type: "int", nullable: false),
                    Gear = table.Column<int>(type: "int", nullable: false),
                    Rpm = table.Column<int>(type: "int", nullable: false),
                    SessionKey = table.Column<int>(type: "int", nullable: false),
                    Speed = table.Column<int>(type: "int", nullable: false),
                    Throttle = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_F1_CARDATAS", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_F1_CARDATAS_DriveNumber",
                table: "F1_CARDATAS",
                column: "DriveNumber");

            migrationBuilder.CreateIndex(
                name: "IX_F1_CARDATAS_Id",
                table: "F1_CARDATAS",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_F1_CARDATAS_MeetingKey",
                table: "F1_CARDATAS",
                column: "MeetingKey");

            migrationBuilder.CreateIndex(
                name: "IX_F1_CARDATAS_SessionKey",
                table: "F1_CARDATAS",
                column: "SessionKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "F1_CARDATAS");
        }
    }
}
