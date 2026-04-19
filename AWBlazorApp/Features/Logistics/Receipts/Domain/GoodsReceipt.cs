using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Logistics.Receipts.Domain;

/// <summary>
/// Inbound receipt header — PO → ASN → Receipt → Putaway in four-walls terms. Stays Draft
/// while the receiver builds out lines; Approved → Posted posts one <c>RECEIPT</c>
/// inventory transaction per line via <c>ILogisticsPostingService</c>, which also auto-creates
/// any required <c>inv.Lot</c> rows when the item tracks lots.
/// </summary>
[Table("GoodsReceipt", Schema = "lgx")]
public class GoodsReceipt
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(32)]
    public string ReceiptNumber { get; set; } = string.Empty;

    /// <summary>FK → <c>Purchasing.PurchaseOrderHeader.PurchaseOrderID</c>. Nullable because a
    /// receipt may predate (or bypass) a formal PO — e.g. emergency inbound.</summary>
    public int? PurchaseOrderId { get; set; }

    /// <summary>FK → <c>Purchasing.Vendor.BusinessEntityID</c>. Snapshotted on creation so the
    /// grid can show the vendor name without a join even if the PO is later voided.</summary>
    public int? VendorBusinessEntityId { get; set; }

    public int ReceivedLocationId { get; set; }

    public GoodsReceiptStatus Status { get; set; } = GoodsReceiptStatus.Draft;

    public DateTime ReceivedAt { get; set; }

    public DateTime? PostedAt { get; set; }

    [MaxLength(450)]
    public string? PostedByUserId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public enum GoodsReceiptStatus : byte
{
    Draft = 1,
    Approved = 2,
    Posted = 3,
    Cancelled = 4,
}
