using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class add_table_f1_stints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "F1_STINTS",
                columns: table => new
                {
                    DriverNumber = table.Column<int>(type: "int", nullable: false),
                    MeetingKey = table.Column<int>(type: "int", nullable: false),
                    SessionKey = table.Column<int>(type: "int", nullable: false),
                    StintNumber = table.Column<int>(type: "int", nullable: false),
                    Compound = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LapEnd = table.Column<int>(type: "int", nullable: true),
                    LapStart = table.Column<int>(type: "int", nullable: true),
                    TyreAgeAtStart = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_F1_STINTS", x => new { x.MeetingKey, x.SessionKey, x.DriverNumber, x.StintNumber });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "F1_STINTS");
        }
    }
}
