using AWBlazorApp.Features.AdventureWorks.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Engineering.Domain;

public class ManufacturingRoutingAuditLog : AdventureWorksAuditLogBase
{
    public int ManufacturingRoutingId { get; set; }

    [MaxLength(32)] public string? Code { get; set; }
    [MaxLength(200)] public string? Name { get; set; }
    [MaxLength(2000)] public string? Description { get; set; }
    public int ProductId { get; set; }
    public int RevisionNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
