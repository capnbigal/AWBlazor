using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Mes.Instructions.Domain;

public class WorkInstructionStepAuditLog : AdventureWorksAuditLogBase
{
    public int WorkInstructionStepId { get; set; }

    public int WorkInstructionRevisionId { get; set; }
    public int SequenceNumber { get; set; }
    [MaxLength(200)] public string? Title { get; set; }
    [MaxLength(500)] public string? AttachmentUrl { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
