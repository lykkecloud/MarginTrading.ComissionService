using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MarginTrading.CommissionService.SqlRepositories.Migrations
{
    public partial class KidScenarios : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "commission");

            migrationBuilder.CreateTable(
                name: "KidScenarios",
                schema: "commission",
                columns: table => new
                {
                    Isin = table.Column<string>(maxLength: 400, nullable: false),
                    KidModerateScenario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KidModerateScenarioAvreturn = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KidScenarios", x => x.Isin);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KidScenarios",
                schema: "commission");
        }
    }
}
