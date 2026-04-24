namespace AWBlazorApp.Features.Processes.Timelines.Application;

public sealed record ChainInstanceSummary(
    string ChainCode,
    string RootEntityId,
    string? RootLabel,
    DateTime FirstEventAt,
    DateTime LastEventAt,
    int EventCount,
    IReadOnlyList<string> ContributorUsers);
