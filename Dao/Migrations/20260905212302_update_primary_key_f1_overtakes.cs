using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class update_primary_key_f1_overtakes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_F1_OVERTAKES",
                table: "F1_OVERTAKES");

            migrationBuilder.AddPrimaryKey(
                name: "PK_F1_OVERTAKES",
                table: "F1_OVERTAKES",
                columns: new[] { "MeetingKey", "SessionKey", "OvertakingDriverNumber", "OvertakedDriverNumber", "Date", "Position" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_F1_OVERTAKES",
                table: "F1_OVERTAKES");

            migrationBuilder.AddPrimaryKey(
                name: "PK_F1_OVERTAKES",
                table: "F1_OVERTAKES",
                columns: new[] { "MeetingKey", "SessionKey" });
        }
    }
}
