using System.Net;
using System.Text.Json;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Features.Identity.Domain;
using AWBlazorApp.Features.Maintenance.AssetProfiles.Domain; using AWBlazorApp.Features.Maintenance.Logs.Domain; using AWBlazorApp.Features.Maintenance.MeterReadings.Domain; using AWBlazorApp.Features.Maintenance.PmSchedules.Domain; using AWBlazorApp.Features.Maintenance.SpareParts.Domain; using AWBlazorApp.Features.Maintenance.WorkOrders.Domain; 
using AWBlazorApp.Features.Maintenance.PmSchedules.Application.Services; using AWBlazorApp.Features.Maintenance.WorkOrders.Application.Services; 
using AWBlazorApp.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using AWBlazorApp.Tests.Infrastructure.Testing;

namespace AWBlazorApp.Tests.Features.Maintenance.Api;

file static class MaintIdResponseExtensions
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
/// Integration tests for Module M6 — Maintenance. Exercises:
///   1. Auth coverage on the six endpoint groups.
///   2. Work order state machine: full happy path Draft → Scheduled → InProgress → Completed.
///   3. Guard: can't approve/skip ahead in the state machine.
///   4. PmScheduleService.GenerateDueWorkOrdersAsync — time-based schedule with no prior
///      completion should generate exactly one WO, re-running should generate zero.
///   5. Meter-based PM: reading below threshold does not trigger; reading above threshold triggers.
/// </summary>
public class MaintenanceEndpointTests : IntegrationTestFixtureBase
{
    private static readonly string[] MaintenanceEndpointGroups =
    [
        "/api/asset-maintenance-profiles",
        "/api/pm-schedules",
        "/api/maintenance-work-orders",
        "/api/spare-parts",
        "/api/meter-readings",
        "/api/maintenance-logs",
    ];

    private static IEnumerable<string> Groups => MaintenanceEndpointGroups;

    [TestCaseSource(nameof(Groups))]
    public async Task MaintenanceEndpoint_Without_Auth_Returns_Unauthorized(string endpoint)
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
    public async Task MaintenanceEndpoint_With_ApiKey_Returns_Success(string endpoint)
    {
        var apiKey = await EnsureAdminApiKeyAsync();
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        var response = await client.GetAsync(endpoint + "?take=1");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"Expected 200 for {endpoint}, got {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    }

    [Test]
    public async Task WorkOrder_HappyPath_Draft_To_Completed()
    {
        var assetId = await EnsureTestAssetAsync();
        int woId;
        await using (var db = await GetDbContextAsync())
        {
            var wo = new MaintenanceWorkOrder
            {
                WorkOrderNumber = "WO-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant(),
                Title = "Happy path test",
                AssetId = assetId,
                Type = WorkOrderType.Corrective,
                Status = WorkOrderStatus.Draft,
                Priority = WorkOrderPriority.Medium,
                RaisedByUserId = "test@maint",
                RaisedAt = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
            };
            db.MaintenanceWorkOrders.Add(wo);
            await db.SaveChangesAsync();
            woId = wo.Id;
        }

        try
        {
            using var scope = Factory.Services.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<IWorkOrderService>();

            await svc.ScheduleAsync(woId, DateTime.UtcNow.AddHours(1), assigneeBusinessEntityId: 1, "test@maint", CancellationToken.None);
            await svc.StartAsync(woId, "test@maint", CancellationToken.None);
            await svc.CompleteAsync(woId, "all good", completedMeterValue: null, "test@maint", CancellationToken.None);

            await using var verify = await GetDbContextAsync();
            var after = await verify.MaintenanceWorkOrders.AsNoTracking().FirstAsync(w => w.Id == woId);
            Assert.That(after.Status, Is.EqualTo(WorkOrderStatus.Completed));
            Assert.That(after.StartedAt, Is.Not.Null);
            Assert.That(after.CompletedAt, Is.Not.Null);
            Assert.That(after.CompletionNotes, Is.EqualTo("all good"));
        }
        finally
        {
            await CleanupWorkOrderAsync(woId);
        }
    }

    [Test]
    public async Task WorkOrder_Cant_Complete_From_Draft()
    {
        var assetId = await EnsureTestAssetAsync();
        int woId;
        await using (var db = await GetDbContextAsync())
        {
            var wo = new MaintenanceWorkOrder
            {
                WorkOrderNumber = "WOG-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant(),
                Title = "Guard test",
                AssetId = assetId,
                Status = WorkOrderStatus.Draft,
                RaisedAt = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
            };
            db.MaintenanceWorkOrders.Add(wo);
            await db.SaveChangesAsync();
            woId = wo.Id;
        }

        try
        {
            using var scope = Factory.Services.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<IWorkOrderService>();
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await svc.CompleteAsync(woId, null, null, "test@maint", CancellationToken.None),
                "Completing a Draft WO should throw — must go through Scheduled + InProgress first.");
        }
        finally
        {
            await CleanupWorkOrderAsync(woId);
        }
    }

