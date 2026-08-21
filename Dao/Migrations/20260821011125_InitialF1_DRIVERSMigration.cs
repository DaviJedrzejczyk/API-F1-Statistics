using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class InitialF1_DRIVERSMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "F1_DRIVERS",
                columns: table => new
                {
                    DriverKey = table.Column<int>(type: "int", nullable: false),
                    DriveNumber = table.Column<int>(type: "int", nullable: false),
                    BroadcastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HeadshotUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MeetingKey = table.Column<int>(type: "int", nullable: false),
                    NameAcronym = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    SessionKey = table.Column<int>(type: "int", nullable: false),
                    TeamColour = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TeamName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_F1_DRIVERS", x => x.DriverKey);
                });

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
