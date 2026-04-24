namespace AWBlazorApp.Features.Processes.Timelines.Application;

public sealed record ProcessTimeline(
    ChainInstance Instance,
    IReadOnlyList<TimelineEvent> Events,
    bool Truncated);
