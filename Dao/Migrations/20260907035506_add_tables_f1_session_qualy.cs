using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class add_tables_f1_session_qualy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "F1_SESSION_RESULTS_QUALIFYINGS",
                columns: table => new
                {
                    SessionKey = table.Column<int>(type: "int", nullable: false),
                    MeetingKey = table.Column<int>(type: "int", nullable: false),
                    DriverNumber = table.Column<int>(type: "int", nullable: false),
                    QualifyingPhase = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Duration = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_F1_SESSION_RESULTS_QUALIFYINGS", x => new { x.MeetingKey, x.SessionKey, x.DriverNumber, x.QualifyingPhase });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "F1_SESSION_RESULTS_QUALIFYINGS");
        }
    }
}
