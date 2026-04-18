using AWBlazorApp.Features.AdventureWorks.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Engineering.Domain;

public class EngineeringChangeOrderAuditLog : AdventureWorksAuditLogBase
{
    public int EngineeringChangeOrderId { get; set; }

    [MaxLength(32)] public string? Code { get; set; }
    [MaxLength(200)] public string? Title { get; set; }
    [MaxLength(4000)] public string? Description { get; set; }
    public EcoStatus Status { get; set; }
    [MaxLength(450)] public string? RaisedByUserId { get; set; }
    public DateTime RaisedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
    [MaxLength(450)] public string? DecidedByUserId { get; set; }
    [MaxLength(2000)] public string? DecisionNotes { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
