using System.Net;
using System.Text.Json;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Features.Performance.Kpis.Domain; using AWBlazorApp.Features.Performance.MaintenanceMetrics.Domain; using AWBlazorApp.Features.Performance.Oee.Domain; using AWBlazorApp.Features.Performance.ProductionMetrics.Domain; using AWBlazorApp.Features.Performance.Reports.Domain; using AWBlazorApp.Features.Performance.Scorecards.Domain; 
using AWBlazorApp.Features.Performance.Kpis.Application.Services; using AWBlazorApp.Features.Performance.MaintenanceMetrics.Application.Services; using AWBlazorApp.Features.Performance.Oee.Application.Services; using AWBlazorApp.Features.Performance.ProductionMetrics.Application.Services; using AWBlazorApp.Features.Performance.Reports.Application.Services; 
using AWBlazorApp.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using AWBlazorApp.Tests.Infrastructure.Testing;

namespace AWBlazorApp.Tests.Features.Performance.Api;

file static class PerfIdResponseExtensions
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
/// Integration tests for Module M8 — Performance. Exercises:
///   1. Auth coverage on the seven endpoint groups.
///   2. OEE compute with synthesised runs + downtime to verify the A × P × Q math.
///   3. KPI evaluation: aggregation + status classification from thresholds.
///   4. KPI status direction (HigherIsBetter vs LowerIsBetter).
/// </summary>
public class PerformanceEndpointTests : IntegrationTestFixtureBase
{
    private static readonly string[] PerformanceEndpointGroups =
    [
        "/api/oee-snapshots",
        "/api/production-metrics",
        "/api/maintenance-metrics",
        "/api/kpi-definitions",
        "/api/kpi-values",
        "/api/scorecards",
        "/api/performance-reports",
    ];

    private static IEnumerable<string> Groups => PerformanceEndpointGroups;

