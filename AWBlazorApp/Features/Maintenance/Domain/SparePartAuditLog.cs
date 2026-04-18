using AWBlazorApp.Features.AdventureWorks.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Maintenance.Domain;

public class SparePartAuditLog : AdventureWorksAuditLogBase
{
    public int SparePartId { get; set; }

    [MaxLength(32)] public string? PartNumber { get; set; }
    [MaxLength(200)] public string? Name { get; set; }
    [MaxLength(2000)] public string? Description { get; set; }
    public int? ProductId { get; set; }
    [MaxLength(3)] public string? UnitMeasureCode { get; set; }
    public decimal? StandardCost { get; set; }
    public int? ReorderPoint { get; set; }
    public int? ReorderQuantity { get; set; }
    public bool IsActive { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
