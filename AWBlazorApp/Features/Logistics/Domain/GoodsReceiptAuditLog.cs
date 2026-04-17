using AWBlazorApp.Features.AdventureWorks.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Logistics.Domain;

public class GoodsReceiptAuditLog : AdventureWorksAuditLogBase
{
    public int GoodsReceiptId { get; set; }

    [MaxLength(32)] public string? ReceiptNumber { get; set; }
    public int? PurchaseOrderId { get; set; }
    public int? VendorBusinessEntityId { get; set; }
    public int ReceivedLocationId { get; set; }
    public GoodsReceiptStatus Status { get; set; }
    public DateTime ReceivedAt { get; set; }
    public DateTime? PostedAt { get; set; }
    [MaxLength(450)] public string? PostedByUserId { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
