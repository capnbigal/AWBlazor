namespace AWBlazorApp.Features.Scheduling.WeeklyPlans.Application;

public interface IWeeklyPlanGenerator
{
    Task<WeeklyPlanGenerationResult> GenerateAsync(
        int weekId, short locationId, WeeklyPlanGenerationOptions options,
        string requestedBy, CancellationToken ct);
}