    [Test]
    public async Task PmSchedule_GenerateDue_TimeBased_Creates_WO_OnFirstRun_And_NoOp_On_Repeat()
    {
        var assetId = await EnsureTestAssetAsync();
        int scheduleId;
        await using (var db = await GetDbContextAsync())
        {
            var schedule = new PmSchedule
            {
                Code = "PM-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant(),
                Name = "Test schedule",
                AssetId = assetId,
                IntervalKind = PmIntervalKind.Days,
                IntervalValue = 30,
                DefaultPriority = WorkOrderPriority.Medium,
                EstimatedMinutes = 60,
                IsActive = true,
                ModifiedDate = DateTime.UtcNow,
            };
            db.PmSchedules.Add(schedule);
            await db.SaveChangesAsync();
            scheduleId = schedule.Id;
        }

        try
        {
            using var scope = Factory.Services.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<IPmScheduleService>();

            var first = await svc.GenerateDueWorkOrdersAsync(scheduleId, "test@maint", CancellationToken.None);
            Assert.That(first, Is.EqualTo(1), "First run with no LastCompletedAt should generate one WO.");

            var second = await svc.GenerateDueWorkOrdersAsync(scheduleId, "test@maint", CancellationToken.None);
            Assert.That(second, Is.EqualTo(0), "Second run with an open WO should be a no-op.");

            await using var verify = await GetDbContextAsync();
            var wos = await verify.MaintenanceWorkOrders.AsNoTracking()
                .Where(w => w.PmScheduleId == scheduleId).ToListAsync();
            Assert.That(wos.Count, Is.EqualTo(1));
            Assert.That(wos[0].Type, Is.EqualTo(WorkOrderType.Preventive));
        }
        finally
        {
            await CleanupPmScheduleAsync(scheduleId);
        }
    }

