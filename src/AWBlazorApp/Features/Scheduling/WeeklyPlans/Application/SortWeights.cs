namespace AWBlazorApp.Features.Scheduling.WeeklyPlans.Application;

public sealed record SortWeights(
    int DueDateRank = 10000,
    int CustomerPriorityRank = 1000,
    int ProductModelRank = 100,
    int TotalDueRank = 10,
    int ModifiedDateRank = 1);
