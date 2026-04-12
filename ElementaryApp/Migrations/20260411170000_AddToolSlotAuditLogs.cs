using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElementaryApp.Migrations
{
    /// <inheritdoc />
    public partial class AddToolSlotAuditLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ToolSlotAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToolSlotConfigurationId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Family = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MtCode = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Destination = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Fcl1 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Fcl2 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Fcr1 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Ffl1 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Ffl2 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Ffr1 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Ffr2 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Ffr3 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Ffr4 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Rcl1 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Rcr1 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Rcr2 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Rfl1 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Rfr1 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Rfr2 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolSlotAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ToolSlotAuditLogs_ToolSlotConfigurationId",
                table: "ToolSlotAuditLogs",
                column: "ToolSlotConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolSlotAuditLogs_ChangedDate",
                table: "ToolSlotAuditLogs",
                column: "ChangedDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ToolSlotAuditLogs");
        }
    }
}
