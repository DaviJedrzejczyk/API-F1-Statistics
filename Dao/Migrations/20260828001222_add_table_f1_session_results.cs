using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class add_table_f1_session_results : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "F1_SESSION_RESULTS",
                columns: table => new
                {
                    DriverNumber = table.Column<int>(type: "int", nullable: false),
                    SessionKey = table.Column<int>(type: "int", nullable: false),
                    Dnf = table.Column<bool>(type: "bit", nullable: false),
                    Dns = table.Column<bool>(type: "bit", nullable: false),
                    Dsq = table.Column<bool>(type: "bit", nullable: false),
                    Duration = table.Column<double>(type: "float", nullable: false),
                    GapToLeader = table.Column<double>(type: "float", nullable: false),
                    NumberOfLaps = table.Column<int>(type: "int", nullable: false),
                    MeetingKey = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_F1_SESSION_RESULTS", x => new { x.SessionKey, x.DriverNumber });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "F1_SESSION_RESULTS");
        }
    }
}
