using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Inventory.Domain;

public class LotAuditLog : AdventureWorksAuditLogBase
{
    public int LotId { get; set; }

    public int InventoryItemId { get; set; }
    [MaxLength(64)] public string? LotCode { get; set; }
    public DateTime? ManufacturedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public int? VendorBusinessEntityId { get; set; }
    public LotStatus Status { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
