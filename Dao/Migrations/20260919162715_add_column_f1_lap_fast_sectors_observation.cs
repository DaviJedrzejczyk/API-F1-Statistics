using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class add_column_f1_lap_fast_sectors_observation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Observation",
                table: "F1_LAPS_FAST_SECTORS",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Observation",
                table: "F1_LAPS_FAST_SECTORS");
        }
    }
}
