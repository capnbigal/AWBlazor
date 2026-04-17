using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AWBlazorApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQualityModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkInstruction_WorkInstructionRevision_ActiveRevisionId",
                schema: "mes",
                table: "WorkInstruction");

            migrationBuilder.EnsureSchema(
                name: "qa");

            migrationBuilder.CreateTable(
                name: "CapaCase",
                schema: "qa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    RootCause = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CorrectiveAction = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PreventiveAction = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    VerificationNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    OwnerBusinessEntityId = table.Column<int>(type: "int", nullable: true),
                    OpenedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapaCase", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CapaCaseAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CapaCaseId = table.Column<int>(type: "int", nullable: false),
                    CaseNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    RootCause = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CorrectiveAction = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PreventiveAction = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    VerificationNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    OwnerBusinessEntityId = table.Column<int>(type: "int", nullable: true),
                    OpenedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapaCaseAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InspectionAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectionId = table.Column<int>(type: "int", nullable: false),
                    InspectionNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    InspectionPlanId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    SourceKind = table.Column<byte>(type: "tinyint", nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    InspectorBusinessEntityId = table.Column<int>(type: "int", nullable: true),
                    InspectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InventoryItemId = table.Column<int>(type: "int", nullable: true),
                    LotId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitMeasureCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PostedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InspectionPlan",
                schema: "qa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Scope = table.Column<byte>(type: "tinyint", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    WorkOrderRoutingId = table.Column<int>(type: "int", nullable: true),
                    VendorBusinessEntityId = table.Column<int>(type: "int", nullable: true),
                    SamplingRule = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AutoTriggerOnReceipt = table.Column<bool>(type: "bit", nullable: false),
                    AutoTriggerOnShipment = table.Column<bool>(type: "bit", nullable: false),
                    AutoTriggerOnProductionRun = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionPlan", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InspectionPlanAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectionPlanId = table.Column<int>(type: "int", nullable: false),
                    PlanCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Scope = table.Column<byte>(type: "tinyint", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    WorkOrderRoutingId = table.Column<int>(type: "int", nullable: true),
                    VendorBusinessEntityId = table.Column<int>(type: "int", nullable: true),
                    SamplingRule = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AutoTriggerOnReceipt = table.Column<bool>(type: "bit", nullable: false),
                    AutoTriggerOnShipment = table.Column<bool>(type: "bit", nullable: false),
                    AutoTriggerOnProductionRun = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionPlanAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InspectionPlanCharacteristicAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectionPlanCharacteristicId = table.Column<int>(type: "int", nullable: false),
                    InspectionPlanId = table.Column<int>(type: "int", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Kind = table.Column<byte>(type: "tinyint", nullable: false),
                    MinValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TargetValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UnitMeasureCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    ExpectedValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsCritical = table.Column<bool>(type: "bit", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionPlanCharacteristicAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NonConformanceActionAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NonConformanceActionId = table.Column<int>(type: "int", nullable: false),
                    NonConformanceId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PerformedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NonConformanceActionAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NonConformanceAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NonConformanceId = table.Column<int>(type: "int", nullable: false),
                    NcrNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    InspectionId = table.Column<int>(type: "int", nullable: true),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    LotId = table.Column<int>(type: "int", nullable: true),
                    LocationId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitMeasureCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    Disposition = table.Column<byte>(type: "tinyint", nullable: true),
                    DispositionedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    DispositionedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DispositionNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PostedTransactionId = table.Column<long>(type: "bigint", nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NonConformanceAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Inspection",
                schema: "qa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectionNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    InspectionPlanId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    SourceKind = table.Column<byte>(type: "tinyint", nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    InspectorBusinessEntityId = table.Column<int>(type: "int", nullable: true),
                    InspectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InventoryItemId = table.Column<int>(type: "int", nullable: true),
                    LotId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitMeasureCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PostedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inspection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inspection_InspectionPlan_InspectionPlanId",
                        column: x => x.InspectionPlanId,
                        principalSchema: "qa",
                        principalTable: "InspectionPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inspection_InventoryItem_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalSchema: "inv",
                        principalTable: "InventoryItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Inspection_Lot_LotId",
                        column: x => x.LotId,
                        principalSchema: "inv",
                        principalTable: "Lot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "InspectionPlanCharacteristic",
                schema: "qa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectionPlanId = table.Column<int>(type: "int", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Kind = table.Column<byte>(type: "tinyint", nullable: false),
                    MinValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    MaxValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    TargetValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    UnitMeasureCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    ExpectedValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsCritical = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionPlanCharacteristic", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionPlanCharacteristic_InspectionPlan_InspectionPlanId",
                        column: x => x.InspectionPlanId,
                        principalSchema: "qa",
                        principalTable: "InspectionPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NonConformance",
                schema: "qa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NcrNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    InspectionId = table.Column<int>(type: "int", nullable: true),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    LotId = table.Column<int>(type: "int", nullable: true),
                    LocationId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitMeasureCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    Disposition = table.Column<byte>(type: "tinyint", nullable: true),
                    DispositionedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    DispositionedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DispositionNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PostedTransactionId = table.Column<long>(type: "bigint", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NonConformance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NonConformance_Inspection_InspectionId",
                        column: x => x.InspectionId,
                        principalSchema: "qa",
                        principalTable: "Inspection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_NonConformance_InventoryItem_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalSchema: "inv",
                        principalTable: "InventoryItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NonConformance_InventoryLocation_LocationId",
                        column: x => x.LocationId,
                        principalSchema: "inv",
                        principalTable: "InventoryLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_NonConformance_InventoryTransaction_PostedTransactionId",
                        column: x => x.PostedTransactionId,
                        principalSchema: "inv",
                        principalTable: "InventoryTransaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NonConformance_Lot_LotId",
                        column: x => x.LotId,
                        principalSchema: "inv",
                        principalTable: "Lot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "InspectionResult",
                schema: "qa",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectionId = table.Column<int>(type: "int", nullable: false),
                    InspectionPlanCharacteristicId = table.Column<int>(type: "int", nullable: false),
                    NumericResult = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    AttributeResult = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Passed = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecordedByBusinessEntityId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionResult_InspectionPlanCharacteristic_InspectionPlanCharacteristicId",
                        column: x => x.InspectionPlanCharacteristicId,
                        principalSchema: "qa",
                        principalTable: "InspectionPlanCharacteristic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionResult_Inspection_InspectionId",
                        column: x => x.InspectionId,
                        principalSchema: "qa",
                        principalTable: "Inspection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CapaCaseNonConformance",
                schema: "qa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CapaCaseId = table.Column<int>(type: "int", nullable: false),
                    NonConformanceId = table.Column<int>(type: "int", nullable: false),
                    LinkedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapaCaseNonConformance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CapaCaseNonConformance_CapaCase_CapaCaseId",
                        column: x => x.CapaCaseId,
                        principalSchema: "qa",
                        principalTable: "CapaCase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CapaCaseNonConformance_NonConformance_NonConformanceId",
                        column: x => x.NonConformanceId,
                        principalSchema: "qa",
                        principalTable: "NonConformance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NonConformanceAction",
                schema: "qa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NonConformanceId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PerformedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NonConformanceAction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NonConformanceAction_NonConformance_NonConformanceId",
                        column: x => x.NonConformanceId,
                        principalSchema: "qa",
                        principalTable: "NonConformance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CapaCase_CaseNumber",
                schema: "qa",
                table: "CapaCase",
                column: "CaseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CapaCase_OpenedAt",
                schema: "qa",
                table: "CapaCase",
                column: "OpenedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_CapaCase_Status",
                schema: "qa",
                table: "CapaCase",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CapaCaseAuditLogs_CapaCaseId",
                table: "CapaCaseAuditLogs",
                column: "CapaCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CapaCaseAuditLogs_ChangedDate",
                table: "CapaCaseAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_CapaCaseNonConformance_CapaCaseId_NonConformanceId",
                schema: "qa",
                table: "CapaCaseNonConformance",
                columns: new[] { "CapaCaseId", "NonConformanceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CapaCaseNonConformance_NonConformanceId",
                schema: "qa",
                table: "CapaCaseNonConformance",
                column: "NonConformanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspection_InspectionNumber",
                schema: "qa",
                table: "Inspection",
                column: "InspectionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inspection_InspectionPlanId",
                schema: "qa",
                table: "Inspection",
                column: "InspectionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspection_InventoryItemId",
                schema: "qa",
                table: "Inspection",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspection_LotId",
                schema: "qa",
                table: "Inspection",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspection_SourceKind_SourceId",
                schema: "qa",
                table: "Inspection",
                columns: new[] { "SourceKind", "SourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_Inspection_Status",
                schema: "qa",
                table: "Inspection",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionAuditLogs_ChangedDate",
                table: "InspectionAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionAuditLogs_InspectionId",
                table: "InspectionAuditLogs",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionPlan_PlanCode",
                schema: "qa",
                table: "InspectionPlan",
                column: "PlanCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InspectionPlan_ProductId",
                schema: "qa",
                table: "InspectionPlan",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionPlan_Scope",
                schema: "qa",
                table: "InspectionPlan",
                column: "Scope");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionPlan_VendorBusinessEntityId",
                schema: "qa",
                table: "InspectionPlan",
                column: "VendorBusinessEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionPlanAuditLogs_ChangedDate",
                table: "InspectionPlanAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionPlanAuditLogs_InspectionPlanId",
                table: "InspectionPlanAuditLogs",
                column: "InspectionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionPlanCharacteristic_InspectionPlanId_SequenceNumber",
                schema: "qa",
                table: "InspectionPlanCharacteristic",
                columns: new[] { "InspectionPlanId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InspectionPlanCharacteristicAuditLogs_ChangedDate",
                table: "InspectionPlanCharacteristicAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionPlanCharacteristicAuditLogs_InspectionPlanCharacteristicId",
                table: "InspectionPlanCharacteristicAuditLogs",
                column: "InspectionPlanCharacteristicId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionResult_InspectionId",
                schema: "qa",
                table: "InspectionResult",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionResult_InspectionPlanCharacteristicId",
                schema: "qa",
                table: "InspectionResult",
                column: "InspectionPlanCharacteristicId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionResult_RecordedAt",
                schema: "qa",
                table: "InspectionResult",
                column: "RecordedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_NonConformance_InspectionId",
                schema: "qa",
                table: "NonConformance",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_NonConformance_InventoryItemId",
                schema: "qa",
                table: "NonConformance",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_NonConformance_LocationId",
                schema: "qa",
                table: "NonConformance",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_NonConformance_LotId",
                schema: "qa",
                table: "NonConformance",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_NonConformance_NcrNumber",
                schema: "qa",
                table: "NonConformance",
                column: "NcrNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NonConformance_PostedTransactionId",
                schema: "qa",
                table: "NonConformance",
                column: "PostedTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_NonConformance_Status",
                schema: "qa",
                table: "NonConformance",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_NonConformanceAction_NonConformanceId",
                schema: "qa",
                table: "NonConformanceAction",
                column: "NonConformanceId");

            migrationBuilder.CreateIndex(
                name: "IX_NonConformanceAction_PerformedAt",
                schema: "qa",
                table: "NonConformanceAction",
                column: "PerformedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_NonConformanceActionAuditLogs_ChangedDate",
                table: "NonConformanceActionAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_NonConformanceActionAuditLogs_NonConformanceActionId",
                table: "NonConformanceActionAuditLogs",
                column: "NonConformanceActionId");

            migrationBuilder.CreateIndex(
                name: "IX_NonConformanceAuditLogs_ChangedDate",
                table: "NonConformanceAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_NonConformanceAuditLogs_NonConformanceId",
                table: "NonConformanceAuditLogs",
                column: "NonConformanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkInstruction_WorkInstructionRevision_ActiveRevisionId",
                schema: "mes",
                table: "WorkInstruction",
                column: "ActiveRevisionId",
                principalSchema: "mes",
                principalTable: "WorkInstructionRevision",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkInstruction_WorkInstructionRevision_ActiveRevisionId",
                schema: "mes",
                table: "WorkInstruction");

            migrationBuilder.DropTable(
                name: "CapaCaseAuditLogs");

            migrationBuilder.DropTable(
                name: "CapaCaseNonConformance",
                schema: "qa");

            migrationBuilder.DropTable(
                name: "InspectionAuditLogs");

            migrationBuilder.DropTable(
                name: "InspectionPlanAuditLogs");

            migrationBuilder.DropTable(
                name: "InspectionPlanCharacteristicAuditLogs");

            migrationBuilder.DropTable(
                name: "InspectionResult",
                schema: "qa");

            migrationBuilder.DropTable(
                name: "NonConformanceAction",
                schema: "qa");

            migrationBuilder.DropTable(
                name: "NonConformanceActionAuditLogs");

            migrationBuilder.DropTable(
                name: "NonConformanceAuditLogs");

            migrationBuilder.DropTable(
                name: "CapaCase",
                schema: "qa");

            migrationBuilder.DropTable(
                name: "InspectionPlanCharacteristic",
                schema: "qa");

            migrationBuilder.DropTable(
                name: "NonConformance",
                schema: "qa");

            migrationBuilder.DropTable(
                name: "Inspection",
                schema: "qa");

            migrationBuilder.DropTable(
                name: "InspectionPlan",
                schema: "qa");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkInstruction_WorkInstructionRevision_ActiveRevisionId",
                schema: "mes",
                table: "WorkInstruction",
                column: "ActiveRevisionId",
                principalSchema: "mes",
                principalTable: "WorkInstructionRevision",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
