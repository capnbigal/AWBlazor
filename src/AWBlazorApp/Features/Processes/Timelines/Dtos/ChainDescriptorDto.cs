using AWBlazorApp.Features.Processes.Timelines.Domain;

namespace AWBlazorApp.Features.Processes.Timelines.Dtos;

public sealed record ChainDescriptorDto(string Code, string Name, string? Description);

public static class ChainDescriptorMappings
{
    public static ChainDescriptorDto ToDescriptor(this ProcessChainDefinition e) =>
        new(e.Code, e.Name, e.Description);
}
