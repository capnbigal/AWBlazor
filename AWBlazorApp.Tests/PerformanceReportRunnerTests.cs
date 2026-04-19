using AWBlazorApp.Features.Admin.Services;
using AWBlazorApp.Features.Performance.Kpis.Domain; using AWBlazorApp.Features.Performance.MaintenanceMetrics.Domain; using AWBlazorApp.Features.Performance.Oee.Domain; using AWBlazorApp.Features.Performance.ProductionMetrics.Domain; using AWBlazorApp.Features.Performance.Reports.Domain; using AWBlazorApp.Features.Performance.Scorecards.Domain; 
using AWBlazorApp.Features.Performance.Kpis.Application.Services; using AWBlazorApp.Features.Performance.MaintenanceMetrics.Application.Services; using AWBlazorApp.Features.Performance.Oee.Application.Services; using AWBlazorApp.Features.Performance.ProductionMetrics.Application.Services; using AWBlazorApp.Features.Performance.Reports.Application.Services; 
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests;

/// <summary>
/// Integration tests for the <see cref="PerformanceReportRunner"/> — verifies the runner
/// turns a saved report definition into actual rows, persists a run-history entry, advances
/// LastRunAt, and that the date-range presets resolve to the windows we promise users.
/// </summary>
public class PerformanceReportRunnerTests : IntegrationTestFixtureBase
{
    [Test]
    public void ResolveRange_Last7Days_Ends_At_Midnight_UTC_Today()
    {
        var now = new DateTime(2026, 4, 19, 13, 45, 0, DateTimeKind.Utc);
        var (start, end) = PerformanceReportRunner.ResolveRange(ReportRangePreset.Last7Days, now);

        Assert.That(end, Is.EqualTo(new DateTime(2026, 4, 19, 0, 0, 0, DateTimeKind.Utc)),
            "End is exclusive midnight today UTC.");
        Assert.That(start, Is.EqualTo(new DateTime(2026, 4, 12, 0, 0, 0, DateTimeKind.Utc)),
            "Start is 7 days before end.");
    }

