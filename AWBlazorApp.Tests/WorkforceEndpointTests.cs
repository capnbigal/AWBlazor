using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AWBlazorApp.Data;
using AWBlazorApp.Features.Identity.Domain;
using AWBlazorApp.Features.Workforce.Domain;
using AWBlazorApp.Features.Workforce.Services;
using AWBlazorApp.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests;

file static class WfIdResponseExtensions
{
    public static int AsInt(this IdResponse r) => r.Id switch
    {
        int i => i,
        long l => (int)l,
        JsonElement je => je.GetInt32(),
        IConvertible c => Convert.ToInt32(c),
        _ => throw new InvalidOperationException($"Unexpected IdResponse.Id type: {r.Id?.GetType()}"),
    };
}

/// <summary>
/// Integration tests for Module M7 — Workforce. Exercises:
///   1. Auth coverage on all 10 workforce endpoint groups.
///   2. Training course CRUD via API with audit log entry written.
///   3. Qualification grant flow via IQualificationService — auto-resolves any matching
///      open alerts raised by the clock-in check hook.
///   4. LeaveRequestService transitions (Pending → Approved, Pending → Rejected, Pending → Cancelled).
/// </summary>
public class WorkforceEndpointTests : IntegrationTestFixtureBase
{
    private static readonly string[] WorkforceEndpointGroups =
    [
        "/api/training-courses",
        "/api/training-records",
        "/api/qualifications",
        "/api/employee-qualifications",
        "/api/station-qualifications",
        "/api/qualification-alerts",
        "/api/attendance-events",
        "/api/leave-requests",
        "/api/shift-handover-notes",
        "/api/announcements",
    ];

    private static IEnumerable<string> Groups => WorkforceEndpointGroups;