    [Test]
    public async Task PmSchedule_GenerateDue_MeterBased_Triggers_Only_When_Threshold_Reached()
    {
        var assetId = await EnsureTestAssetAsync();
        int scheduleId;
        await using (var db = await GetDbContextAsync())
        {
            var schedule = new PmSchedule
            {
                Code = "PMM-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant(),
                Name = "Meter test",
                AssetId = assetId,
                IntervalKind = PmIntervalKind.RuntimeHours,
                IntervalValue = 100,
                DefaultPriority = WorkOrderPriority.Medium,
                EstimatedMinutes = 60,
                IsActive = true,
                LastCompletedMeterValue = 500m,
                ModifiedDate = DateTime.UtcNow,
            };
            db.PmSchedules.Add(schedule);
            await db.SaveChangesAsync();
            scheduleId = schedule.Id;

            db.MeterReadings.Add(new MeterReading
            {
                AssetId = assetId,
                Kind = MeterKind.RuntimeHours,
                Value = 550m,
                RecordedAt = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        try
        {
            using var scope = Factory.Services.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<IPmScheduleService>();

            var belowThreshold = await svc.GenerateDueWorkOrdersAsync(scheduleId, "test@maint", CancellationToken.None);
            Assert.That(belowThreshold, Is.EqualTo(0), "Meter delta 50 < interval 100 should not trigger.");

            await using (var db = await GetDbContextAsync())
            {
                db.MeterReadings.Add(new MeterReading
                {
                    AssetId = assetId,
                    Kind = MeterKind.RuntimeHours,
                    Value = 650m,
                    RecordedAt = DateTime.UtcNow.AddMinutes(1),
                    ModifiedDate = DateTime.UtcNow,
                });
                await db.SaveChangesAsync();
            }

            var aboveThreshold = await svc.GenerateDueWorkOrdersAsync(scheduleId, "test@maint", CancellationToken.None);
            Assert.That(aboveThreshold, Is.EqualTo(1), "Meter delta 150 >= interval 100 should trigger.");
        }
        finally
        {
            await CleanupPmScheduleAsync(scheduleId);
            await using var cleanup = await GetDbContextAsync();
            var readings = cleanup.MeterReadings.Where(m => m.AssetId == assetId);
            cleanup.MeterReadings.RemoveRange(readings);
            await cleanup.SaveChangesAsync();
        }
    }

    private async Task<int> EnsureTestAssetAsync()
    {
        await using var db = await GetDbContextAsync();
        var existing = await db.Assets.AsNoTracking().Select(a => a.Id).FirstOrDefaultAsync();
        if (existing > 0) return existing;

        // No seed assets exist — create one for these tests. The M-test-asset is cleaned up
        // by not cleaning up (test data, conftest-style), but in practice AW has dozens of
        // enterprise fixtures so this branch rarely runs.
        var orgId = await db.Organizations.AsNoTracking().Select(o => o.Id).FirstOrDefaultAsync();
        if (orgId == 0)
        {
            var org = new AWBlazorApp.Features.Enterprise.Organizations.Domain.Organization
            {
                Code = "TESTORG-M",
                Name = "Test org for maintenance tests",
                ModifiedDate = DateTime.UtcNow,
            };
            db.Organizations.Add(org);
            await db.SaveChangesAsync();
            orgId = org.Id;
        }

        var asset = new AWBlazorApp.Features.Enterprise.Assets.Domain.Asset
        {
            AssetTag = "ASSET-M-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant(),
            Name = "Test asset for maintenance tests",
            OrganizationId = orgId,
            AssetType = AWBlazorApp.Features.Enterprise.Assets.Domain.AssetType.Machine,
            Status = AWBlazorApp.Features.Enterprise.Assets.Domain.AssetStatus.Active,
            ModifiedDate = DateTime.UtcNow,
        };
        db.Assets.Add(asset);
        await db.SaveChangesAsync();
        return asset.Id;
    }

    private async Task CleanupWorkOrderAsync(int woId)
    {
        await using var cleanup = await GetDbContextAsync();
        var audits = cleanup.MaintenanceWorkOrderAuditLogs.Where(a => a.MaintenanceWorkOrderId == woId);
        cleanup.MaintenanceWorkOrderAuditLogs.RemoveRange(audits);
        var wo = await cleanup.MaintenanceWorkOrders.FirstOrDefaultAsync(w => w.Id == woId);
        if (wo is not null) cleanup.MaintenanceWorkOrders.Remove(wo);
        await cleanup.SaveChangesAsync();
    }

    private async Task CleanupPmScheduleAsync(int scheduleId)
    {
        await using var cleanup = await GetDbContextAsync();
        // Cascade would handle tasks, but we also need to clean WOs generated from this schedule.
        var wos = cleanup.MaintenanceWorkOrders.Where(w => w.PmScheduleId == scheduleId);
        var woIds = await wos.Select(w => w.Id).ToListAsync();
        var woAudits = cleanup.MaintenanceWorkOrderAuditLogs.Where(a => woIds.Contains(a.MaintenanceWorkOrderId));
        cleanup.MaintenanceWorkOrderAuditLogs.RemoveRange(woAudits);
        var woTasks = cleanup.MaintenanceWorkOrderTasks.Where(t => woIds.Contains(t.MaintenanceWorkOrderId));
        cleanup.MaintenanceWorkOrderTasks.RemoveRange(woTasks);
        cleanup.MaintenanceWorkOrders.RemoveRange(wos);

        var schAudits = cleanup.PmScheduleAuditLogs.Where(a => a.PmScheduleId == scheduleId);
        cleanup.PmScheduleAuditLogs.RemoveRange(schAudits);
        var schTasks = cleanup.PmScheduleTasks.Where(t => t.PmScheduleId == scheduleId);
        cleanup.PmScheduleTasks.RemoveRange(schTasks);
        var schedule = await cleanup.PmSchedules.FirstOrDefaultAsync(s => s.Id == scheduleId);
        if (schedule is not null) cleanup.PmSchedules.Remove(schedule);
        await cleanup.SaveChangesAsync();
    }

    private static string? _adminApiKey;
    private async Task<string> EnsureAdminApiKeyAsync()
    {
        if (_adminApiKey is not null) return _adminApiKey;
        await using var db = await GetDbContextAsync();
        var adminRoleId = await db.Roles.Where(r => r.Name == AppRoles.Admin).Select(r => r.Id).FirstAsync();
        var adminUserId = await db.UserRoles.Where(ur => ur.RoleId == adminRoleId).Select(ur => ur.UserId).FirstAsync();

        var rawKey = "ek_maint_" + Guid.NewGuid().ToString("N");
        var hashed = AWBlazorApp.Infrastructure.Authentication.ApiKeyHasher.Hash(rawKey);
        db.ApiKeys.Add(new ApiKey
        {
            Name = "maintenance-tests",
            Key = hashed,
            UserId = adminUserId,
            CreatedDate = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
        _adminApiKey = rawKey;
        return rawKey;
    }
}
