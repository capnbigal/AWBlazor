using AWBlazorApp.Features.Scheduling.Rules.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Tests.Infrastructure.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Features.Scheduling.Seed;

public class SchedulingRuleSeedTests : IntegrationTestFixtureBase
{
    [Test]
    public async Task Three_Seed_Rules_Exist_After_Startup()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var rules = await db.SchedulingRules.AsNoTracking()
            .OrderBy(r => r.Priority).ThenBy(r => r.Id).ToListAsync();

        Assert.That(rules, Has.Count.GreaterThanOrEqualTo(3));
        Assert.That(rules.Any(r => r.EventType == SchedulingEventType.NewSO && !r.InFrozenWindow && r.Action == RecalcActionType.SoftResort), Is.True,
            "expected SoftResort rule for NewSO outside frozen window");
        Assert.That(rules.Any(r => r.EventType == SchedulingEventType.NewSO && r.InFrozenWindow && r.Action == RecalcActionType.AlertOnly), Is.True,
            "expected AlertOnly rule for NewSO inside frozen window");
        Assert.That(rules.Any(r => r.EventType == SchedulingEventType.NewSO && r.InFrozenWindow && r.Action == RecalcActionType.HardReplan), Is.True,
            "expected HardReplan fallback rule for NewSO inside frozen window");
    }
}
