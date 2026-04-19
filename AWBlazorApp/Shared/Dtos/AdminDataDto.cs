namespace AWBlazorApp.Shared.Dtos;

public sealed record PageStats(string Label, int Total);

public sealed record AdminDataResponse(IReadOnlyList<PageStats> PageStats);
