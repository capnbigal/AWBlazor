namespace AWBlazorApp.Features.Processes.Timelines.Application;

public interface IProcessTimelineComposer
{
    Task<ProcessTimeline> ComposeAsync(ChainInstance instance, CancellationToken ct);
}
