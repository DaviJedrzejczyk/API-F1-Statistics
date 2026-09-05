using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class update_column_flag_type_to_flag_f1_race_controls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlagType",
                table: "F1_RACE_CONTROLS");

            migrationBuilder.AddColumn<string>(
                name: "Flag",
                table: "F1_RACE_CONTROLS",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Flag",
                table: "F1_RACE_CONTROLS");

            migrationBuilder.AddColumn<string>(
                name: "FlagType",
                table: "F1_RACE_CONTROLS",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
