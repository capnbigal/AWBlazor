using AWBlazorApp.Features.Scheduling.Rules.Domain;

namespace AWBlazorApp.Features.Scheduling.Services;

public class SchedulingRuleResolver : ISchedulingRuleResolver
{
    public IEnumerable<SchedulingRule> Resolve(
        IEnumerable<SchedulingRule> rules,
        SchedulingEventType eventType,
        bool inFrozenWindow)
        => rules.Where(r => r.IsActive && r.EventType == eventType && r.InFrozenWindow == inFrozenWindow)
                .OrderByDescending(r => r.Priority)
                .ThenBy(r => r.Id);
}
