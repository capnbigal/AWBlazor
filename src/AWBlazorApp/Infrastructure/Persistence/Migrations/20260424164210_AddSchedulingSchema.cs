using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AWBlazorApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSchedulingSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Scheduling");

            migrationBuilder.CreateTable(
                name: "LineConfiguration",
                schema: "Scheduling",
                columns: table => new
                {
                    LineConfigurationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationID = table.Column<short>(type: "smallint", nullable: false),
                    TaktSeconds = table.Column<int>(type: "int", nullable: false),
                    ShiftsPerDay = table.Column<byte>(type: "tinyint", nullable: false),
                    MinutesPerShift = table.Column<short>(type: "smallint", nullable: false),
                    FrozenLookaheadHours = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineConfiguration", x => x.LineConfigurationID);
                });

            migrationBuilder.CreateTable(
                name: "LineProductAssignment",
                schema: "Scheduling",
                columns: table => new
                {
                    LineProductAssignmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationID = table.Column<short>(type: "smallint", nullable: false),
                    ProductModelID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineProductAssignment", x => x.LineProductAssignmentID);
                });

            migrationBuilder.CreateTable(
                name: "SchedulingAlert",
                schema: "Scheduling",
                columns: table => new
                {
                    SchedulingAlertID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Severity = table.Column<byte>(type: "tinyint", nullable: false),
                    EventType = table.Column<byte>(type: "tinyint", nullable: false),
                    WeekId = table.Column<int>(type: "int", nullable: false),
                    LocationID = table.Column<short>(type: "smallint", nullable: false),
                    SalesOrderID = table.Column<int>(type: "int", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AcknowledgedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcknowledgedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulingAlert", x => x.SchedulingAlertID);
                });

            migrationBuilder.CreateTable(
                name: "SchedulingException",
                schema: "Scheduling",
                columns: table => new
                {
                    SchedulingExceptionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WeekId = table.Column<int>(type: "int", nullable: false),
                    LocationID = table.Column<short>(type: "smallint", nullable: false),
                    SalesOrderDetailID = table.Column<int>(type: "int", nullable: false),
                    ExceptionType = table.Column<byte>(type: "tinyint", nullable: false),
                    PinnedSequence = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulingException", x => x.SchedulingExceptionID);
                });

            migrationBuilder.CreateTable(
                name: "SchedulingRule",
                schema: "Scheduling",
                columns: table => new
                {
                    SchedulingRuleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventType = table.Column<byte>(type: "tinyint", nullable: false),
                    InFrozenWindow = table.Column<bool>(type: "bit", nullable: false),
                    Action = table.Column<byte>(type: "tinyint", nullable: false),
                    ParametersJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulingRule", x => x.SchedulingRuleID);
                });

            migrationBuilder.CreateTable(
                name: "WeeklyPlan",
                schema: "Scheduling",
                columns: table => new
                {
                    WeeklyPlanID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WeekId = table.Column<int>(type: "int", nullable: false),
                    LocationID = table.Column<short>(type: "smallint", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublishedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    BaselineDiverged = table.Column<bool>(type: "bit", nullable: false),
                    GenerationOptionsJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyPlan", x => x.WeeklyPlanID);
                });

            migrationBuilder.CreateTable(
                name: "WeeklyPlanItem",
                schema: "Scheduling",
                columns: table => new
                {
                    WeeklyPlanItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WeeklyPlanID = table.Column<int>(type: "int", nullable: false),
                    SalesOrderID = table.Column<int>(type: "int", nullable: false),
                    SalesOrderDetailID = table.Column<int>(type: "int", nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    PlannedSequence = table.Column<int>(type: "int", nullable: false),
                    PlannedStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlannedEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlannedQty = table.Column<short>(type: "smallint", nullable: false),
                    OverCapacity = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyPlanItem", x => x.WeeklyPlanItemID);
                    table.ForeignKey(
                        name: "FK_WeeklyPlanItem_WeeklyPlan_WeeklyPlanID",
                        column: x => x.WeeklyPlanID,
                        principalSchema: "Scheduling",
                        principalTable: "WeeklyPlan",
                        principalColumn: "WeeklyPlanID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LineConfiguration_LocationID",
                schema: "Scheduling",
                table: "LineConfiguration",
                column: "LocationID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LineProductAssignment_LocationID_ProductModelID",
                schema: "Scheduling",
                table: "LineProductAssignment",
                columns: new[] { "LocationID", "ProductModelID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchedulingAlert_AcknowledgedAt_CreatedAt",
                schema: "Scheduling",
                table: "SchedulingAlert",
                columns: new[] { "AcknowledgedAt", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SchedulingException_WeekId_LocationID_SalesOrderDetailID",
                schema: "Scheduling",
                table: "SchedulingException",
                columns: new[] { "WeekId", "LocationID", "SalesOrderDetailID" },
                unique: true,
                filter: "[ResolvedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyPlan_WeekId_LocationID_Version",
                schema: "Scheduling",
                table: "WeeklyPlan",
                columns: new[] { "WeekId", "LocationID", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyPlanItem_SalesOrderDetailID",
                schema: "Scheduling",
                table: "WeeklyPlanItem",
                column: "SalesOrderDetailID");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyPlanItem_WeeklyPlanID_PlannedSequence",
                schema: "Scheduling",
                table: "WeeklyPlanItem",
                columns: new[] { "WeeklyPlanID", "PlannedSequence" });

            // Slice-1 note: CurrentStart/CurrentEnd currently equal PlannedStart/PlannedEnd and
            // StartDriftMinutes is always 0. Real takt-based recompute against live CurrentQty
            // sequences lands in slice 2. The sequence drift is already correct.
            migrationBuilder.Sql(@"
CREATE OR ALTER VIEW Scheduling.vw_CurrentDeliverySchedule AS
WITH LatestPlan AS (
    SELECT wp.WeekId, wp.LocationID, wp.WeeklyPlanID,
           ROW_NUMBER() OVER (PARTITION BY wp.WeekId, wp.LocationID ORDER BY wp.Version DESC) AS rn
    FROM Scheduling.WeeklyPlan wp
),
Items AS (
    SELECT wpi.*, lp.WeekId, lp.LocationID
    FROM Scheduling.WeeklyPlanItem wpi
    JOIN LatestPlan lp ON lp.WeeklyPlanID = wpi.WeeklyPlanID AND lp.rn = 1
),
ActiveEx AS (
    SELECT * FROM Scheduling.SchedulingException WHERE ResolvedAt IS NULL
),
Joined AS (
    SELECT
        i.WeekId, i.LocationID,
        i.SalesOrderID, i.SalesOrderDetailID, i.ProductID,
        i.PlannedSequence, i.PlannedStart, i.PlannedEnd, i.PlannedQty,
        soh.DueDate        AS PromiseDate,
        soh.Status         AS SoStatus,
        CASE WHEN soh.Status = 6 THEN 1 ELSE 0 END AS IsCancelled,
        CASE WHEN soh.OnlineOrderFlag = 0 THEN 1 ELSE 0 END AS CustomerPriority,
        sod.OrderQty       AS CurrentQty,
        p.ProductModelID,
        soh.TotalDue,
        soh.ModifiedDate   AS SoModifiedDate,
        ax.ExceptionType,
        ax.PinnedSequence,
        ax.Reason          AS ExceptionReason,
        CASE WHEN ax.ExceptionType = 3 THEN 1 ELSE 0 END AS IsHotOrder
    FROM Items i
    LEFT JOIN Sales.SalesOrderHeader soh ON soh.SalesOrderID = i.SalesOrderID
    LEFT JOIN Sales.SalesOrderDetail sod ON sod.SalesOrderDetailID = i.SalesOrderDetailID
    LEFT JOIN Production.Product   p   ON p.ProductID = i.ProductID
    LEFT JOIN ActiveEx              ax  ON ax.SalesOrderDetailID = i.SalesOrderDetailID
                                         AND ax.WeekId = i.WeekId
                                         AND ax.LocationID = i.LocationID
),
Sequenced AS (
    SELECT *,
        CASE WHEN IsCancelled = 1 THEN NULL
             ELSE ROW_NUMBER() OVER (
                PARTITION BY WeekId, LocationID, IsCancelled
                ORDER BY
                    CASE WHEN PinnedSequence IS NULL THEN 1 ELSE 0 END,
                    PinnedSequence,
                    IsHotOrder DESC,
                    PromiseDate ASC,
                    CustomerPriority DESC,
                    ProductModelID ASC,
                    TotalDue DESC,
                    SoModifiedDate ASC,
                    SalesOrderDetailID ASC)
        END AS CurrentSequence
    FROM Joined
)
SELECT
    WeekId, LocationID, SalesOrderID, SalesOrderDetailID, ProductID,
    PlannedSequence, PlannedStart, PlannedEnd, PlannedQty,
    CurrentSequence,
    PlannedStart      AS CurrentStart,
    PlannedEnd        AS CurrentEnd,
    CurrentQty,
    CASE WHEN CurrentSequence IS NULL OR PlannedSequence IS NULL THEN NULL
         ELSE CurrentSequence - PlannedSequence END AS SequenceDrift,
    0                                AS StartDriftMinutes,
    PromiseDate,
    CASE WHEN PromiseDate IS NULL OR PlannedEnd IS NULL THEN NULL
         ELSE DATEDIFF(MINUTE, PromiseDate, PlannedEnd) END AS PromiseDriftMinutes,
    ExceptionType,
    ExceptionReason,
    SoStatus,
    CAST(IsCancelled AS BIT) AS IsCancelled,
    CAST(IsHotOrder  AS BIT) AS IsHotOrder
FROM Sequenced;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF OBJECT_ID('Scheduling.vw_CurrentDeliverySchedule','V') IS NOT NULL DROP VIEW Scheduling.vw_CurrentDeliverySchedule;");

            migrationBuilder.DropTable(
                name: "LineConfiguration",
                schema: "Scheduling");

            migrationBuilder.DropTable(
                name: "LineProductAssignment",
                schema: "Scheduling");

            migrationBuilder.DropTable(
                name: "SchedulingAlert",
                schema: "Scheduling");

            migrationBuilder.DropTable(
                name: "SchedulingException",
                schema: "Scheduling");

            migrationBuilder.DropTable(
                name: "SchedulingRule",
                schema: "Scheduling");

            migrationBuilder.DropTable(
                name: "WeeklyPlanItem",
                schema: "Scheduling");

            migrationBuilder.DropTable(
                name: "WeeklyPlan",
                schema: "Scheduling");
        }
    }
}
