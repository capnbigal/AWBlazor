using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Engineering.Boms.Domain;

public class BomHeaderAuditLog : AdventureWorksAuditLogBase
{
    public int BomHeaderId { get; set; }

    [MaxLength(32)] public string? Code { get; set; }
    [MaxLength(200)] public string? Name { get; set; }
    [MaxLength(2000)] public string? Description { get; set; }
    public int ProductId { get; set; }
    public int RevisionNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
