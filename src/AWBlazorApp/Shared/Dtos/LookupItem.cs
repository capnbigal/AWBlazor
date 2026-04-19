namespace AWBlazorApp.Shared.Dtos;

public sealed record LookupItem<T>(T Id, string DisplayText);
