using AWBlazorApp.Features.Processes.Timelines.Application;

namespace AWBlazorApp.Features.Processes.Timelines.Dtos;

public sealed record ChainInstanceSummaryDto(
    string ChainCode,
    string RootEntityId,
    string? RootLabel,
    DateTime FirstEventAt,
    DateTime LastEventAt,
    int EventCount,
    IReadOnlyList<string> ContributorUsers);

public static class ChainInstanceSummaryMappings
{
    public static ChainInstanceSummaryDto ToDto(this ChainInstanceSummary s) =>
        new(s.ChainCode, s.RootEntityId, s.RootLabel,
            s.FirstEventAt, s.LastEventAt, s.EventCount, s.ContributorUsers);
}
