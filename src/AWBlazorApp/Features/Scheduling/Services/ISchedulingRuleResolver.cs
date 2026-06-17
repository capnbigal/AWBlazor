using AWBlazorApp.Features.Scheduling.Rules.Domain;

namespace AWBlazorApp.Features.Scheduling.Services;

public interface ISchedulingRuleResolver
{
    IEnumerable<SchedulingRule> Resolve(
        IEnumerable<SchedulingRule> rules,
        SchedulingEventType eventType,
        bool inFrozenWindow);
}
