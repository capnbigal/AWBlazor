using AWBlazorApp.Features.Mes.Instructions.Dtos;
namespace AWBlazorApp.Features.Mes.Instructions.Application.Services;

/// <summary>
/// Owns the revision lifecycle for <c>mes.WorkInstruction</c>. <see cref="CreateNewRevisionAsync"/>
/// allocates the next RevisionNumber and copies all steps from the previous active revision
/// into new step rows tied to the new (Draft) revision. <see cref="PublishAsync"/> flips the
/// new revision to Published, supersedes the prior active one, and updates the header's
/// <c>ActiveRevisionId</c>.
/// </summary>
public interface IWorkInstructionRevisionService
{
    Task<int> CreateNewRevisionAsync(int workInstructionId, string? userId, CancellationToken cancellationToken);
    Task PublishAsync(int revisionId, string? userId, CancellationToken cancellationToken);
}
