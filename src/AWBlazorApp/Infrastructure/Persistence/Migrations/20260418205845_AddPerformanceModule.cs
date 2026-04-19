using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AWBlazorApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "perf");

            migrationBuilder.CreateTable(
                name: "KpiDefinition",
                schema: "perf",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Source = table.Column<byte>(type: "tinyint", nullable: false),
                    Aggregation = table.Column<byte>(type: "tinyint", nullable: false),
                    TargetValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    WarningThreshold = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    CriticalThreshold = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Direction = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpiDefinition", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KpiDefinitionAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KpiDefinitionId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Source = table.Column<byte>(type: "tinyint", nullable: false),
                    Aggregation = table.Column<byte>(type: "tinyint", nullable: false),
                    TargetValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    WarningThreshold = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CriticalThreshold = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Direction = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpiDefinitionAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceMonthlyMetric",
                schema: "perf",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    WorkOrderCount = table.Column<int>(type: "int", nullable: false),
                    BreakdownCount = table.Column<int>(type: "int", nullable: false),
                    PmWorkOrderCount = table.Column<int>(type: "int", nullable: false),
                    PmCompletedCount = table.Column<int>(type: "int", nullable: false),
                    MtbfHours = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    MttrHours = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    AvailabilityFraction = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    PmComplianceFraction = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    ComputedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceMonthlyMetric", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceMonthlyMetric_Asset_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "org",
                        principalTable: "Asset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OeeSnapshot",
                schema: "perf",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    PeriodKind = table.Column<byte>(type: "tinyint", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlannedRuntimeMinutes = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ActualRuntimeMinutes = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DowntimeMinutes = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitsProduced = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitsScrapped = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IdealCycleSeconds = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Availability = table.Column<decimal>(type: "decimal(5,4)", nullable: false),
                    Performance = table.Column<decimal>(type: "decimal(5,4)", nullable: false),
                    Quality = table.Column<decimal>(type: "decimal(5,4)", nullable: false),
                    Oee = table.Column<decimal>(type: "decimal(5,4)", nullable: false),
                    ComputedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OeeSnapshot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OeeSnapshot_Station_StationId",
                        column: x => x.StationId,
                        principalSchema: "org",
                        principalTable: "Station",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerformanceReport",
                schema: "perf",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Kind = table.Column<byte>(type: "tinyint", nullable: false),
                    DefinitionJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastRunAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceReport", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PerformanceReportAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerformanceReportId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Kind = table.Column<byte>(type: "tinyint", nullable: false),
                    DefinitionJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastRunAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceReportAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductionDailyMetric",
                schema: "perf",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    UnitsProduced = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitsScrapped = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    AverageCycleSeconds = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    YieldFraction = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    RunCount = table.Column<int>(type: "int", nullable: false),
                    ComputedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionDailyMetric", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionDailyMetric_Station_StationId",
                        column: x => x.StationId,
                        principalSchema: "org",
                        principalTable: "Station",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScorecardDefinition",
                schema: "perf",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    OwnerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScorecardDefinition", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScorecardDefinitionAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScorecardDefinitionId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    OwnerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScorecardDefinitionAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KpiValue",
                schema: "perf",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KpiDefinitionId = table.Column<int>(type: "int", nullable: false),
                    PeriodKind = table.Column<byte>(type: "tinyint", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    TargetAtPeriod = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    ComputedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpiValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KpiValue_KpiDefinition_KpiDefinitionId",
                        column: x => x.KpiDefinitionId,
                        principalSchema: "perf",
                        principalTable: "KpiDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerformanceReportRun",
                schema: "perf",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerformanceReportId = table.Column<int>(type: "int", nullable: false),
                    RunAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RunByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RowCount = table.Column<int>(type: "int", nullable: false),
                    DurationMs = table.Column<int>(type: "int", nullable: false),
                    ResultJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceReportRun", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerformanceReportRun_PerformanceReport_PerformanceReportId",
                        column: x => x.PerformanceReportId,
                        principalSchema: "perf",
                        principalTable: "PerformanceReport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScorecardKpi",
                schema: "perf",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScorecardDefinitionId = table.Column<int>(type: "int", nullable: false),
                    KpiDefinitionId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Visual = table.Column<byte>(type: "tinyint", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScorecardKpi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScorecardKpi_KpiDefinition_KpiDefinitionId",
                        column: x => x.KpiDefinitionId,
                        principalSchema: "perf",
                        principalTable: "KpiDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScorecardKpi_ScorecardDefinition_ScorecardDefinitionId",
                        column: x => x.ScorecardDefinitionId,
                        principalSchema: "perf",
                        principalTable: "ScorecardDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KpiDefinition_Code",
                schema: "perf",
                table: "KpiDefinition",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KpiDefinition_IsActive",
                schema: "perf",
                table: "KpiDefinition",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_KpiDefinitionAuditLogs_ChangedDate",
                table: "KpiDefinitionAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_KpiDefinitionAuditLogs_KpiDefinitionId",
                table: "KpiDefinitionAuditLogs",
                column: "KpiDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_KpiValue_KpiDefinitionId_PeriodKind_PeriodStart",
                schema: "perf",
                table: "KpiValue",
                columns: new[] { "KpiDefinitionId", "PeriodKind", "PeriodStart" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KpiValue_PeriodStart",
                schema: "perf",
                table: "KpiValue",
                column: "PeriodStart",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceMonthlyMetric_AssetId_Year_Month",
                schema: "perf",
                table: "MaintenanceMonthlyMetric",
                columns: new[] { "AssetId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OeeSnapshot_PeriodStart",
                schema: "perf",
                table: "OeeSnapshot",
                column: "PeriodStart",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_OeeSnapshot_StationId_PeriodKind_PeriodStart",
                schema: "perf",
                table: "OeeSnapshot",
                columns: new[] { "StationId", "PeriodKind", "PeriodStart" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReport_Code",
                schema: "perf",
                table: "PerformanceReport",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReport_IsActive",
                schema: "perf",
                table: "PerformanceReport",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReportAuditLogs_ChangedDate",
                table: "PerformanceReportAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReportAuditLogs_PerformanceReportId",
                table: "PerformanceReportAuditLogs",
                column: "PerformanceReportId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReportRun_PerformanceReportId",
                schema: "perf",
                table: "PerformanceReportRun",
                column: "PerformanceReportId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReportRun_RunAt",
                schema: "perf",
                table: "PerformanceReportRun",
                column: "RunAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionDailyMetric_Date",
                schema: "perf",
                table: "ProductionDailyMetric",
                column: "Date",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionDailyMetric_StationId_Date",
                schema: "perf",
                table: "ProductionDailyMetric",
                columns: new[] { "StationId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScorecardDefinition_Code",
                schema: "perf",
                table: "ScorecardDefinition",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScorecardDefinition_IsActive",
                schema: "perf",
                table: "ScorecardDefinition",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ScorecardDefinitionAuditLogs_ChangedDate",
                table: "ScorecardDefinitionAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ScorecardDefinitionAuditLogs_ScorecardDefinitionId",
                table: "ScorecardDefinitionAuditLogs",
                column: "ScorecardDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ScorecardKpi_KpiDefinitionId",
                schema: "perf",
                table: "ScorecardKpi",
                column: "KpiDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ScorecardKpi_ScorecardDefinitionId_DisplayOrder",
                schema: "perf",
                table: "ScorecardKpi",
                columns: new[] { "ScorecardDefinitionId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ScorecardKpi_ScorecardDefinitionId_KpiDefinitionId",
                schema: "perf",
                table: "ScorecardKpi",
                columns: new[] { "ScorecardDefinitionId", "KpiDefinitionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KpiDefinitionAuditLogs");

            migrationBuilder.DropTable(
                name: "KpiValue",
                schema: "perf");

            migrationBuilder.DropTable(
                name: "MaintenanceMonthlyMetric",
                schema: "perf");

            migrationBuilder.DropTable(
                name: "OeeSnapshot",
                schema: "perf");

            migrationBuilder.DropTable(
                name: "PerformanceReportAuditLogs");

            migrationBuilder.DropTable(
                name: "PerformanceReportRun",
                schema: "perf");

            migrationBuilder.DropTable(
                name: "ProductionDailyMetric",
                schema: "perf");

            migrationBuilder.DropTable(
                name: "ScorecardDefinitionAuditLogs");

            migrationBuilder.DropTable(
                name: "ScorecardKpi",
                schema: "perf");

            migrationBuilder.DropTable(
                name: "PerformanceReport",
                schema: "perf");

            migrationBuilder.DropTable(
                name: "KpiDefinition",
                schema: "perf");

            migrationBuilder.DropTable(
                name: "ScorecardDefinition",
                schema: "perf");
        }
    }
}
