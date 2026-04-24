namespace AWBlazorApp.Features.Scheduling.WeeklyPlans.Application;

public sealed record WeeklyPlanGenerationOptions(
    bool StrictCapacity = false,
    bool DryRun = false,
    IReadOnlySet<byte>? PlannableSalesOrderStatuses = null, // default {1 InProcess, 2 Approved}
    SortWeights? SortWeights = null);
