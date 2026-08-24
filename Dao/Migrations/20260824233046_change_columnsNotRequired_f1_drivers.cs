using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class change_columnsNotRequired_f1_drivers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "F1_DRIVERS",
                columns: table => new
                {
                    DriverKey = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DriverNumber = table.Column<int>(type: "int", nullable: false),
                    BroadcastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HeadshotUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MeetingKey = table.Column<int>(type: "int", nullable: false),
                    NameAcronym = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    SessionKey = table.Column<int>(type: "int", nullable: false),
                    TeamColour = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TeamName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_F1_DRIVERS", x => x.DriverKey);
                });

            migrationBuilder.CreateIndex(
                name: "IX_F1_DRIVERS_DriverKey",
                table: "F1_DRIVERS",
                column: "DriverKey");

            migrationBuilder.CreateIndex(
                name: "IX_F1_DRIVERS_MeetingKey",
                table: "F1_DRIVERS",
                column: "MeetingKey");

            migrationBuilder.CreateIndex(
                name: "IX_F1_DRIVERS_SessionKey",
                table: "F1_DRIVERS",
                column: "SessionKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "F1_DRIVERS");
        }
    }
}
