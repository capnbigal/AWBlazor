using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Mes.Instructions.Domain;

public class WorkInstructionRevisionAuditLog : AdventureWorksAuditLogBase
{
    public int WorkInstructionRevisionId { get; set; }

    public int WorkInstructionId { get; set; }
    public int RevisionNumber { get; set; }
    public WorkInstructionRevisionStatus Status { get; set; }
    [MaxLength(450)] public string? CreatedByUserId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? PublishedAt { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
