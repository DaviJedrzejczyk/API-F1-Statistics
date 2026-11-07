using Dao;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dao.Migrations;

[DbContext(typeof(ApiF1DB))]
[Migration("20261102000000_RaceControlEventIdentity")]
public class RaceControlEventIdentity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey("PK_F1_RACE_CONTROLS", "F1_RACE_CONTROLS");
        migrationBuilder.AddColumn<long>(name: "Id", table: "F1_RACE_CONTROLS", type: "bigint", nullable: false)
            .Annotation("SqlServer:Identity", "1, 1");
        migrationBuilder.AddPrimaryKey("PK_F1_RACE_CONTROLS", "F1_RACE_CONTROLS", "Id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey("PK_F1_RACE_CONTROLS", "F1_RACE_CONTROLS");
        migrationBuilder.DropColumn("Id", "F1_RACE_CONTROLS");
        migrationBuilder.AddPrimaryKey("PK_F1_RACE_CONTROLS", "F1_RACE_CONTROLS", new[] { "Date", "SessionKey", "MeetingKey" });
    }
}
