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

public class AlertOnlyActionTests : IntegrationTestFixtureBase
{
    [Test]
    public async Task Execute_WritesWarning_When_TotalDue_MeetsThreshold()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var action = new AlertOnlyAction();

        var soh = new SalesOrderHeader { Id = 999_998, DueDate = DateTime.UtcNow.AddHours(48), TotalDue = 5000m };
        var rule = new SchedulingRule
        {
            Action = RecalcActionType.AlertOnly, EventType = SchedulingEventType.NewSO,
            ParametersJson = "{\"minOrderValue\":5000}"
        };
        var ctx = new RecalcContext(db, rule, soh, 60, 202618, InFrozenWindow: true, DateTime.UtcNow);

        var result = await action.ExecuteAsync(ctx, CancellationToken.None);
        await db.SaveChangesAsync();

        Assert.That(result.Handled, Is.True);
        var alert = await db.SchedulingAlerts.OrderByDescending(a => a.Id).FirstAsync();
        Assert.That(alert.Severity, Is.EqualTo(AlertSeverity.Warning));
        Assert.That(alert.SalesOrderId, Is.EqualTo(999_998));

        db.SchedulingAlerts.Remove(alert);
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Execute_ReturnsHandledFalse_And_WritesNothing_When_Below_Threshold()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var action = new AlertOnlyAction();

        var soh = new SalesOrderHeader { Id = 999_997, DueDate = DateTime.UtcNow.AddHours(48), TotalDue = 100m };
        var rule = new SchedulingRule
        {
            Action = RecalcActionType.AlertOnly, EventType = SchedulingEventType.NewSO,
            ParametersJson = "{\"minOrderValue\":5000}"
        };
        var ctx = new RecalcContext(db, rule, soh, 60, 202618, InFrozenWindow: true, DateTime.UtcNow);

        var before = await db.SchedulingAlerts.CountAsync();
        var result = await action.ExecuteAsync(ctx, CancellationToken.None);
        await db.SaveChangesAsync();
        var after = await db.SchedulingAlerts.CountAsync();

        Assert.That(result.Handled, Is.False);
        Assert.That(after, Is.EqualTo(before));
    }
}
