using AWBlazorApp.Features.AdventureWorks.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Inventory.Domain;

public class SerialUnitAuditLog : AdventureWorksAuditLogBase
{
    public int SerialUnitId { get; set; }

    public int InventoryItemId { get; set; }
    public int? LotId { get; set; }
    [MaxLength(128)] public string? SerialNumber { get; set; }
    public SerialUnitStatus Status { get; set; }
    public int? CurrentLocationId { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