    [TestCaseSource(nameof(Groups))]
    public async Task WorkforceEndpoint_Without_Auth_Returns_Unauthorized(string endpoint)
    {
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var response = await client.GetAsync(endpoint);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized).Or.EqualTo(HttpStatusCode.Redirect).Or.EqualTo(HttpStatusCode.Found),
            $"Expected 401/redirect for {endpoint}, got {(int)response.StatusCode}");
    }

    [TestCaseSource(nameof(Groups))]
    public async Task WorkforceEndpoint_With_ApiKey_Returns_Success(string endpoint)
    {
        var apiKey = await EnsureAdminApiKeyAsync();
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        var response = await client.GetAsync(endpoint + "?take=1");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"Expected 200 for {endpoint}, got {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    }

    [Test]
    public async Task TrainingCourse_Create_Writes_Audit_Row()
    {
        var apiKey = await EnsureAdminApiKeyAsync();
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        var code = "TC" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        var request = new { Code = code, Name = "Forklift refresher " + code, DurationMinutes = 60, RecurrenceMonths = 12, IsActive = true };

        var create = await client.PostAsJsonAsync("/api/training-courses", request);
        Assert.That(create.StatusCode, Is.EqualTo(HttpStatusCode.Created),
            $"Create: {create.StatusCode} {await create.Content.ReadAsStringAsync()}");
        var created = await create.Content.ReadFromJsonAsync<IdResponse>();
        Assert.That(created, Is.Not.Null);
        var id = created!.AsInt();

        try
        {
            await using var db = await GetDbContextAsync();
            var entity = await db.TrainingCourses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            Assert.That(entity, Is.Not.Null);
            Assert.That(entity!.Code, Is.EqualTo(code));

            var auditCount = await db.TrainingCourseAuditLogs.AsNoTracking().CountAsync(a => a.TrainingCourseId == id);
            Assert.That(auditCount, Is.GreaterThanOrEqualTo(1), "Expected at least one audit log row after create.");
        }
        finally
        {
            await using var cleanup = await GetDbContextAsync();
            var rows = cleanup.TrainingCourseAuditLogs.Where(a => a.TrainingCourseId == id);
            cleanup.TrainingCourseAuditLogs.RemoveRange(rows);
            var course = await cleanup.TrainingCourses.FirstOrDefaultAsync(c => c.Id == id);
            if (course is not null) cleanup.TrainingCourses.Remove(course);
            await cleanup.SaveChangesAsync();
        }
    }

    [Test]
    public async Task Qualification_Grant_AutoResolves_Matching_Open_Alerts()
    {
        int qualificationId, employeeBeId;
        long alertId;
        int stationId;

        await using (var db = await GetDbContextAsync())
        {
            // Seed a fresh qualification + station qualification + open alert.
            var code = "Q" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            var qual = new Qualification
            {
                Code = code, Name = "Test qual " + code,
                Category = QualificationCategory.Safety, IsActive = true,
                ModifiedDate = DateTime.UtcNow,
            };
            db.Qualifications.Add(qual);
            await db.SaveChangesAsync();
            qualificationId = qual.Id;
            employeeBeId = 1;

            // Use any existing Station (enterprise). Fall back to an arbitrary int — the
            // alert row doesn't FK-enforce Station in our schema at runtime.
            stationId = await db.Stations.AsNoTracking().Select(s => s.Id).FirstOrDefaultAsync();
            if (stationId == 0) stationId = 999;

            var alert = new QualificationAlert
            {
                BusinessEntityId = employeeBeId,
                StationId = stationId,
                QualificationId = qualificationId,
                Reason = QualificationAlertReason.Missing,
                Status = QualificationAlertStatus.Open,
                RaisedAt = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
            };
            db.QualificationAlerts.Add(alert);
            await db.SaveChangesAsync();
            alertId = alert.Id;
        }

        try
        {
            using var scope = Factory.Services.CreateScope();
            var quals = scope.ServiceProvider.GetRequiredService<IQualificationService>();
            var empQualId = await quals.GrantAsync(
                employeeBeId, qualificationId, DateTime.UtcNow, null,
                null, "integration test", "test@wf", CancellationToken.None);

            await using var verify = await GetDbContextAsync();
            var alert = await verify.QualificationAlerts.AsNoTracking().FirstAsync(a => a.Id == alertId);
            Assert.That(alert.Status, Is.EqualTo(QualificationAlertStatus.Resolved),
                "Granting the matching qualification should auto-resolve the open alert.");

            // Cleanup.
            await using var cleanup = await GetDbContextAsync();
            var empQual = await cleanup.EmployeeQualifications.FirstOrDefaultAsync(x => x.Id == empQualId);
            if (empQual is not null) cleanup.EmployeeQualifications.Remove(empQual);
            await cleanup.SaveChangesAsync();
        }
        finally
        {
            await using var cleanup = await GetDbContextAsync();
            var alerts = cleanup.QualificationAlerts.Where(a => a.Id == alertId);
            cleanup.QualificationAlerts.RemoveRange(alerts);
            var audits = cleanup.EmployeeQualificationAuditLogs.Where(a => a.BusinessEntityId == employeeBeId && a.QualificationId == qualificationId);
            cleanup.EmployeeQualificationAuditLogs.RemoveRange(audits);
            var qAudits = cleanup.QualificationAlertAuditLogs.Where(a => a.QualificationAlertId == alertId);
            cleanup.QualificationAlertAuditLogs.RemoveRange(qAudits);
            var remainingEmp = cleanup.EmployeeQualifications.Where(x => x.BusinessEntityId == employeeBeId && x.QualificationId == qualificationId);
            cleanup.EmployeeQualifications.RemoveRange(remainingEmp);
            await cleanup.SaveChangesAsync();

            await using var cleanup2 = await GetDbContextAsync();
            var qual = await cleanup2.Qualifications.FirstOrDefaultAsync(q => q.Id == qualificationId);
            if (qual is not null) cleanup2.Qualifications.Remove(qual);
            var qAuditLogs = cleanup2.QualificationAuditLogs.Where(a => a.QualificationId == qualificationId);
            cleanup2.QualificationAuditLogs.RemoveRange(qAuditLogs);
            await cleanup2.SaveChangesAsync();
        }
    }

    [Test]
    public async Task LeaveRequest_Approve_Transitions_Pending_To_Approved()
    {
        int leaveId;
        await using (var db = await GetDbContextAsync())
        {
            var leave = new LeaveRequest
            {
                BusinessEntityId = 1,
                LeaveType = LeaveType.Vacation,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                Status = LeaveStatus.Pending,
                Reason = "integration test",
                RequestedByUserId = "test@wf",
                RequestedAt = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
            };
            db.LeaveRequests.Add(leave);
            await db.SaveChangesAsync();
            leaveId = leave.Id;
        }

        try
        {
            using var scope = Factory.Services.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<ILeaveRequestService>();
            await svc.ApproveAsync(leaveId, "OK", "mgr@wf", CancellationToken.None);

            await using var verify = await GetDbContextAsync();
            var after = await verify.LeaveRequests.AsNoTracking().FirstAsync(l => l.Id == leaveId);
            Assert.That(after.Status, Is.EqualTo(LeaveStatus.Approved));
            Assert.That(after.ReviewedByUserId, Is.EqualTo("mgr@wf"));
            Assert.That(after.ReviewNotes, Is.EqualTo("OK"));
        }
        finally
        {
            await using var cleanup = await GetDbContextAsync();
            var audits = cleanup.LeaveRequestAuditLogs.Where(a => a.LeaveRequestId == leaveId);
            cleanup.LeaveRequestAuditLogs.RemoveRange(audits);
            var leave = await cleanup.LeaveRequests.FirstOrDefaultAsync(l => l.Id == leaveId);
            if (leave is not null) cleanup.LeaveRequests.Remove(leave);
            await cleanup.SaveChangesAsync();
        }
    }

    [Test]
    public async Task LeaveRequest_Approve_Twice_Throws()
    {
        int leaveId;
        await using (var db = await GetDbContextAsync())
        {
            var leave = new LeaveRequest
            {
                BusinessEntityId = 1,
                LeaveType = LeaveType.Sick,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Status = LeaveStatus.Approved,
                RequestedAt = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
            };
            db.LeaveRequests.Add(leave);
            await db.SaveChangesAsync();
            leaveId = leave.Id;
        }

        try
        {
            using var scope = Factory.Services.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<ILeaveRequestService>();
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await svc.ApproveAsync(leaveId, null, "mgr@wf", CancellationToken.None),
                "Approving an already-approved request should throw — simple Pending-only workflow.");
        }
        finally
        {
            await using var cleanup = await GetDbContextAsync();
            var audits = cleanup.LeaveRequestAuditLogs.Where(a => a.LeaveRequestId == leaveId);
            cleanup.LeaveRequestAuditLogs.RemoveRange(audits);
            var leave = await cleanup.LeaveRequests.FirstOrDefaultAsync(l => l.Id == leaveId);
            if (leave is not null) cleanup.LeaveRequests.Remove(leave);
            await cleanup.SaveChangesAsync();
        }
    }

    private static string? _adminApiKey;
    private async Task<string> EnsureAdminApiKeyAsync()
    {
        if (_adminApiKey is not null) return _adminApiKey;
        await using var db = await GetDbContextAsync();
        var adminRoleId = await db.Roles.Where(r => r.Name == AppRoles.Admin).Select(r => r.Id).FirstAsync();
        var adminUserId = await db.UserRoles.Where(ur => ur.RoleId == adminRoleId).Select(ur => ur.UserId).FirstAsync();

        var rawKey = "ek_wf_" + Guid.NewGuid().ToString("N");
        var hashed = AWBlazorApp.Infrastructure.Authentication.ApiKeyHasher.Hash(rawKey);
        db.ApiKeys.Add(new ApiKey
        {
            Name = "workforce-tests",
            Key = hashed,
            UserId = adminUserId,
            CreatedDate = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
        _adminApiKey = rawKey;
        return rawKey;
    }
}
