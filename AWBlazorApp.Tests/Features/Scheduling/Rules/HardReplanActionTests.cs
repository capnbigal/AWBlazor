using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
using AWBlazorApp.Features.Scheduling.Alerts.Domain;
using AWBlazorApp.Features.Scheduling.Rules.Application;
using AWBlazorApp.Features.Scheduling.Rules.Domain;
using AWBlazorApp.Features.Scheduling.WeeklyPlans.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Tests.Infrastructure.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Features.Scheduling.Rules;

public class HardReplanActionTests : IntegrationTestFixtureBase
{
    private const int TestWeekId = 202650;
    private const short TestLocation = 60;
    private const int TestSoId = 999_996;

    [SetUp]
    public async Task CleanupBefore()
    {
        // Crash-resilient: clear any residue from a prior failing run.
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.SchedulingAlerts.Where(a => a.SalesOrderId == TestSoId).ExecuteDeleteAsync();
        await db.WeeklyPlans.Where(p => p.WeekId == TestWeekId && p.LocationId == TestLocation).ExecuteDeleteAsync();
    }

    [TearDown]
    public Task CleanupAfter() => CleanupBefore();

    [Test]
    public async Task Execute_WritesCriticalAlert_And_SetsBaselineDiverged()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var plan = new WeeklyPlan
        {
            WeekId = TestWeekId, LocationId = TestLocation, Version = 1,
            PublishedAt = DateTime.UtcNow, PublishedBy = "test", BaselineDiverged = false
        };
        db.WeeklyPlans.Add(plan);
        await db.SaveChangesAsync();

        var action = new HardReplanAction();
        var soh = new SalesOrderHeader { Id = TestSoId, DueDate = DateTime.UtcNow.AddHours(24), TotalDue = 100m };
        var rule = new SchedulingRule { Action = RecalcActionType.HardReplan, EventType = SchedulingEventType.NewSO };
        var ctx = new RecalcContext(db, rule, soh, TestLocation, TestWeekId, InFrozenWindow: true, DateTime.UtcNow);

        var result = await action.ExecuteAsync(ctx, CancellationToken.None);
        await db.SaveChangesAsync();

        Assert.That(result.Handled, Is.True);

        var refreshed = await db.WeeklyPlans.AsNoTracking().SingleAsync(p => p.Id == plan.Id);
        Assert.That(refreshed.BaselineDiverged, Is.True);

        var alert = await db.SchedulingAlerts.OrderByDescending(a => a.Id).FirstAsync();
        Assert.That(alert.Severity, Is.EqualTo(AlertSeverity.Critical));
        Assert.That(alert.SalesOrderId, Is.EqualTo(TestSoId));
    }
}
