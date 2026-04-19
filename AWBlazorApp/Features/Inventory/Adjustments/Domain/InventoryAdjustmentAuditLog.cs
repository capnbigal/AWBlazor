using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Inventory.Adjustments.Domain;

public class InventoryAdjustmentAuditLog : AdventureWorksAuditLogBase
{
    public int InventoryAdjustmentId { get; set; }

    [MaxLength(32)] public string? AdjustmentNumber { get; set; }
    public int InventoryItemId { get; set; }
    public int LocationId { get; set; }
    public int? LotId { get; set; }
    public decimal QuantityDelta { get; set; }
    public AdjustmentReason ReasonCode { get; set; }
    [MaxLength(500)] public string? Reason { get; set; }
    public AdjustmentStatus Status { get; set; }
    [MaxLength(450)] public string? RequestedByUserId { get; set; }
    public DateTime RequestedAt { get; set; }
    [MaxLength(450)] public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public long? PostedTransactionId { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
