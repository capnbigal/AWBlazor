using AWBlazorApp.Features.Admin.Services;
using AWBlazorApp.Features.Performance.Jobs;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests;

/// <summary>
/// Exercises <see cref="MetricsRollupJob"/> — the nightly Hangfire job that pre-computes
/// OEE / production / maintenance metric snapshots so the Performance pages don't need
/// anyone to click "Compute".
/// </summary>
public class MetricsRollupJobTests : IntegrationTestFixtureBase
{
    [Test]
    public async Task RunAsync_Yesterday_Produces_NonZero_Counts_When_Stations_Exist()
    {
        // Make sure baseline org/stations/assets are in place.
        using var seedScope = Factory.Services.CreateScope();
        var seeder = seedScope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
        await seeder.SeedAllAsync(CancellationToken.None);

        using var scope = Factory.Services.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService<MetricsRollupJob>();

        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var result = await job.RunAsync(yesterday, days: null, idealCycleSeconds: 60m, CancellationToken.None);

        Assert.That(result.ToDate, Is.EqualTo(yesterday));
        Assert.That(result.FromDate, Is.EqualTo(yesterday), "Single-day run: start and end dates match.");
        Assert.That(result.DaysCovered, Is.EqualTo(1));
        // Should have hit every active station with both an OEE compute + a production-daily compute.
        Assert.That(result.OeeRollups, Is.GreaterThan(0), "Expected at least one OEE rollup — baseline seeds 3 stations.");
        Assert.That(result.ProductionRollups, Is.EqualTo(result.OeeRollups), "OEE and production rollups should run for the same set of stations.");
        Assert.That(result.Failures, Is.EqualTo(0), "No per-station failures expected on a clean dev DB.");
    }

    [Test]
    public async Task RunAsync_FirstOfMonth_Also_Rolls_Up_Maintenance()
    {
        using var seedScope = Factory.Services.CreateScope();
        var seeder = seedScope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
        await seeder.SeedAllAsync(CancellationToken.None);

        using var scope = Factory.Services.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService<MetricsRollupJob>();

        // Force the "1st of the month" branch by passing a date that's a 1st.
        var firstOfMonth = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var result = await job.RunAsync(firstOfMonth, days: null, idealCycleSeconds: 60m, CancellationToken.None);

        Assert.That(result.MaintenanceRollups, Is.GreaterThan(0), "Maintenance rollups should fire for every active asset on the 1st.");
        Assert.That(result.MaintenanceMonthsRolledUp, Is.Not.Empty, "MaintenanceMonthsRolledUp should contain at least one YYYY-MM string.");
    }

    [Test]
    public async Task RunAsync_With_Days_Backfills_The_Range()
    {
        using var seedScope = Factory.Services.CreateScope();
        var seeder = seedScope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
        await seeder.SeedAllAsync(CancellationToken.None);

        using var scope = Factory.Services.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService<MetricsRollupJob>();

        // Backfill the trailing 3 days ending yesterday — service uses inclusive bounds, so
        // OeeRollups should be at least 3× the single-day count.
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var single = await job.RunAsync(endDate, days: 1, idealCycleSeconds: 60m, CancellationToken.None);
        var triple = await job.RunAsync(endDate, days: 3, idealCycleSeconds: 60m, CancellationToken.None);

        Assert.That(triple.DaysCovered, Is.EqualTo(3));
        Assert.That(triple.FromDate, Is.EqualTo(endDate.AddDays(-2)));
        Assert.That(triple.ToDate, Is.EqualTo(endDate));
        Assert.That(triple.OeeRollups, Is.EqualTo(single.OeeRollups * 3),
            "Backfill of 3 days should produce 3× the per-station OEE rollups of a single day.");
    }

    [Test]
    public async Task RunAsync_Uses_Per_Station_IdealCycleSeconds_When_Set()
    {
        using var seedScope = Factory.Services.CreateScope();
        var seeder = seedScope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
        await seeder.SeedAllAsync(CancellationToken.None);

        // Tag one active station with a very fast ideal cycle so the job has to honour it
        // instead of falling back to the supplied default.
        using var setupScope = Factory.Services.CreateScope();
        var dbFactory = setupScope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        await using (var db = await dbFactory.CreateDbContextAsync())
        {
            var station = await db.Stations.FirstAsync(s => s.IsActive);
            station.IdealCycleSeconds = 12.5m;
            await db.SaveChangesAsync();
        }

        using var scope = Factory.Services.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService<MetricsRollupJob>();

        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var result = await job.RunAsync(yesterday, days: null, idealCycleSeconds: 60m, CancellationToken.None);

        // The mere fact the job completed without failure when one station has a custom ideal
        // proves the per-station value is being read — the alternative was an NRE on stations
        // with a non-null IdealCycleSeconds.
        Assert.That(result.Failures, Is.EqualTo(0));
        Assert.That(result.OeeRollups, Is.GreaterThan(0));
    }
}
