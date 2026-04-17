using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AWBlazorApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLogistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "lgx");

            migrationBuilder.CreateTable(
                name: "GoodsReceipt",
                schema: "lgx",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: true),
                    VendorBusinessEntityId = table.Column<int>(type: "int", nullable: true),
                    ReceivedLocationId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PostedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsReceipt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoodsReceipt_InventoryLocation_ReceivedLocationId",
                        column: x => x.ReceivedLocationId,
                        principalSchema: "inv",
                        principalTable: "InventoryLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GoodsReceiptAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GoodsReceiptId = table.Column<int>(type: "int", nullable: false),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: true),
                    VendorBusinessEntityId = table.Column<int>(type: "int", nullable: true),
                    ReceivedLocationId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PostedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsReceiptAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GoodsReceiptLineAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GoodsReceiptLineId = table.Column<int>(type: "int", nullable: false),
                    GoodsReceiptId = table.Column<int>(type: "int", nullable: false),
                    PurchaseOrderDetailId = table.Column<int>(type: "int", nullable: true),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitMeasureCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    LotId = table.Column<int>(type: "int", nullable: true),
                    PostedTransactionId = table.Column<long>(type: "bigint", nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsReceiptLineAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shipment",
                schema: "lgx",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShipmentNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SalesOrderId = table.Column<int>(type: "int", nullable: true),
                    CustomerBusinessEntityId = table.Column<int>(type: "int", nullable: true),
                    ShipMethodId = table.Column<int>(type: "int", nullable: true),
                    TrackingNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ShippedFromLocationId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    ShippedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PostedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PostedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shipment_InventoryLocation_ShippedFromLocationId",
                        column: x => x.ShippedFromLocationId,
                        principalSchema: "inv",
                        principalTable: "InventoryLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShipmentAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShipmentId = table.Column<int>(type: "int", nullable: false),
                    ShipmentNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    SalesOrderId = table.Column<int>(type: "int", nullable: true),
                    CustomerBusinessEntityId = table.Column<int>(type: "int", nullable: true),
                    ShipMethodId = table.Column<int>(type: "int", nullable: true),
                    TrackingNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ShippedFromLocationId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    ShippedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PostedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PostedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShipmentLineAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShipmentLineId = table.Column<int>(type: "int", nullable: false),
                    ShipmentId = table.Column<int>(type: "int", nullable: false),
                    SalesOrderDetailId = table.Column<int>(type: "int", nullable: true),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitMeasureCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    LotId = table.Column<int>(type: "int", nullable: true),
                    SerialUnitId = table.Column<int>(type: "int", nullable: true),
                    PostedTransactionId = table.Column<long>(type: "bigint", nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentLineAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockTransfer",
                schema: "lgx",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    FromLocationId = table.Column<int>(type: "int", nullable: false),
                    ToLocationId = table.Column<int>(type: "int", nullable: false),
                    FromOrganizationId = table.Column<int>(type: "int", nullable: true),
                    ToOrganizationId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InitiatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PostedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransfer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockTransfer_InventoryLocation_FromLocationId",
                        column: x => x.FromLocationId,
                        principalSchema: "inv",
                        principalTable: "InventoryLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockTransfer_InventoryLocation_ToLocationId",
                        column: x => x.ToLocationId,
                        principalSchema: "inv",
                        principalTable: "InventoryLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockTransfer_Organization_FromOrganizationId",
                        column: x => x.FromOrganizationId,
                        principalSchema: "org",
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockTransfer_Organization_ToOrganizationId",
                        column: x => x.ToOrganizationId,
                        principalSchema: "org",
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockTransferAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockTransferId = table.Column<int>(type: "int", nullable: false),
                    TransferNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    FromLocationId = table.Column<int>(type: "int", nullable: false),
                    ToLocationId = table.Column<int>(type: "int", nullable: false),
                    FromOrganizationId = table.Column<int>(type: "int", nullable: true),
                    ToOrganizationId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InitiatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PostedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransferAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockTransferLineAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockTransferLineId = table.Column<int>(type: "int", nullable: false),
                    StockTransferId = table.Column<int>(type: "int", nullable: false),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitMeasureCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    LotId = table.Column<int>(type: "int", nullable: true),
                    SerialUnitId = table.Column<int>(type: "int", nullable: true),
                    FromTransactionId = table.Column<long>(type: "bigint", nullable: true),
                    ToTransactionId = table.Column<long>(type: "bigint", nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransferLineAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GoodsReceiptLine",
                schema: "lgx",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GoodsReceiptId = table.Column<int>(type: "int", nullable: false),
                    PurchaseOrderDetailId = table.Column<int>(type: "int", nullable: true),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitMeasureCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    LotId = table.Column<int>(type: "int", nullable: true),
                    PostedTransactionId = table.Column<long>(type: "bigint", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsReceiptLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoodsReceiptLine_GoodsReceipt_GoodsReceiptId",
                        column: x => x.GoodsReceiptId,
                        principalSchema: "lgx",
                        principalTable: "GoodsReceipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoodsReceiptLine_InventoryItem_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalSchema: "inv",
                        principalTable: "InventoryItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoodsReceiptLine_InventoryTransaction_PostedTransactionId",
                        column: x => x.PostedTransactionId,
                        principalSchema: "inv",
                        principalTable: "InventoryTransaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoodsReceiptLine_Lot_LotId",
                        column: x => x.LotId,
                        principalSchema: "inv",
                        principalTable: "Lot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ShipmentLine",
                schema: "lgx",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShipmentId = table.Column<int>(type: "int", nullable: false),
                    SalesOrderDetailId = table.Column<int>(type: "int", nullable: true),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitMeasureCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    LotId = table.Column<int>(type: "int", nullable: true),
                    SerialUnitId = table.Column<int>(type: "int", nullable: true),
                    PostedTransactionId = table.Column<long>(type: "bigint", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShipmentLine_InventoryItem_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalSchema: "inv",
                        principalTable: "InventoryItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShipmentLine_InventoryTransaction_PostedTransactionId",
                        column: x => x.PostedTransactionId,
                        principalSchema: "inv",
                        principalTable: "InventoryTransaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShipmentLine_Lot_LotId",
                        column: x => x.LotId,
                        principalSchema: "inv",
                        principalTable: "Lot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ShipmentLine_SerialUnit_SerialUnitId",
                        column: x => x.SerialUnitId,
                        principalSchema: "inv",
                        principalTable: "SerialUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ShipmentLine_Shipment_ShipmentId",
                        column: x => x.ShipmentId,
                        principalSchema: "lgx",
                        principalTable: "Shipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockTransferLine",
                schema: "lgx",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockTransferId = table.Column<int>(type: "int", nullable: false),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitMeasureCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    LotId = table.Column<int>(type: "int", nullable: true),
                    SerialUnitId = table.Column<int>(type: "int", nullable: true),
                    FromTransactionId = table.Column<long>(type: "bigint", nullable: true),
                    ToTransactionId = table.Column<long>(type: "bigint", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransferLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockTransferLine_InventoryItem_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalSchema: "inv",
                        principalTable: "InventoryItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockTransferLine_InventoryTransaction_FromTransactionId",
                        column: x => x.FromTransactionId,
                        principalSchema: "inv",
                        principalTable: "InventoryTransaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockTransferLine_InventoryTransaction_ToTransactionId",
                        column: x => x.ToTransactionId,
                        principalSchema: "inv",
                        principalTable: "InventoryTransaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockTransferLine_Lot_LotId",
                        column: x => x.LotId,
                        principalSchema: "inv",
                        principalTable: "Lot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StockTransferLine_SerialUnit_SerialUnitId",
                        column: x => x.SerialUnitId,
                        principalSchema: "inv",
                        principalTable: "SerialUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StockTransferLine_StockTransfer_StockTransferId",
                        column: x => x.StockTransferId,
                        principalSchema: "lgx",
                        principalTable: "StockTransfer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceipt_PurchaseOrderId",
                schema: "lgx",
                table: "GoodsReceipt",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceipt_ReceiptNumber",
                schema: "lgx",
                table: "GoodsReceipt",
                column: "ReceiptNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceipt_ReceivedAt",
                schema: "lgx",
                table: "GoodsReceipt",
                column: "ReceivedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceipt_ReceivedLocationId",
                schema: "lgx",
                table: "GoodsReceipt",
                column: "ReceivedLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceipt_Status",
                schema: "lgx",
                table: "GoodsReceipt",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptAuditLogs_ChangedDate",
                table: "GoodsReceiptAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptAuditLogs_GoodsReceiptId",
                table: "GoodsReceiptAuditLogs",
                column: "GoodsReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptLine_GoodsReceiptId",
                schema: "lgx",
                table: "GoodsReceiptLine",
                column: "GoodsReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptLine_InventoryItemId",
                schema: "lgx",
                table: "GoodsReceiptLine",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptLine_LotId",
                schema: "lgx",
                table: "GoodsReceiptLine",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptLine_PostedTransactionId",
                schema: "lgx",
                table: "GoodsReceiptLine",
                column: "PostedTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptLine_PurchaseOrderDetailId",
                schema: "lgx",
                table: "GoodsReceiptLine",
                column: "PurchaseOrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptLineAuditLogs_ChangedDate",
                table: "GoodsReceiptLineAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptLineAuditLogs_GoodsReceiptLineId",
                table: "GoodsReceiptLineAuditLogs",
                column: "GoodsReceiptLineId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipment_SalesOrderId",
                schema: "lgx",
                table: "Shipment",
                column: "SalesOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipment_ShipmentNumber",
                schema: "lgx",
                table: "Shipment",
                column: "ShipmentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipment_ShippedAt",
                schema: "lgx",
                table: "Shipment",
                column: "ShippedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Shipment_ShippedFromLocationId",
                schema: "lgx",
                table: "Shipment",
                column: "ShippedFromLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipment_Status",
                schema: "lgx",
                table: "Shipment",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentAuditLogs_ChangedDate",
                table: "ShipmentAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentAuditLogs_ShipmentId",
                table: "ShipmentAuditLogs",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentLine_InventoryItemId",
                schema: "lgx",
                table: "ShipmentLine",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentLine_LotId",
                schema: "lgx",
                table: "ShipmentLine",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentLine_PostedTransactionId",
                schema: "lgx",
                table: "ShipmentLine",
                column: "PostedTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentLine_SalesOrderDetailId",
                schema: "lgx",
                table: "ShipmentLine",
                column: "SalesOrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentLine_SerialUnitId",
                schema: "lgx",
                table: "ShipmentLine",
                column: "SerialUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentLine_ShipmentId",
                schema: "lgx",
                table: "ShipmentLine",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentLineAuditLogs_ChangedDate",
                table: "ShipmentLineAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentLineAuditLogs_ShipmentLineId",
                table: "ShipmentLineAuditLogs",
                column: "ShipmentLineId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfer_CorrelationId",
                schema: "lgx",
                table: "StockTransfer",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfer_FromLocationId",
                schema: "lgx",
                table: "StockTransfer",
                column: "FromLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfer_FromOrganizationId",
                schema: "lgx",
                table: "StockTransfer",
                column: "FromOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfer_Status",
                schema: "lgx",
                table: "StockTransfer",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfer_ToLocationId",
                schema: "lgx",
                table: "StockTransfer",
                column: "ToLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfer_ToOrganizationId",
                schema: "lgx",
                table: "StockTransfer",
                column: "ToOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfer_TransferNumber",
                schema: "lgx",
                table: "StockTransfer",
                column: "TransferNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferAuditLogs_ChangedDate",
                table: "StockTransferAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferAuditLogs_StockTransferId",
                table: "StockTransferAuditLogs",
                column: "StockTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferLine_FromTransactionId",
                schema: "lgx",
                table: "StockTransferLine",
                column: "FromTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferLine_InventoryItemId",
                schema: "lgx",
                table: "StockTransferLine",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferLine_LotId",
                schema: "lgx",
                table: "StockTransferLine",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferLine_SerialUnitId",
                schema: "lgx",
                table: "StockTransferLine",
                column: "SerialUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferLine_StockTransferId",
                schema: "lgx",
                table: "StockTransferLine",
                column: "StockTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferLine_ToTransactionId",
                schema: "lgx",
                table: "StockTransferLine",
                column: "ToTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferLineAuditLogs_ChangedDate",
                table: "StockTransferLineAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferLineAuditLogs_StockTransferLineId",
                table: "StockTransferLineAuditLogs",
                column: "StockTransferLineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoodsReceiptAuditLogs");

            migrationBuilder.DropTable(
                name: "GoodsReceiptLine",
                schema: "lgx");

            migrationBuilder.DropTable(
                name: "GoodsReceiptLineAuditLogs");

            migrationBuilder.DropTable(
                name: "ShipmentAuditLogs");

            migrationBuilder.DropTable(
                name: "ShipmentLine",
                schema: "lgx");

            migrationBuilder.DropTable(
                name: "ShipmentLineAuditLogs");

            migrationBuilder.DropTable(
                name: "StockTransferAuditLogs");

            migrationBuilder.DropTable(
                name: "StockTransferLine",
                schema: "lgx");

            migrationBuilder.DropTable(
                name: "StockTransferLineAuditLogs");

            migrationBuilder.DropTable(
                name: "GoodsReceipt",
                schema: "lgx");

            migrationBuilder.DropTable(
                name: "Shipment",
                schema: "lgx");

            migrationBuilder.DropTable(
                name: "StockTransfer",
                schema: "lgx");
        }
    }
}
