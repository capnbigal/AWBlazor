using AWBlazorApp.Features.Processes.Timelines.Domain;

namespace AWBlazorApp.Features.Processes.Timelines.Application;

public sealed record ChainInstance(
    ProcessChainDefinition Definition,
    string RootEntityId,
    IReadOnlyDictionary<string, IReadOnlyList<string>> CollectedIds);