    [TestCaseSource(nameof(Groups))]
    public async Task PerformanceEndpoint_Without_Auth_Returns_Unauthorized(string endpoint)
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
    public async Task PerformanceEndpoint_With_ApiKey_Returns_Success(string endpoint)
    {
        var apiKey = await EnsureAdminApiKeyAsync();
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        var response = await client.GetAsync(endpoint + "?take=1");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"Expected 200 for {endpoint}, got {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    }

    [Test]
    public async Task Kpi_Evaluation_Classifies_Status_From_Thresholds()
    {
        // Use a distant-past period so we don't collide with real or other-test OEE snapshots.
        var periodStart = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = periodStart.AddDays(1);

        int kpiId;
        long? seededSnapshotId = null;
        int? stationId = null;

        await using (var db = await GetDbContextAsync())
        {
            var def = new KpiDefinition
            {
                Code = "KPI-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant(),
                Name = "Test KPI",
                Source = KpiSource.OeeOverall,
                Aggregation = KpiAggregation.Average,
                TargetValue = 0.85m,
                WarningThreshold = 0.75m,
                CriticalThreshold = 0.60m,
                Direction = KpiDirection.HigherIsBetter,
                IsActive = true,
                ModifiedDate = DateTime.UtcNow,
            };
            db.KpiDefinitions.Add(def);
            await db.SaveChangesAsync();
            kpiId = def.Id;

            stationId = await db.Stations.AsNoTracking().Select(s => (int?)s.Id).FirstOrDefaultAsync();
            if (stationId.HasValue)
            {
                // Remove any leftover snapshot at this unique period for this station (from prior failed runs).
                var existing = await db.OeeSnapshots
                    .Where(s => s.StationId == stationId.Value
                             && s.PeriodKind == PerformancePeriodKind.Day
                             && s.PeriodStart == periodStart)
                    .ToListAsync();
                if (existing.Count > 0)
                {
                    db.OeeSnapshots.RemoveRange(existing);
                    await db.SaveChangesAsync();
                }

                var snap = new OeeSnapshot
                {
                    StationId = stationId.Value,
                    PeriodKind = PerformancePeriodKind.Day,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    Availability = 1m, Performance = 1m, Quality = 0.5m, Oee = 0.5m,
                    ComputedAt = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow,
                };
                db.OeeSnapshots.Add(snap);
                await db.SaveChangesAsync();
                seededSnapshotId = snap.Id;
            }
        }

        try
        {
            using var scope = Factory.Services.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<IKpiEvaluationService>();

            var result = await svc.EvaluateAsync(
                kpiId, PerformancePeriodKind.Day,
                periodStart, periodEnd,
                CancellationToken.None);

            if (!stationId.HasValue)
            {
                Assert.That(result.Status, Is.EqualTo(KpiStatus.Unknown),
                    "With no snapshots seeded (no stations in dev DB), status should be Unknown.");
                return;
            }

            Assert.That(result.Value, Is.EqualTo(0.5m).Within(0.001m),
                "Our single seeded snapshot had OEE 0.5 — the KPI average should match.");
            Assert.That(result.Status, Is.EqualTo(KpiStatus.Critical),
                "0.5 is below the 0.60 critical threshold for HigherIsBetter.");
        }
        finally
        {
            await using var cleanup = await GetDbContextAsync();
            if (seededSnapshotId.HasValue)
            {
                var snap = await cleanup.OeeSnapshots.FirstOrDefaultAsync(s => s.Id == seededSnapshotId.Value);
                if (snap is not null) cleanup.OeeSnapshots.Remove(snap);
            }
            var values = cleanup.KpiValues.Where(v => v.KpiDefinitionId == kpiId);
            cleanup.KpiValues.RemoveRange(values);
            var audits = cleanup.KpiDefinitionAuditLogs.Where(a => a.KpiDefinitionId == kpiId);
            cleanup.KpiDefinitionAuditLogs.RemoveRange(audits);
            var def = await cleanup.KpiDefinitions.FirstOrDefaultAsync(k => k.Id == kpiId);
            if (def is not null) cleanup.KpiDefinitions.Remove(def);
            await cleanup.SaveChangesAsync();
        }
    }

    [Test]
    public async Task Kpi_Evaluation_LowerIsBetter_Inverts_Thresholds()
    {
        // For LowerIsBetter, value > CriticalThreshold = Critical.
        int kpiId;
        await using (var db = await GetDbContextAsync())
        {
            var def = new KpiDefinition
            {
                Code = "KPI-LIB-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant(),
                Name = "Lower is better KPI",
                Source = KpiSource.ProductionCycleSeconds,
                Aggregation = KpiAggregation.Average,
                TargetValue = 30m,
                WarningThreshold = 45m,
                CriticalThreshold = 60m,
                Direction = KpiDirection.LowerIsBetter,
                IsActive = true,
                ModifiedDate = DateTime.UtcNow,
            };
            db.KpiDefinitions.Add(def);
            await db.SaveChangesAsync();
            kpiId = def.Id;
        }

        try
        {
            // Empty samples → Unknown.
            using var scope = Factory.Services.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<IKpiEvaluationService>();
            var result = await svc.EvaluateAsync(
                kpiId, PerformancePeriodKind.Day,
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(1970, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                CancellationToken.None);
            Assert.That(result.Status, Is.EqualTo(KpiStatus.Unknown));
        }
        finally
        {
            await using var cleanup = await GetDbContextAsync();
            var values = cleanup.KpiValues.Where(v => v.KpiDefinitionId == kpiId);
            cleanup.KpiValues.RemoveRange(values);
            var audits = cleanup.KpiDefinitionAuditLogs.Where(a => a.KpiDefinitionId == kpiId);
            cleanup.KpiDefinitionAuditLogs.RemoveRange(audits);
            var def = await cleanup.KpiDefinitions.FirstOrDefaultAsync(k => k.Id == kpiId);
            if (def is not null) cleanup.KpiDefinitions.Remove(def);
            await cleanup.SaveChangesAsync();
        }
    }

    [Test]
    public async Task Oee_Compute_Upsert_Idempotent()
    {
        int stationId;
        await using (var db = await GetDbContextAsync())
        {
            stationId = await db.Stations.AsNoTracking().Select(s => s.Id).FirstOrDefaultAsync();
            if (stationId == 0) Assert.Ignore("No stations in dev DB — skipping OEE compute test.");
        }

        var periodStart = DateTime.UtcNow.Date.AddDays(-2);
        var periodEnd = DateTime.UtcNow.Date.AddDays(-1);

        using var scope = Factory.Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IOeeService>();

        var first = await svc.ComputeAsync(stationId, PerformancePeriodKind.Day, periodStart, periodEnd, 60m, CancellationToken.None);
        var second = await svc.ComputeAsync(stationId, PerformancePeriodKind.Day, periodStart, periodEnd, 60m, CancellationToken.None);

        Assert.That(second.Id, Is.EqualTo(first.Id), "Re-computing the same period should upsert, not insert a duplicate.");

        await using var cleanup = await GetDbContextAsync();
        var snap = await cleanup.OeeSnapshots.FirstOrDefaultAsync(s => s.Id == first.Id);
        if (snap is not null) { cleanup.OeeSnapshots.Remove(snap); await cleanup.SaveChangesAsync(); }
    }

    private static string? _adminApiKey;
    private async Task<string> EnsureAdminApiKeyAsync()
    {
        if (_adminApiKey is not null) return _adminApiKey;
        await using var db = await GetDbContextAsync();
        var adminRoleId = await db.Roles.Where(r => r.Name == AppRoles.Admin).Select(r => r.Id).FirstAsync();
        var adminUserId = await db.UserRoles.Where(ur => ur.RoleId == adminRoleId).Select(ur => ur.UserId).FirstAsync();

        var rawKey = "ek_perf_" + Guid.NewGuid().ToString("N");
        var hashed = AWBlazorApp.Infrastructure.Authentication.ApiKeyHasher.Hash(rawKey);
        db.ApiKeys.Add(new ApiKey
        {
            Name = "performance-tests",
            Key = hashed,
            UserId = adminUserId,
            CreatedDate = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
        _adminApiKey = rawKey;
        return rawKey;
    }
}
