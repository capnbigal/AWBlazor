using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AWBlazorApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStationIdealCycleSeconds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "IdealCycleSeconds",
                table: "StationAuditLogs",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "IdealCycleSeconds",
                schema: "org",
                table: "Station",
                type: "decimal(10,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdealCycleSeconds",
                table: "StationAuditLogs");

            migrationBuilder.DropColumn(
                name: "IdealCycleSeconds",
                schema: "org",
                table: "Station");
        }
    }
}
