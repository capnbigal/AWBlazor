using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AWBlazorApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceReportStructuredFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssetId",
                table: "PerformanceReportAuditLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "RangePreset",
                table: "PerformanceReportAuditLogs",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)1); // Last7Days — matches the entity-side default so existing rows get a sane preset.

            migrationBuilder.AddColumn<int>(
                name: "StationId",
                table: "PerformanceReportAuditLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssetId",
                schema: "perf",
                table: "PerformanceReport",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "RangePreset",
                schema: "perf",
                table: "PerformanceReport",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)1); // Last7Days — matches the entity-side default so existing rows get a sane preset.

            migrationBuilder.AddColumn<int>(
                name: "StationId",
                schema: "perf",
                table: "PerformanceReport",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssetId",
                table: "PerformanceReportAuditLogs");

            migrationBuilder.DropColumn(
                name: "RangePreset",
                table: "PerformanceReportAuditLogs");

            migrationBuilder.DropColumn(
                name: "StationId",
                table: "PerformanceReportAuditLogs");

            migrationBuilder.DropColumn(
                name: "AssetId",
                schema: "perf",
                table: "PerformanceReport");

            migrationBuilder.DropColumn(
                name: "RangePreset",
                schema: "perf",
                table: "PerformanceReport");

            migrationBuilder.DropColumn(
                name: "StationId",
                schema: "perf",
                table: "PerformanceReport");
        }
    }
}
