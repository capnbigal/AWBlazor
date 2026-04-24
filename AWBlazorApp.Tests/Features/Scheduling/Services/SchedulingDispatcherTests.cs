using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
using AWBlazorApp.Features.Scheduling.Alerts.Domain;
using AWBlazorApp.Features.Scheduling.LineConfigurations.Domain;
using AWBlazorApp.Features.Scheduling.Services;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Tests.Infrastructure.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Features.Scheduling.Services;

public class SchedulingDispatcherTests : IntegrationTestFixtureBase
{
    private const short Loc = 60;
    private const int TestSoId = 999_995;

    [SetUp] public Task Before() => Cleanup();
    [TearDown] public Task After() => Cleanup();

    private async Task Cleanup()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.SchedulingAlerts.Where(a => a.SalesOrderId == TestSoId).ExecuteDeleteAsync();
    }

    [Test]
    public async Task Dispatch_OutsideFrozen_WritesSingleInfoAlert_viaSoftResort()
    {
        using var scope = Factory.Services.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<ISchedulingDispatcher>();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // ensure line config exists (may be seeded already by FrozenWindowEvaluatorTests)
        if (!await db.LineConfigurations.AnyAsync(l => l.LocationId == Loc))
        {
            db.LineConfigurations.Add(new LineConfiguration
            {
                LocationId = Loc, TaktSeconds = 600, ShiftsPerDay = 2, MinutesPerShift = 480,
                FrozenLookaheadHours = 72, IsActive = true, ModifiedDate = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var soh = new SalesOrderHeader { Id = TestSoId, DueDate = DateTime.UtcNow.AddDays(30), TotalDue = 100m };
        var before = await db.SchedulingAlerts.CountAsync(a => a.SalesOrderId == TestSoId);
        await sut.OnSalesOrderCreatedAsync(soh, Loc, db, CancellationToken.None);
        await db.SaveChangesAsync();
        var after = await db.SchedulingAlerts.CountAsync(a => a.SalesOrderId == TestSoId);

        Assert.That(after - before, Is.EqualTo(1));
        var alert = await db.SchedulingAlerts.Where(a => a.SalesOrderId == TestSoId)
            .OrderByDescending(a => a.Id).FirstAsync();
        Assert.That(alert.Severity, Is.EqualTo(AlertSeverity.Info));
    }
}
