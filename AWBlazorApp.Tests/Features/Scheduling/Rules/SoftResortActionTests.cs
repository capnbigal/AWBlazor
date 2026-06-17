using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
using AWBlazorApp.Features.Scheduling.Alerts.Domain;
using AWBlazorApp.Features.Scheduling.Rules.Application;
using AWBlazorApp.Features.Scheduling.Rules.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Tests.Infrastructure.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Features.Scheduling.Rules;

public class SoftResortActionTests : IntegrationTestFixtureBase
{
    [Test]
    public async Task Execute_Writes_Info_Alert_And_Returns_Handled()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var action = new SoftResortAction();

        var soh = new SalesOrderHeader { Id = 999_999, DueDate = DateTime.UtcNow.AddDays(10), TotalDue = 1000m };
        var rule = new SchedulingRule { Action = RecalcActionType.SoftResort, EventType = SchedulingEventType.NewSO };
        var ctx = new RecalcContext(db, rule, soh, LocationId: 60, WeekId: 202618,
            InFrozenWindow: false, NowUtc: DateTime.UtcNow);

        var before = await db.SchedulingAlerts.CountAsync();
        var result = await action.ExecuteAsync(ctx, CancellationToken.None);
        await db.SaveChangesAsync();
        var after = await db.SchedulingAlerts.CountAsync();

        Assert.That(result.Handled, Is.True);
        Assert.That(after - before, Is.EqualTo(1));

        var alert = await db.SchedulingAlerts.OrderByDescending(a => a.Id).FirstAsync();
        Assert.That(alert.Severity, Is.EqualTo(AlertSeverity.Info));
        Assert.That(alert.SalesOrderId, Is.EqualTo(999_999));

        // cleanup
        db.SchedulingAlerts.Remove(alert);
        await db.SaveChangesAsync();
    }
}
