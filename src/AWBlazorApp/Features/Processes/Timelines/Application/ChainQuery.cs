namespace AWBlazorApp.Features.Processes.Timelines.Application;

public sealed record ChainQuery(
    string? ChainCode = null,
    string? Owner = null,
    DateTime? Since = null,
    DateTime? Until = null,
    int Limit = 100);
