namespace AWBlazorApp.Features.Processes.Timelines.Domain;

/// <summary>
/// One step in a ProcessChainDefinition.StepsJson array. Exactly one step per chain has
/// Role=Root; all others are Role=Child and specify how to join to their parent via ForeignKey.
/// </summary>
public sealed record ChainStep(
    string Entity,
    string Role,
    string? ParentEntity = null,
    string? ForeignKey = null)
{
    public const string RoleRoot = "Root";
    public const string RoleChild = "Child";
}
