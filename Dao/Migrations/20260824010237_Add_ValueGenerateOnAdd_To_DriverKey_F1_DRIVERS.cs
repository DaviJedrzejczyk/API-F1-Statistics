using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class Add_ValueGenerateOnAdd_To_DriverKey_F1_DRIVERS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_F1_DRIVERS_DriverKey",
                table: "F1_DRIVERS",
                column: "DriverKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_F1_DRIVERS_DriverKey",
                table: "F1_DRIVERS");
        }
    }
}
