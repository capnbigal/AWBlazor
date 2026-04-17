using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AWBlazorApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvancedInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "inv");

            migrationBuilder.CreateTable(
                name: "InventoryAdjustmentAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryAdjustmentId = table.Column<int>(type: "int", nullable: false),
                    AdjustmentNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    LotId = table.Column<int>(type: "int", nullable: true),
                    QuantityDelta = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReasonCode = table.Column<byte>(type: "tinyint", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    RequestedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PostedTransactionId = table.Column<long>(type: "bigint", nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryAdjustmentAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryItemAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    TracksLot = table.Column<bool>(type: "bit", nullable: false),
                    TracksSerial = table.Column<bool>(type: "bit", nullable: false),
                    DefaultLocationId = table.Column<int>(type: "int", nullable: true),
                    MinQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReorderPoint = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReorderQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItemAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryLocation",
                schema: "inv",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    OrgUnitId = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Kind = table.Column<byte>(type: "tinyint", nullable: false),
                    ParentLocationId = table.Column<int>(type: "int", nullable: true),
                    Path = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Depth = table.Column<byte>(type: "tinyint", nullable: false),
                    ProductionLocationId = table.Column<short>(type: "smallint", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryLocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryLocation_InventoryLocation_ParentLocationId",
                        column: x => x.ParentLocationId,
                        principalSchema: "inv",
                        principalTable: "InventoryLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryLocation_OrgUnit_OrgUnitId",
                        column: x => x.OrgUnitId,
                        principalSchema: "org",
                        principalTable: "OrgUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InventoryLocation_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "org",
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryLocationAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryLocationId = table.Column<int>(type: "int", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    OrgUnitId = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Kind = table.Column<byte>(type: "tinyint", nullable: false),
                    ParentLocationId = table.Column<int>(type: "int", nullable: true),
                    Path = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Depth = table.Column<byte>(type: "tinyint", nullable: false),
                    ProductionLocationId = table.Column<short>(type: "smallint", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryLocationAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransactionType",
                schema: "inv",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Sign = table.Column<short>(type: "smallint", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: false),
                    EmitsJson = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransactionType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LotAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LotId = table.Column<int>(type: "int", nullable: false),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    LotCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ManufacturedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VendorBusinessEntityId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LotAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SerialUnitAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerialUnitId = table.Column<int>(type: "int", nullable: false),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    LotId = table.Column<int>(type: "int", nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CurrentLocationId = table.Column<int>(type: "int", nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerialUnitAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryItem",
                schema: "inv",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    TracksLot = table.Column<bool>(type: "bit", nullable: false),
                    TracksSerial = table.Column<bool>(type: "bit", nullable: false),
                    DefaultLocationId = table.Column<int>(type: "int", nullable: true),
                    MinQty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MaxQty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ReorderPoint = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ReorderQty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryItem_InventoryLocation_DefaultLocationId",
                        column: x => x.DefaultLocationId,
                        principalSchema: "inv",
                        principalTable: "InventoryLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Lot",
                schema: "inv",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    LotCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ManufacturedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VendorBusinessEntityId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lot_InventoryItem_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalSchema: "inv",
                        principalTable: "InventoryItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryBalance",
                schema: "inv",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    LotId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    LastCountedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastTransactionAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryBalance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryBalance_InventoryItem_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalSchema: "inv",
                        principalTable: "InventoryItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryBalance_InventoryLocation_LocationId",
                        column: x => x.LocationId,
                        principalSchema: "inv",
                        principalTable: "InventoryLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryBalance_Lot_LotId",
                        column: x => x.LotId,
                        principalSchema: "inv",
                        principalTable: "Lot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SerialUnit",
                schema: "inv",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    LotId = table.Column<int>(type: "int", nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CurrentLocationId = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerialUnit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SerialUnit_InventoryItem_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalSchema: "inv",
                        principalTable: "InventoryItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SerialUnit_InventoryLocation_CurrentLocationId",
                        column: x => x.CurrentLocationId,
                        principalSchema: "inv",
                        principalTable: "InventoryLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SerialUnit_Lot_LotId",
                        column: x => x.LotId,
                        principalSchema: "inv",
                        principalTable: "Lot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransaction",
                schema: "inv",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    TransactionTypeId = table.Column<int>(type: "int", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    FromLocationId = table.Column<int>(type: "int", nullable: true),
                    ToLocationId = table.Column<int>(type: "int", nullable: true),
                    LotId = table.Column<int>(type: "int", nullable: true),
                    SerialUnitId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitMeasureCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    FromStatus = table.Column<byte>(type: "tinyint", nullable: true),
                    ToStatus = table.Column<byte>(type: "tinyint", nullable: true),
                    ReferenceType = table.Column<byte>(type: "tinyint", nullable: true),
                    ReferenceId = table.Column<int>(type: "int", nullable: true),
                    ReferenceLineId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryTransaction_InventoryItem_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalSchema: "inv",
                        principalTable: "InventoryItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryTransaction_InventoryLocation_FromLocationId",
                        column: x => x.FromLocationId,
                        principalSchema: "inv",
                        principalTable: "InventoryLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryTransaction_InventoryLocation_ToLocationId",
                        column: x => x.ToLocationId,
                        principalSchema: "inv",
                        principalTable: "InventoryLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryTransaction_InventoryTransactionType_TransactionTypeId",
                        column: x => x.TransactionTypeId,
                        principalSchema: "inv",
                        principalTable: "InventoryTransactionType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryTransaction_Lot_LotId",
                        column: x => x.LotId,
                        principalSchema: "inv",
                        principalTable: "Lot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryTransaction_SerialUnit_SerialUnitId",
                        column: x => x.SerialUnitId,
                        principalSchema: "inv",
                        principalTable: "SerialUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryAdjustment",
                schema: "inv",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdjustmentNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    LotId = table.Column<int>(type: "int", nullable: true),
                    QuantityDelta = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ReasonCode = table.Column<byte>(type: "tinyint", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    RequestedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PostedTransactionId = table.Column<long>(type: "bigint", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryAdjustment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryAdjustment_InventoryItem_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalSchema: "inv",
                        principalTable: "InventoryItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryAdjustment_InventoryLocation_LocationId",
                        column: x => x.LocationId,
                        principalSchema: "inv",
                        principalTable: "InventoryLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryAdjustment_InventoryTransaction_PostedTransactionId",
                        column: x => x.PostedTransactionId,
                        principalSchema: "inv",
                        principalTable: "InventoryTransaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryAdjustment_Lot_LotId",
                        column: x => x.LotId,
                        principalSchema: "inv",
                        principalTable: "Lot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransactionOutbox",
                schema: "inv",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryTransactionId = table.Column<long>(type: "bigint", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    NextAttemptAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransactionOutbox", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryTransactionOutbox_InventoryTransaction_InventoryTransactionId",
                        column: x => x.InventoryTransactionId,
                        principalSchema: "inv",
                        principalTable: "InventoryTransaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransactionQueue",
                schema: "inv",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Source = table.Column<byte>(type: "tinyint", nullable: false),
                    RawPayload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParseStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    ProcessStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PostedTransactionId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransactionQueue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryTransactionQueue_InventoryTransaction_PostedTransactionId",
                        column: x => x.PostedTransactionId,
                        principalSchema: "inv",
                        principalTable: "InventoryTransaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustment_AdjustmentNumber",
                schema: "inv",
                table: "InventoryAdjustment",
                column: "AdjustmentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustment_InventoryItemId_LocationId",
                schema: "inv",
                table: "InventoryAdjustment",
                columns: new[] { "InventoryItemId", "LocationId" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustment_LocationId",
                schema: "inv",
                table: "InventoryAdjustment",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustment_LotId",
                schema: "inv",
                table: "InventoryAdjustment",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustment_PostedTransactionId",
                schema: "inv",
                table: "InventoryAdjustment",
                column: "PostedTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustment_Status",
                schema: "inv",
                table: "InventoryAdjustment",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustmentAuditLogs_ChangedDate",
                table: "InventoryAdjustmentAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustmentAuditLogs_InventoryAdjustmentId",
                table: "InventoryAdjustmentAuditLogs",
                column: "InventoryAdjustmentId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryBalance_LocationId",
                schema: "inv",
                table: "InventoryBalance",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryBalance_LotId",
                schema: "inv",
                table: "InventoryBalance",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "UX_InventoryBalance_ItemLocationLotStatus",
                schema: "inv",
                table: "InventoryBalance",
                columns: new[] { "InventoryItemId", "LocationId", "LotId", "Status" },
                unique: true,
                filter: "[LotId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_InventoryBalance_ItemLocationStatus_NoLot",
                schema: "inv",
                table: "InventoryBalance",
                columns: new[] { "InventoryItemId", "LocationId", "Status" },
                unique: true,
                filter: "[LotId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItem_DefaultLocationId",
                schema: "inv",
                table: "InventoryItem",
                column: "DefaultLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItem_ProductId",
                schema: "inv",
                table: "InventoryItem",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItemAuditLogs_ChangedDate",
                table: "InventoryItemAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItemAuditLogs_InventoryItemId",
                table: "InventoryItemAuditLogs",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLocation_OrganizationId_Code",
                schema: "inv",
                table: "InventoryLocation",
                columns: new[] { "OrganizationId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLocation_OrgUnitId",
                schema: "inv",
                table: "InventoryLocation",
                column: "OrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLocation_ParentLocationId",
                schema: "inv",
                table: "InventoryLocation",
                column: "ParentLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLocation_Path",
                schema: "inv",
                table: "InventoryLocation",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLocationAuditLogs_ChangedDate",
                table: "InventoryLocationAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLocationAuditLogs_InventoryLocationId",
                table: "InventoryLocationAuditLogs",
                column: "InventoryLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransaction_CorrelationId",
                schema: "inv",
                table: "InventoryTransaction",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransaction_FromLocationId",
                schema: "inv",
                table: "InventoryTransaction",
                column: "FromLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransaction_InventoryItemId_OccurredAt",
                schema: "inv",
                table: "InventoryTransaction",
                columns: new[] { "InventoryItemId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransaction_LotId",
                schema: "inv",
                table: "InventoryTransaction",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransaction_OccurredAt",
                schema: "inv",
                table: "InventoryTransaction",
                column: "OccurredAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransaction_ReferenceType_ReferenceId",
                schema: "inv",
                table: "InventoryTransaction",
                columns: new[] { "ReferenceType", "ReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransaction_SerialUnitId",
                schema: "inv",
                table: "InventoryTransaction",
                column: "SerialUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransaction_ToLocationId",
                schema: "inv",
                table: "InventoryTransaction",
                column: "ToLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransaction_TransactionNumber",
                schema: "inv",
                table: "InventoryTransaction",
                column: "TransactionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransaction_TransactionTypeId",
                schema: "inv",
                table: "InventoryTransaction",
                column: "TransactionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactionOutbox_InventoryTransactionId",
                schema: "inv",
                table: "InventoryTransactionOutbox",
                column: "InventoryTransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactionOutbox_Status_NextAttemptAt",
                schema: "inv",
                table: "InventoryTransactionOutbox",
                columns: new[] { "Status", "NextAttemptAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactionQueue_ParseStatus_ProcessStatus_ReceivedAt",
                schema: "inv",
                table: "InventoryTransactionQueue",
                columns: new[] { "ParseStatus", "ProcessStatus", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactionQueue_PostedTransactionId",
                schema: "inv",
                table: "InventoryTransactionQueue",
                column: "PostedTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactionType_Code",
                schema: "inv",
                table: "InventoryTransactionType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lot_InventoryItemId_LotCode",
                schema: "inv",
                table: "Lot",
                columns: new[] { "InventoryItemId", "LotCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lot_VendorBusinessEntityId",
                schema: "inv",
                table: "Lot",
                column: "VendorBusinessEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_LotAuditLogs_ChangedDate",
                table: "LotAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_LotAuditLogs_LotId",
                table: "LotAuditLogs",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_SerialUnit_CurrentLocationId",
                schema: "inv",
                table: "SerialUnit",
                column: "CurrentLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_SerialUnit_InventoryItemId_SerialNumber",
                schema: "inv",
                table: "SerialUnit",
                columns: new[] { "InventoryItemId", "SerialNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SerialUnit_LotId",
                schema: "inv",
                table: "SerialUnit",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_SerialUnitAuditLogs_ChangedDate",
                table: "SerialUnitAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_SerialUnitAuditLogs_SerialUnitId",
                table: "SerialUnitAuditLogs",
                column: "SerialUnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryAdjustment",
                schema: "inv");

            migrationBuilder.DropTable(
                name: "InventoryAdjustmentAuditLogs");

            migrationBuilder.DropTable(
                name: "InventoryBalance",
                schema: "inv");

            migrationBuilder.DropTable(
                name: "InventoryItemAuditLogs");

            migrationBuilder.DropTable(
                name: "InventoryLocationAuditLogs");

            migrationBuilder.DropTable(
                name: "InventoryTransactionOutbox",
                schema: "inv");

            migrationBuilder.DropTable(
                name: "InventoryTransactionQueue",
                schema: "inv");

            migrationBuilder.DropTable(
                name: "LotAuditLogs");

            migrationBuilder.DropTable(
                name: "SerialUnitAuditLogs");

            migrationBuilder.DropTable(
                name: "InventoryTransaction",
                schema: "inv");

            migrationBuilder.DropTable(
                name: "InventoryTransactionType",
                schema: "inv");

            migrationBuilder.DropTable(
                name: "SerialUnit",
                schema: "inv");

            migrationBuilder.DropTable(
                name: "Lot",
                schema: "inv");

            migrationBuilder.DropTable(
                name: "InventoryItem",
                schema: "inv");

            migrationBuilder.DropTable(
                name: "InventoryLocation",
                schema: "inv");
        }
    }
}
