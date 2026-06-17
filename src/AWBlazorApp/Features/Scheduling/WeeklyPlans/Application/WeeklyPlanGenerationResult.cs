namespace AWBlazorApp.Features.Scheduling.WeeklyPlans.Application;

public sealed record WeeklyPlanGenerationResult(
    int WeeklyPlanId,
    int Version,
    int ItemCount,
    int OverCapacityCount,
    decimal UtilizationPercent,
    IReadOnlyList<string> Warnings);
