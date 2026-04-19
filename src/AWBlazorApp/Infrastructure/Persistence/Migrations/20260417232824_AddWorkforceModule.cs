using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AWBlazorApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkforceModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "wf");

            migrationBuilder.CreateTable(
                name: "Announcement",
                schema: "wf",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<byte>(type: "tinyint", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: true),
                    OrgUnitId = table.Column<int>(type: "int", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AuthoredByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Announcement_OrgUnit_OrgUnitId",
                        column: x => x.OrgUnitId,
                        principalSchema: "org",
                        principalTable: "OrgUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Announcement_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "org",
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AnnouncementAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnnouncementId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Severity = table.Column<byte>(type: "tinyint", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: true),
                    OrgUnitId = table.Column<int>(type: "int", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AuthoredByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnnouncementAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceEvent",
                schema: "wf",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessEntityId = table.Column<int>(type: "int", nullable: false),
                    ShiftId = table.Column<int>(type: "int", nullable: true),
                    ShiftDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ClockInAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClockOutAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceEvent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeQualificationAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeQualificationId = table.Column<int>(type: "int", nullable: false),
                    BusinessEntityId = table.Column<int>(type: "int", nullable: false),
                    QualificationId = table.Column<int>(type: "int", nullable: false),
                    EarnedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EvidenceUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VerifiedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeQualificationAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeaveRequest",
                schema: "wf",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessEntityId = table.Column<int>(type: "int", nullable: false),
                    LeaveType = table.Column<byte>(type: "tinyint", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RequestedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeaveRequestAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeaveRequestId = table.Column<int>(type: "int", nullable: false),
                    BusinessEntityId = table.Column<int>(type: "int", nullable: false),
                    LeaveType = table.Column<byte>(type: "tinyint", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RequestedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequestAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Qualification",
                schema: "wf",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Category = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Qualification", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QualificationAlertAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QualificationAlertId = table.Column<long>(type: "bigint", nullable: false),
                    BusinessEntityId = table.Column<int>(type: "int", nullable: false),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    QualificationId = table.Column<int>(type: "int", nullable: false),
                    OperatorClockEventId = table.Column<long>(type: "bigint", nullable: true),
                    Reason = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    RaisedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcknowledgedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualificationAlertAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QualificationAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QualificationId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Category = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualificationAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShiftHandoverNote",
                schema: "wf",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    ShiftDate = table.Column<DateOnly>(type: "date", nullable: false),
                    FromShiftId = table.Column<int>(type: "int", nullable: true),
                    ToShiftId = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    AuthoredByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AuthoredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequiresAcknowledgment = table.Column<bool>(type: "bit", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcknowledgedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftHandoverNote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShiftHandoverNote_Station_StationId",
                        column: x => x.StationId,
                        principalSchema: "org",
                        principalTable: "Station",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StationQualificationAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StationQualificationId = table.Column<int>(type: "int", nullable: false),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    QualificationId = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StationQualificationAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrainingCourse",
                schema: "wf",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    RecurrenceMonths = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingCourse", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrainingCourseAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrainingCourseId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    RecurrenceMonths = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingCourseAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeQualification",
                schema: "wf",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessEntityId = table.Column<int>(type: "int", nullable: false),
                    QualificationId = table.Column<int>(type: "int", nullable: false),
                    EarnedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EvidenceUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VerifiedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeQualification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeQualification_Qualification_QualificationId",
                        column: x => x.QualificationId,
                        principalSchema: "wf",
                        principalTable: "Qualification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QualificationAlert",
                schema: "wf",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessEntityId = table.Column<int>(type: "int", nullable: false),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    QualificationId = table.Column<int>(type: "int", nullable: false),
                    OperatorClockEventId = table.Column<long>(type: "bigint", nullable: true),
                    Reason = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    RaisedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcknowledgedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualificationAlert", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualificationAlert_Qualification_QualificationId",
                        column: x => x.QualificationId,
                        principalSchema: "wf",
                        principalTable: "Qualification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QualificationAlert_Station_StationId",
                        column: x => x.StationId,
                        principalSchema: "org",
                        principalTable: "Station",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StationQualification",
                schema: "wf",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    QualificationId = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StationQualification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StationQualification_Qualification_QualificationId",
                        column: x => x.QualificationId,
                        principalSchema: "wf",
                        principalTable: "Qualification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StationQualification_Station_StationId",
                        column: x => x.StationId,
                        principalSchema: "org",
                        principalTable: "Station",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingRecord",
                schema: "wf",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrainingCourseId = table.Column<int>(type: "int", nullable: false),
                    BusinessEntityId = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Score = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EvidenceUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RecordedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingRecord_TrainingCourse_TrainingCourseId",
                        column: x => x.TrainingCourseId,
                        principalSchema: "wf",
                        principalTable: "TrainingCourse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Announcement_OrganizationId_OrgUnitId_IsActive",
                schema: "wf",
                table: "Announcement",
                columns: new[] { "OrganizationId", "OrgUnitId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Announcement_OrgUnitId",
                schema: "wf",
                table: "Announcement",
                column: "OrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Announcement_PublishedAt",
                schema: "wf",
                table: "Announcement",
                column: "PublishedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementAuditLogs_AnnouncementId",
                table: "AnnouncementAuditLogs",
                column: "AnnouncementId");

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementAuditLogs_ChangedDate",
                table: "AnnouncementAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceEvent_BusinessEntityId_ShiftDate",
                schema: "wf",
                table: "AttendanceEvent",
                columns: new[] { "BusinessEntityId", "ShiftDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceEvent_ShiftDate",
                schema: "wf",
                table: "AttendanceEvent",
                column: "ShiftDate",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeQualification_BusinessEntityId_QualificationId",
                schema: "wf",
                table: "EmployeeQualification",
                columns: new[] { "BusinessEntityId", "QualificationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeQualification_ExpiresOn",
                schema: "wf",
                table: "EmployeeQualification",
                column: "ExpiresOn");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeQualification_QualificationId",
                schema: "wf",
                table: "EmployeeQualification",
                column: "QualificationId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeQualificationAuditLogs_ChangedDate",
                table: "EmployeeQualificationAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeQualificationAuditLogs_EmployeeQualificationId",
                table: "EmployeeQualificationAuditLogs",
                column: "EmployeeQualificationId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequest_BusinessEntityId",
                schema: "wf",
                table: "LeaveRequest",
                column: "BusinessEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequest_StartDate",
                schema: "wf",
                table: "LeaveRequest",
                column: "StartDate",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequest_Status",
                schema: "wf",
                table: "LeaveRequest",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequestAuditLogs_ChangedDate",
                table: "LeaveRequestAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequestAuditLogs_LeaveRequestId",
                table: "LeaveRequestAuditLogs",
                column: "LeaveRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Qualification_Code",
                schema: "wf",
                table: "Qualification",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QualificationAlert_BusinessEntityId",
                schema: "wf",
                table: "QualificationAlert",
                column: "BusinessEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_QualificationAlert_QualificationId",
                schema: "wf",
                table: "QualificationAlert",
                column: "QualificationId");

            migrationBuilder.CreateIndex(
                name: "IX_QualificationAlert_RaisedAt",
                schema: "wf",
                table: "QualificationAlert",
                column: "RaisedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_QualificationAlert_StationId",
                schema: "wf",
                table: "QualificationAlert",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_QualificationAlert_Status",
                schema: "wf",
                table: "QualificationAlert",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_QualificationAlertAuditLogs_ChangedDate",
                table: "QualificationAlertAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_QualificationAlertAuditLogs_QualificationAlertId",
                table: "QualificationAlertAuditLogs",
                column: "QualificationAlertId");

            migrationBuilder.CreateIndex(
                name: "IX_QualificationAuditLogs_ChangedDate",
                table: "QualificationAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_QualificationAuditLogs_QualificationId",
                table: "QualificationAuditLogs",
                column: "QualificationId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftHandoverNote_AuthoredAt",
                schema: "wf",
                table: "ShiftHandoverNote",
                column: "AuthoredAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ShiftHandoverNote_StationId_ShiftDate",
                schema: "wf",
                table: "ShiftHandoverNote",
                columns: new[] { "StationId", "ShiftDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StationQualification_QualificationId",
                schema: "wf",
                table: "StationQualification",
                column: "QualificationId");

            migrationBuilder.CreateIndex(
                name: "IX_StationQualification_StationId_QualificationId",
                schema: "wf",
                table: "StationQualification",
                columns: new[] { "StationId", "QualificationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StationQualificationAuditLogs_ChangedDate",
                table: "StationQualificationAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_StationQualificationAuditLogs_StationQualificationId",
                table: "StationQualificationAuditLogs",
                column: "StationQualificationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCourse_Code",
                schema: "wf",
                table: "TrainingCourse",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCourse_IsActive",
                schema: "wf",
                table: "TrainingCourse",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCourseAuditLogs_ChangedDate",
                table: "TrainingCourseAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCourseAuditLogs_TrainingCourseId",
                table: "TrainingCourseAuditLogs",
                column: "TrainingCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRecord_BusinessEntityId",
                schema: "wf",
                table: "TrainingRecord",
                column: "BusinessEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRecord_ExpiresOn",
                schema: "wf",
                table: "TrainingRecord",
                column: "ExpiresOn");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRecord_TrainingCourseId",
                schema: "wf",
                table: "TrainingRecord",
                column: "TrainingCourseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Announcement",
                schema: "wf");

            migrationBuilder.DropTable(
                name: "AnnouncementAuditLogs");

            migrationBuilder.DropTable(
                name: "AttendanceEvent",
                schema: "wf");

            migrationBuilder.DropTable(
                name: "EmployeeQualification",
                schema: "wf");

            migrationBuilder.DropTable(
                name: "EmployeeQualificationAuditLogs");

            migrationBuilder.DropTable(
                name: "LeaveRequest",
                schema: "wf");

            migrationBuilder.DropTable(
                name: "LeaveRequestAuditLogs");

            migrationBuilder.DropTable(
                name: "QualificationAlert",
                schema: "wf");

            migrationBuilder.DropTable(
                name: "QualificationAlertAuditLogs");

            migrationBuilder.DropTable(
                name: "QualificationAuditLogs");

            migrationBuilder.DropTable(
                name: "ShiftHandoverNote",
                schema: "wf");

            migrationBuilder.DropTable(
                name: "StationQualification",
                schema: "wf");

            migrationBuilder.DropTable(
                name: "StationQualificationAuditLogs");

            migrationBuilder.DropTable(
                name: "TrainingCourseAuditLogs");

            migrationBuilder.DropTable(
                name: "TrainingRecord",
                schema: "wf");

            migrationBuilder.DropTable(
                name: "Qualification",
                schema: "wf");

            migrationBuilder.DropTable(
                name: "TrainingCourse",
                schema: "wf");
        }
    }
}
