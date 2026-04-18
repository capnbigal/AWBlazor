using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AWBlazorApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEngineeringModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "eng");

            migrationBuilder.CreateTable(
                name: "BomHeader",
                schema: "eng",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    RevisionNumber = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BomHeader", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BomHeaderAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BomHeaderId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    RevisionNumber = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BomHeaderAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviationRequest",
                schema: "eng",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ProposedDisposition = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AuthorizedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UnitMeasureCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    RaisedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RaisedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DecidedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DecisionNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviationRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviationRequestAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviationRequestId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ProposedDisposition = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AuthorizedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UnitMeasureCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    RaisedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RaisedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DecidedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DecisionNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviationRequestAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EngineeringChangeOrder",
                schema: "eng",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    RaisedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RaisedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DecidedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    DecisionNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EngineeringChangeOrder", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EngineeringChangeOrderAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EngineeringChangeOrderId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    RaisedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RaisedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DecidedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    DecisionNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EngineeringChangeOrderAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EngineeringDocument",
                schema: "eng",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Kind = table.Column<byte>(type: "tinyint", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    RevisionNumber = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EngineeringDocument", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EngineeringDocumentAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EngineeringDocumentId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Kind = table.Column<byte>(type: "tinyint", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    RevisionNumber = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EngineeringDocumentAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ManufacturingRouting",
                schema: "eng",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    RevisionNumber = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManufacturingRouting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ManufacturingRoutingAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ManufacturingRoutingId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    RevisionNumber = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManufacturingRoutingAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BomLine",
                schema: "eng",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BomHeaderId = table.Column<int>(type: "int", nullable: false),
                    ComponentProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitMeasureCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ScrapPercentage = table.Column<decimal>(type: "decimal(5,4)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BomLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BomLine_BomHeader_BomHeaderId",
                        column: x => x.BomHeaderId,
                        principalSchema: "eng",
                        principalTable: "BomHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EcoAffectedItem",
                schema: "eng",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EngineeringChangeOrderId = table.Column<int>(type: "int", nullable: false),
                    AffectedKind = table.Column<byte>(type: "tinyint", nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcoAffectedItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EcoAffectedItem_EngineeringChangeOrder_EngineeringChangeOrderId",
                        column: x => x.EngineeringChangeOrderId,
                        principalSchema: "eng",
                        principalTable: "EngineeringChangeOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EcoApproval",
                schema: "eng",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EngineeringChangeOrderId = table.Column<int>(type: "int", nullable: false),
                    ApproverUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Decision = table.Column<byte>(type: "tinyint", nullable: false),
                    DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcoApproval", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EcoApproval_EngineeringChangeOrder_EngineeringChangeOrderId",
                        column: x => x.EngineeringChangeOrderId,
                        principalSchema: "eng",
                        principalTable: "EngineeringChangeOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoutingStep",
                schema: "eng",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ManufacturingRoutingId = table.Column<int>(type: "int", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    OperationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StationId = table.Column<int>(type: "int", nullable: true),
                    StandardMinutes = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutingStep", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoutingStep_ManufacturingRouting_ManufacturingRoutingId",
                        column: x => x.ManufacturingRoutingId,
                        principalSchema: "eng",
                        principalTable: "ManufacturingRouting",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoutingStep_Station_StationId",
                        column: x => x.StationId,
                        principalSchema: "org",
                        principalTable: "Station",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BomHeader_Code",
                schema: "eng",
                table: "BomHeader",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BomHeader_ProductId_IsActive",
                schema: "eng",
                table: "BomHeader",
                columns: new[] { "ProductId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_BomHeader_ProductId_RevisionNumber",
                schema: "eng",
                table: "BomHeader",
                columns: new[] { "ProductId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BomHeaderAuditLogs_BomHeaderId",
                table: "BomHeaderAuditLogs",
                column: "BomHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_BomHeaderAuditLogs_ChangedDate",
                table: "BomHeaderAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_BomLine_BomHeaderId",
                schema: "eng",
                table: "BomLine",
                column: "BomHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_BomLine_ComponentProductId",
                schema: "eng",
                table: "BomLine",
                column: "ComponentProductId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviationRequest_Code",
                schema: "eng",
                table: "DeviationRequest",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviationRequest_ProductId_Status",
                schema: "eng",
                table: "DeviationRequest",
                columns: new[] { "ProductId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviationRequest_RaisedAt",
                schema: "eng",
                table: "DeviationRequest",
                column: "RaisedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_DeviationRequestAuditLogs_ChangedDate",
                table: "DeviationRequestAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_DeviationRequestAuditLogs_DeviationRequestId",
                table: "DeviationRequestAuditLogs",
                column: "DeviationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_EcoAffectedItem_AffectedKind_TargetId",
                schema: "eng",
                table: "EcoAffectedItem",
                columns: new[] { "AffectedKind", "TargetId" });

            migrationBuilder.CreateIndex(
                name: "IX_EcoAffectedItem_EngineeringChangeOrderId",
                schema: "eng",
                table: "EcoAffectedItem",
                column: "EngineeringChangeOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_EcoApproval_DecidedAt",
                schema: "eng",
                table: "EcoApproval",
                column: "DecidedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_EcoApproval_EngineeringChangeOrderId",
                schema: "eng",
                table: "EcoApproval",
                column: "EngineeringChangeOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineeringChangeOrder_Code",
                schema: "eng",
                table: "EngineeringChangeOrder",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EngineeringChangeOrder_RaisedAt",
                schema: "eng",
                table: "EngineeringChangeOrder",
                column: "RaisedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_EngineeringChangeOrder_Status",
                schema: "eng",
                table: "EngineeringChangeOrder",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EngineeringChangeOrderAuditLogs_ChangedDate",
                table: "EngineeringChangeOrderAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_EngineeringChangeOrderAuditLogs_EngineeringChangeOrderId",
                table: "EngineeringChangeOrderAuditLogs",
                column: "EngineeringChangeOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineeringDocument_Code",
                schema: "eng",
                table: "EngineeringDocument",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EngineeringDocument_ProductId_Kind",
                schema: "eng",
                table: "EngineeringDocument",
                columns: new[] { "ProductId", "Kind" });

            migrationBuilder.CreateIndex(
                name: "IX_EngineeringDocumentAuditLogs_ChangedDate",
                table: "EngineeringDocumentAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_EngineeringDocumentAuditLogs_EngineeringDocumentId",
                table: "EngineeringDocumentAuditLogs",
                column: "EngineeringDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingRouting_Code",
                schema: "eng",
                table: "ManufacturingRouting",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingRouting_ProductId_IsActive",
                schema: "eng",
                table: "ManufacturingRouting",
                columns: new[] { "ProductId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingRouting_ProductId_RevisionNumber",
                schema: "eng",
                table: "ManufacturingRouting",
                columns: new[] { "ProductId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingRoutingAuditLogs_ChangedDate",
                table: "ManufacturingRoutingAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingRoutingAuditLogs_ManufacturingRoutingId",
                table: "ManufacturingRoutingAuditLogs",
                column: "ManufacturingRoutingId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutingStep_ManufacturingRoutingId_SequenceNumber",
                schema: "eng",
                table: "RoutingStep",
                columns: new[] { "ManufacturingRoutingId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoutingStep_StationId",
                schema: "eng",
                table: "RoutingStep",
                column: "StationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BomHeaderAuditLogs");

            migrationBuilder.DropTable(
                name: "BomLine",
                schema: "eng");

            migrationBuilder.DropTable(
                name: "DeviationRequest",
                schema: "eng");

            migrationBuilder.DropTable(
                name: "DeviationRequestAuditLogs");

            migrationBuilder.DropTable(
                name: "EcoAffectedItem",
                schema: "eng");

            migrationBuilder.DropTable(
                name: "EcoApproval",
                schema: "eng");

            migrationBuilder.DropTable(
                name: "EngineeringChangeOrderAuditLogs");

            migrationBuilder.DropTable(
                name: "EngineeringDocument",
                schema: "eng");

            migrationBuilder.DropTable(
                name: "EngineeringDocumentAuditLogs");

            migrationBuilder.DropTable(
                name: "ManufacturingRoutingAuditLogs");

            migrationBuilder.DropTable(
                name: "RoutingStep",
                schema: "eng");

            migrationBuilder.DropTable(
                name: "BomHeader",
                schema: "eng");

            migrationBuilder.DropTable(
                name: "EngineeringChangeOrder",
                schema: "eng");

            migrationBuilder.DropTable(
                name: "ManufacturingRouting",
                schema: "eng");
        }
    }
}
