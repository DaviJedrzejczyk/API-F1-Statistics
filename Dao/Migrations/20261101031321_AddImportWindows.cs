using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class AddImportWindows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "F1_IMPORT_WINDOWS",
                columns: table => new
                {
                    Dataset = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    WindowKey = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_F1_IMPORT_WINDOWS", x => new { x.Dataset, x.WindowKey });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "F1_IMPORT_WINDOWS");
        }
    }
}
