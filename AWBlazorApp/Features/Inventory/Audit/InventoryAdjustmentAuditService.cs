using AWBlazorApp.Features.AdventureWorks.Audit;
using AWBlazorApp.Features.Inventory.Domain;
using System.Text;

namespace AWBlazorApp.Features.Inventory.Audit;

public static class InventoryAdjustmentAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(InventoryAdjustment e) => new(e);

    public static InventoryAdjustmentAuditLog RecordCreate(InventoryAdjustment e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static InventoryAdjustmentAuditLog RecordUpdate(Snapshot before, InventoryAdjustment after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static InventoryAdjustmentAuditLog RecordDelete(InventoryAdjustment e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static InventoryAdjustmentAuditLog BuildLog(InventoryAdjustment e, string action, string? by, string? summary)
        => new()
        {
            InventoryAdjustmentId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            AdjustmentNumber = e.AdjustmentNumber,
            InventoryItemId = e.InventoryItemId,
            LocationId = e.LocationId,
            LotId = e.LotId,
            QuantityDelta = e.QuantityDelta,
            ReasonCode = e.ReasonCode,
            Reason = e.Reason,
            Status = e.Status,
            RequestedByUserId = e.RequestedByUserId,
            RequestedAt = e.RequestedAt,
            ApprovedByUserId = e.ApprovedByUserId,
            ApprovedAt = e.ApprovedAt,
            PostedTransactionId = e.PostedTransactionId,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot b, InventoryAdjustment a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "QuantityDelta", b.QuantityDelta, a.QuantityDelta);
        AuditDiffHelpers.AppendIfChanged(sb, "ReasonCode", b.ReasonCode, a.ReasonCode);
        AuditDiffHelpers.AppendIfChanged(sb, "Reason", b.Reason, a.Reason);
        AuditDiffHelpers.AppendIfChanged(sb, "Status", b.Status, a.Status);
        AuditDiffHelpers.AppendIfChanged(sb, "ApprovedByUserId", b.ApprovedByUserId, a.ApprovedByUserId);
        AuditDiffHelpers.AppendIfChanged(sb, "PostedTransactionId", b.PostedTransactionId, a.PostedTransactionId);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        decimal QuantityDelta, AdjustmentReason ReasonCode, string? Reason, AdjustmentStatus Status,
        string? ApprovedByUserId, long? PostedTransactionId)
    {
        public Snapshot(InventoryAdjustment e) : this(
            e.QuantityDelta, e.ReasonCode, e.Reason, e.Status, e.ApprovedByUserId, e.PostedTransactionId) { }
    }
}
