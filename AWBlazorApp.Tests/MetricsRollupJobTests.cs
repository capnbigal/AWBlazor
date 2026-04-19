using AWBlazorApp.Features.Admin.Services;
using AWBlazorApp.Features.Performance.Jobs;
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
        var result = await job.RunAsync(yesterday, idealCycleSeconds: 60m, CancellationToken.None);

        Assert.That(result.Date, Is.EqualTo(yesterday));
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
        var result = await job.RunAsync(firstOfMonth, idealCycleSeconds: 60m, CancellationToken.None);

        Assert.That(result.MaintenanceRollups, Is.GreaterThan(0), "Maintenance rollups should fire for every active asset on the 1st.");
        Assert.That(result.MaintenanceMonthRolledUp, Is.Not.Null, "MaintenanceMonthRolledUp should be set to YYYY-MM string.");
    }
}
