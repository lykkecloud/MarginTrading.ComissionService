using Microsoft.EntityFrameworkCore.Migrations;

namespace MarginTrading.CommissionService.SqlRepositories.Migrations
{
    public partial class KidScenarioOptional : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "KidModerateScenarioAvreturn",
                schema: "commission",
                table: "KidScenarios",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "KidModerateScenario",
                schema: "commission",
                table: "KidScenarios",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "KidModerateScenarioAvreturn",
                schema: "commission",
                table: "KidScenarios",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "KidModerateScenario",
                schema: "commission",
                table: "KidScenarios",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);
        }
    }
}
