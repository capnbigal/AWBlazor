using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AWBlazorApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "maint");

            migrationBuilder.CreateTable(
                name: "AssetMaintenanceProfile",
                schema: "maint",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    Criticality = table.Column<byte>(type: "tinyint", nullable: false),
                    OwnerBusinessEntityId = table.Column<int>(type: "int", nullable: true),
                    TargetMtbfHours = table.Column<int>(type: "int", nullable: true),
                    NextPmDueAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetMaintenanceProfile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetMaintenanceProfile_Asset_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "org",
                        principalTable: "Asset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssetMaintenanceProfileAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetMaintenanceProfileId = table.Column<int>(type: "int", nullable: false),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    Criticality = table.Column<byte>(type: "tinyint", nullable: false),
                    OwnerBusinessEntityId = table.Column<int>(type: "int", nullable: true),
                    TargetMtbfHours = table.Column<int>(type: "int", nullable: true),
                    NextPmDueAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetMaintenanceProfileAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceWorkOrderAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaintenanceWorkOrderId = table.Column<int>(type: "int", nullable: false),
                    WorkOrderNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    Priority = table.Column<byte>(type: "tinyint", nullable: false),
                    PmScheduleId = table.Column<int>(type: "int", nullable: true),
                    ScheduledFor = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedBusinessEntityId = table.Column<int>(type: "int", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HeldAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RaisedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RaisedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedMeterValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceWorkOrderAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MeterReading",
                schema: "maint",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    Kind = table.Column<byte>(type: "tinyint", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecordedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeterReading", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeterReading_Asset_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "org",
                        principalTable: "Asset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PmSchedule",
                schema: "maint",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    IntervalKind = table.Column<byte>(type: "tinyint", nullable: false),
                    IntervalValue = table.Column<int>(type: "int", nullable: false),
                    DefaultPriority = table.Column<byte>(type: "tinyint", nullable: false),
                    EstimatedMinutes = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastCompletedMeterValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PmSchedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PmSchedule_Asset_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "org",
                        principalTable: "Asset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PmScheduleAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PmScheduleId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    IntervalKind = table.Column<byte>(type: "tinyint", nullable: false),
                    IntervalValue = table.Column<int>(type: "int", nullable: false),
                    DefaultPriority = table.Column<byte>(type: "tinyint", nullable: false),
                    EstimatedMinutes = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastCompletedMeterValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PmScheduleAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SparePart",
                schema: "maint",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    UnitMeasureCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    StandardCost = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ReorderPoint = table.Column<int>(type: "int", nullable: true),
                    ReorderQuantity = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SparePart", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SparePartAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SparePartId = table.Column<int>(type: "int", nullable: false),
                    PartNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    UnitMeasureCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    StandardCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReorderPoint = table.Column<int>(type: "int", nullable: true),
                    ReorderQuantity = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SparePartAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceWorkOrder",
                schema: "maint",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    Priority = table.Column<byte>(type: "tinyint", nullable: false),
                    PmScheduleId = table.Column<int>(type: "int", nullable: true),
                    ScheduledFor = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedBusinessEntityId = table.Column<int>(type: "int", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HeldAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RaisedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RaisedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedMeterValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceWorkOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceWorkOrder_Asset_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "org",
                        principalTable: "Asset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaintenanceWorkOrder_PmSchedule_PmScheduleId",
                        column: x => x.PmScheduleId,
                        principalSchema: "maint",
                        principalTable: "PmSchedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PmScheduleTask",
                schema: "maint",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PmScheduleId = table.Column<int>(type: "int", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    TaskName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    EstimatedMinutes = table.Column<int>(type: "int", nullable: true),
                    RequiresSignoff = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PmScheduleTask", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PmScheduleTask_PmSchedule_PmScheduleId",
                        column: x => x.PmScheduleId,
                        principalSchema: "maint",
                        principalTable: "PmSchedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceLog",
                schema: "maint",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    Kind = table.Column<byte>(type: "tinyint", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    AuthoredByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AuthoredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaintenanceWorkOrderId = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceLog_Asset_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "org",
                        principalTable: "Asset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaintenanceLog_MaintenanceWorkOrder_MaintenanceWorkOrderId",
                        column: x => x.MaintenanceWorkOrderId,
                        principalSchema: "maint",
                        principalTable: "MaintenanceWorkOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceWorkOrderTask",
                schema: "maint",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaintenanceWorkOrderId = table.Column<int>(type: "int", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    TaskName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    EstimatedMinutes = table.Column<int>(type: "int", nullable: true),
                    ActualMinutes = table.Column<int>(type: "int", nullable: true),
                    RequiresSignoff = table.Column<bool>(type: "bit", nullable: false),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    SignoffNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceWorkOrderTask", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceWorkOrderTask_MaintenanceWorkOrder_MaintenanceWorkOrderId",
                        column: x => x.MaintenanceWorkOrderId,
                        principalSchema: "maint",
                        principalTable: "MaintenanceWorkOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderPartUsage",
                schema: "maint",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaintenanceWorkOrderId = table.Column<int>(type: "int", nullable: false),
                    SparePartId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderPartUsage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderPartUsage_MaintenanceWorkOrder_MaintenanceWorkOrderId",
                        column: x => x.MaintenanceWorkOrderId,
                        principalSchema: "maint",
                        principalTable: "MaintenanceWorkOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrderPartUsage_SparePart_SparePartId",
                        column: x => x.SparePartId,
                        principalSchema: "maint",
                        principalTable: "SparePart",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetMaintenanceProfile_AssetId",
                schema: "maint",
                table: "AssetMaintenanceProfile",
                column: "AssetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetMaintenanceProfile_NextPmDueAt",
                schema: "maint",
                table: "AssetMaintenanceProfile",
                column: "NextPmDueAt");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMaintenanceProfileAuditLogs_AssetMaintenanceProfileId",
                table: "AssetMaintenanceProfileAuditLogs",
                column: "AssetMaintenanceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMaintenanceProfileAuditLogs_ChangedDate",
                table: "AssetMaintenanceProfileAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceLog_AssetId_AuthoredAt",
                schema: "maint",
                table: "MaintenanceLog",
                columns: new[] { "AssetId", "AuthoredAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceLog_MaintenanceWorkOrderId",
                schema: "maint",
                table: "MaintenanceLog",
                column: "MaintenanceWorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceWorkOrder_AssetId_Status",
                schema: "maint",
                table: "MaintenanceWorkOrder",
                columns: new[] { "AssetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceWorkOrder_PmScheduleId",
                schema: "maint",
                table: "MaintenanceWorkOrder",
                column: "PmScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceWorkOrder_RaisedAt",
                schema: "maint",
                table: "MaintenanceWorkOrder",
                column: "RaisedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceWorkOrder_ScheduledFor",
                schema: "maint",
                table: "MaintenanceWorkOrder",
                column: "ScheduledFor");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceWorkOrder_Status",
                schema: "maint",
                table: "MaintenanceWorkOrder",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceWorkOrder_WorkOrderNumber",
                schema: "maint",
                table: "MaintenanceWorkOrder",
                column: "WorkOrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceWorkOrderAuditLogs_ChangedDate",
                table: "MaintenanceWorkOrderAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceWorkOrderAuditLogs_MaintenanceWorkOrderId",
                table: "MaintenanceWorkOrderAuditLogs",
                column: "MaintenanceWorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceWorkOrderTask_MaintenanceWorkOrderId_SequenceNumber",
                schema: "maint",
                table: "MaintenanceWorkOrderTask",
                columns: new[] { "MaintenanceWorkOrderId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeterReading_AssetId_Kind_RecordedAt",
                schema: "maint",
                table: "MeterReading",
                columns: new[] { "AssetId", "Kind", "RecordedAt" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_PmSchedule_AssetId_IsActive",
                schema: "maint",
                table: "PmSchedule",
                columns: new[] { "AssetId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_PmSchedule_Code",
                schema: "maint",
                table: "PmSchedule",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PmScheduleAuditLogs_ChangedDate",
                table: "PmScheduleAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_PmScheduleAuditLogs_PmScheduleId",
                table: "PmScheduleAuditLogs",
                column: "PmScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_PmScheduleTask_PmScheduleId_SequenceNumber",
                schema: "maint",
                table: "PmScheduleTask",
                columns: new[] { "PmScheduleId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SparePart_IsActive",
                schema: "maint",
                table: "SparePart",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SparePart_PartNumber",
                schema: "maint",
                table: "SparePart",
                column: "PartNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SparePart_ProductId",
                schema: "maint",
                table: "SparePart",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SparePartAuditLogs_ChangedDate",
                table: "SparePartAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_SparePartAuditLogs_SparePartId",
                table: "SparePartAuditLogs",
                column: "SparePartId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderPartUsage_MaintenanceWorkOrderId",
                schema: "maint",
                table: "WorkOrderPartUsage",
                column: "MaintenanceWorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderPartUsage_SparePartId",
                schema: "maint",
                table: "WorkOrderPartUsage",
                column: "SparePartId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetMaintenanceProfile",
                schema: "maint");

            migrationBuilder.DropTable(
                name: "AssetMaintenanceProfileAuditLogs");

            migrationBuilder.DropTable(
                name: "MaintenanceLog",
                schema: "maint");

            migrationBuilder.DropTable(
                name: "MaintenanceWorkOrderAuditLogs");

            migrationBuilder.DropTable(
                name: "MaintenanceWorkOrderTask",
                schema: "maint");

            migrationBuilder.DropTable(
                name: "MeterReading",
                schema: "maint");

            migrationBuilder.DropTable(
                name: "PmScheduleAuditLogs");

            migrationBuilder.DropTable(
                name: "PmScheduleTask",
                schema: "maint");

            migrationBuilder.DropTable(
                name: "SparePartAuditLogs");

            migrationBuilder.DropTable(
                name: "WorkOrderPartUsage",
                schema: "maint");

            migrationBuilder.DropTable(
                name: "MaintenanceWorkOrder",
                schema: "maint");

            migrationBuilder.DropTable(
                name: "SparePart",
                schema: "maint");

            migrationBuilder.DropTable(
                name: "PmSchedule",
                schema: "maint");
        }
    }
}
