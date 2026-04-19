using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Engineering.Domain;

public class EngineeringDocumentAuditLog : AdventureWorksAuditLogBase
{
    public int EngineeringDocumentId { get; set; }

    [MaxLength(32)] public string? Code { get; set; }
    [MaxLength(200)] public string? Title { get; set; }
    public EngineeringDocumentKind Kind { get; set; }
    public int? ProductId { get; set; }
    public int RevisionNumber { get; set; }
    [MaxLength(1000)] public string? Url { get; set; }
    [MaxLength(2000)] public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