    [Test]
    public void ResolveRange_LastMonth_Spans_The_Whole_Prior_Calendar_Month()
    {
        var now = new DateTime(2026, 4, 19, 12, 0, 0, DateTimeKind.Utc);
        var (start, end) = PerformanceReportRunner.ResolveRange(ReportRangePreset.LastMonth, now);

        Assert.That(start, Is.EqualTo(new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc)));
        Assert.That(end, Is.EqualTo(new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)));
    }

    [Test]
    public async Task RunAsync_OeeSummary_Returns_Per_Station_Aggregates()
    {
        // Make sure the demo seeder has run — it provisions OEE snapshots for the last 7 days.
        using var seedScope = Factory.Services.CreateScope();
        await seedScope.ServiceProvider.GetRequiredService<DemoDataSeeder>().SeedAllAsync(CancellationToken.None);

        using var scope = Factory.Services.CreateScope();
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        var runner = scope.ServiceProvider.GetRequiredService<IPerformanceReportRunner>();

        // Find (or create) the demo OEE-week report.
        await using var setup = await dbFactory.CreateDbContextAsync();
        var report = await setup.PerformanceReports.FirstOrDefaultAsync(r => r.Code == "DEMO-RPT-OEE-WEEK")
                  ?? throw new AssertionException("Demo OEE-week report missing — seeder didn't run.");

        var result = await runner.RunAsync(report.Id, runByUserId: "test-runner", CancellationToken.None);

        Assert.That(result.Kind, Is.EqualTo(PerformanceReportKind.OeeSummary));
        Assert.That(result.Columns.Count, Is.EqualTo(9), "OEE summary emits 9 columns (id/name/days/4 OEE pcts/2 unit sums).");
        Assert.That(result.Rows.Count, Is.GreaterThan(0), "Demo seed populates at least one station's OEE history.");

        // Schema check on a row.
        var row = result.Rows[0];
        Assert.That(row.Count, Is.EqualTo(9));
        Assert.That(row[3], Does.EndWith("%"), "Availability column is formatted as a percentage.");

        // Run history is recorded with row count + duration.
        await using var verify = await dbFactory.CreateDbContextAsync();
        var lastRun = await verify.PerformanceReportRuns
            .Where(r => r.PerformanceReportId == report.Id)
            .OrderByDescending(r => r.RunAt)
            .FirstAsync();
        Assert.That(lastRun.ErrorMessage, Is.Null);
        Assert.That(lastRun.RowCount, Is.EqualTo(result.Rows.Count));
        Assert.That(lastRun.RunByUserId, Is.EqualTo("test-runner"));

        var refreshed = await verify.PerformanceReports.FirstAsync(r => r.Id == report.Id);
        Assert.That(refreshed.LastRunAt, Is.Not.Null, "LastRunAt should advance on success.");
    }

    [Test]
    public async Task RunAsync_MaintenanceScorecard_Returns_Per_Asset_Aggregates()
    {
        using var seedScope = Factory.Services.CreateScope();
        await seedScope.ServiceProvider.GetRequiredService<DemoDataSeeder>().SeedAllAsync(CancellationToken.None);

        using var scope = Factory.Services.CreateScope();
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        var runner = scope.ServiceProvider.GetRequiredService<IPerformanceReportRunner>();

        await using var setup = await dbFactory.CreateDbContextAsync();
        var report = await setup.PerformanceReports.FirstOrDefaultAsync(r => r.Code == "DEMO-RPT-MAINT-SC")
                  ?? throw new AssertionException("Demo maintenance scorecard report missing.");

        var result = await runner.RunAsync(report.Id, runByUserId: null, CancellationToken.None);

        Assert.That(result.Kind, Is.EqualTo(PerformanceReportKind.MaintenanceScorecard));
        Assert.That(result.Columns.Count, Is.EqualTo(11), "Maintenance scorecard emits 11 columns.");
        // The seeder writes 3 months of MaintenanceMonthlyMetrics for the first asset, so the
        // LastMonth preset should return at most 1 row (last full prior month). It can also be
        // 0 if the test runs early enough that the prior month has no rows yet — accept both.
        Assert.That(result.Rows.Count, Is.LessThanOrEqualTo(1));
    }

    [Test]
    public async Task RunAsync_Records_Failure_Without_Advancing_LastRunAt()
    {
        using var scope = Factory.Services.CreateScope();
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        var runner = scope.ServiceProvider.GetRequiredService<IPerformanceReportRunner>();

        // Insert a Custom-kind report — runner deliberately throws NotSupportedException for it.
        await using var setup = await dbFactory.CreateDbContextAsync();
        var report = new PerformanceReport
        {
            Code = "TEST-CUSTOM-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant(),
            Name = "Test custom",
            Kind = PerformanceReportKind.Custom,
            RangePreset = ReportRangePreset.Last7Days,
            IsActive = true,
            ModifiedDate = DateTime.UtcNow,
        };
        setup.PerformanceReports.Add(report);
        await setup.SaveChangesAsync();

        Assert.ThrowsAsync<NotSupportedException>(async () =>
            await runner.RunAsync(report.Id, runByUserId: null, CancellationToken.None));

        await using var verify = await dbFactory.CreateDbContextAsync();
        var lastRun = await verify.PerformanceReportRuns
            .Where(r => r.PerformanceReportId == report.Id)
            .OrderByDescending(r => r.RunAt)
            .FirstAsync();
        Assert.That(lastRun.ErrorMessage, Is.Not.Null.And.Contains("not yet supported"));

        var refreshed = await verify.PerformanceReports.FirstAsync(r => r.Id == report.Id);
        Assert.That(refreshed.LastRunAt, Is.Null, "LastRunAt should NOT advance on failure.");

        // Cleanup.
        verify.PerformanceReports.Remove(refreshed);
        await verify.SaveChangesAsync();
    }
}
