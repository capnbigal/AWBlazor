using AWBlazorApp.Features.Scheduling.Rules.Domain;
using AWBlazorApp.Features.Scheduling.Services;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Features.Scheduling.Services;

public class SchedulingRuleResolverTests
{
    private static readonly List<SchedulingRule> Seeded = new()
    {
        new() { Id = 1, EventType = SchedulingEventType.NewSO, InFrozenWindow = false,
                Action = RecalcActionType.SoftResort, Priority = 100, IsActive = true },
        new() { Id = 2, EventType = SchedulingEventType.NewSO, InFrozenWindow = true,
                Action = RecalcActionType.AlertOnly, ParametersJson = "{\"minOrderValue\":5000}",
                Priority = 100, IsActive = true },
        new() { Id = 3, EventType = SchedulingEventType.NewSO, InFrozenWindow = true,
                Action = RecalcActionType.HardReplan, Priority = 50, IsActive = true },
        new() { Id = 4, EventType = SchedulingEventType.NewSO, InFrozenWindow = true,
                Action = RecalcActionType.AlertOnly, Priority = 10, IsActive = false }
    };

    [Test]
    public void Resolve_OutsideFrozen_ReturnsSoftResortOnly()
    {
        var sut = new SchedulingRuleResolver();
        var list = sut.Resolve(Seeded, SchedulingEventType.NewSO, inFrozenWindow: false).ToList();
        Assert.That(list, Has.Count.EqualTo(1));
        Assert.That(list[0].Action, Is.EqualTo(RecalcActionType.SoftResort));
    }

    [Test]
    public void Resolve_InsideFrozen_ReturnsActiveRules_InPriorityDescOrder()
    {
        var sut = new SchedulingRuleResolver();
        var list = sut.Resolve(Seeded, SchedulingEventType.NewSO, inFrozenWindow: true).ToList();
        Assert.That(list.Select(r => r.Id), Is.EqualTo(new[] { 2, 3 })); // 4 excluded (inactive)
    }
}
