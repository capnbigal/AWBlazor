namespace AWBlazorApp.Features.Scheduling.Rules.Application;

public sealed record RecalcResult(bool Handled, string? Note = null);
