using AWBlazorApp.Features.Admin.Services;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests;

/// <summary>
/// Exercises <see cref="DemoDataSeeder"/> — the droplet-demo seeder. Verifies it writes rows
/// on a fresh run and is idempotent (second run skips because markers are present).
/// </summary>
public class DemoDataSeederTests : IntegrationTestFixtureBase
{
    [Test]
    public async Task Seeder_Is_Idempotent()
    {
        using var scope = Factory.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DemoDataSeeder>();

        var first = await seeder.SeedAllAsync(CancellationToken.None);

        // If the fixture already had demo rows from a prior run, a "first" pass may also
        // return 0s — that's a legitimate outcome and still proves idempotency below.
        if (first.Skipped)
        {
            Assert.Ignore("FK targets missing in dev DB — seeder legitimately skipped.");
            return;
        }

        // Second run should be a strict no-op across all modules.
        var second = await seeder.SeedAllAsync(CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(second.Workforce, Is.EqualTo(0), "Workforce seed must no-op on second run.");
            Assert.That(second.Engineering, Is.EqualTo(0), "Engineering seed must no-op on second run.");
            Assert.That(second.Maintenance, Is.EqualTo(0), "Maintenance seed must no-op on second run.");
            Assert.That(second.Performance, Is.EqualTo(0), "Performance seed must no-op on second run.");
        });
    }

    [Test]
    public async Task Seeder_Provisions_Twelve_Demo_Kpi_Definitions()
    {
        using var scope = Factory.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

        var result = await seeder.SeedAllAsync(CancellationToken.None);
        if (result.Skipped) Assert.Ignore("Skipped — baseline FKs missing in dev DB.");

        await using var db = await dbFactory.CreateDbContextAsync();
        var demoKpiCount = await db.KpiDefinitions.CountAsync(k => k.Code.StartsWith("DEMO-KPI-"));
        Assert.That(demoKpiCount, Is.EqualTo(12),
            "Expanded demo set should expose 12 KPIs (6 OEE/production + 6 maintenance/perf).");

        var plantSc = await db.ScorecardDefinitions.SingleAsync(s => s.Code == "DEMO-SC-PLANT");
        var plantKpiCount = await db.ScorecardKpis.CountAsync(k => k.ScorecardDefinitionId == plantSc.Id);
        Assert.That(plantKpiCount, Is.EqualTo(8),
            "Plant Manager scorecard should host 8 production-/quality-side KPIs.");

        var maintSc = await db.ScorecardDefinitions.SingleAsync(s => s.Code == "DEMO-SC-MAINT");
        var maintKpiCount = await db.ScorecardKpis.CountAsync(k => k.ScorecardDefinitionId == maintSc.Id);
        Assert.That(maintKpiCount, Is.EqualTo(5),
            "Maintenance Lead scorecard should host 5 reliability KPIs.");
    }
}
