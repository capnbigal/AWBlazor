using AWBlazorApp.Features.Inventory.Domain;

namespace AWBlazorApp.Features.Inventory.Models;

public sealed record InventoryAdjustmentDto(
    int Id, string AdjustmentNumber, int InventoryItemId, int LocationId, int? LotId,
    decimal QuantityDelta, AdjustmentReason ReasonCode, string? Reason, AdjustmentStatus Status,
    string? RequestedByUserId, DateTime RequestedAt, string? ApprovedByUserId, DateTime? ApprovedAt,
    long? PostedTransactionId, DateTime ModifiedDate);

public sealed record CreateInventoryAdjustmentRequest
{
    public int InventoryItemId { get; set; }
    public int LocationId { get; set; }
    public int? LotId { get; set; }
    public decimal QuantityDelta { get; set; }
    public AdjustmentReason ReasonCode { get; set; }
    public string? Reason { get; set; }
}

public sealed record UpdateInventoryAdjustmentRequest
{
    public decimal? QuantityDelta { get; set; }
    public AdjustmentReason? ReasonCode { get; set; }
    public string? Reason { get; set; }
    public AdjustmentStatus? Status { get; set; }
}

public sealed record InventoryAdjustmentAuditLogDto(
    int Id, int InventoryAdjustmentId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    string? AdjustmentNumber, int InventoryItemId, int LocationId, int? LotId,
    decimal QuantityDelta, AdjustmentReason ReasonCode, string? Reason, AdjustmentStatus Status,
    string? RequestedByUserId, DateTime RequestedAt, string? ApprovedByUserId, DateTime? ApprovedAt,
    long? PostedTransactionId, DateTime SourceModifiedDate);

public static class InventoryAdjustmentMappings
{
    public static InventoryAdjustmentDto ToDto(this InventoryAdjustment e) => new(
        e.Id, e.AdjustmentNumber, e.InventoryItemId, e.LocationId, e.LotId,
        e.QuantityDelta, e.ReasonCode, e.Reason, e.Status,
        e.RequestedByUserId, e.RequestedAt, e.ApprovedByUserId, e.ApprovedAt,
        e.PostedTransactionId, e.ModifiedDate);

    public static InventoryAdjustment ToEntity(this CreateInventoryAdjustmentRequest r, string? userId)
    {
        var now = DateTime.UtcNow;
        return new InventoryAdjustment
        {
            AdjustmentNumber = $"ADJ-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            InventoryItemId = r.InventoryItemId,
            LocationId = r.LocationId,
            LotId = r.LotId,
            QuantityDelta = r.QuantityDelta,
            ReasonCode = r.ReasonCode,
            Reason = r.Reason?.Trim(),
            Status = AdjustmentStatus.Draft,
            RequestedByUserId = userId,
            RequestedAt = now,
            ModifiedDate = now,
        };
    }

    public static void ApplyTo(this UpdateInventoryAdjustmentRequest r, InventoryAdjustment e)
    {
        if (r.QuantityDelta is not null) e.QuantityDelta = r.QuantityDelta.Value;
        if (r.ReasonCode is not null) e.ReasonCode = r.ReasonCode.Value;
        if (r.Reason is not null) e.Reason = r.Reason.Trim();
        if (r.Status is not null) e.Status = r.Status.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static InventoryAdjustmentAuditLogDto ToDto(this InventoryAdjustmentAuditLog a) => new(
        a.Id, a.InventoryAdjustmentId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.AdjustmentNumber, a.InventoryItemId, a.LocationId, a.LotId,
        a.QuantityDelta, a.ReasonCode, a.Reason, a.Status,
        a.RequestedByUserId, a.RequestedAt, a.ApprovedByUserId, a.ApprovedAt,
        a.PostedTransactionId, a.SourceModifiedDate);
}
