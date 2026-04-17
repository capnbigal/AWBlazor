using AWBlazorApp.Features.AdventureWorks.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Mes.Domain;

public class WorkInstructionAuditLog : AdventureWorksAuditLogBase
{
    public int WorkInstructionId { get; set; }

    public int WorkOrderRoutingId { get; set; }
    [MaxLength(200)] public string? Title { get; set; }
    public int? ActiveRevisionId { get; set; }
    public bool IsActive { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
