using System.Text;
using AWBlazorApp.Data.Entities.AdventureWorks;

namespace AWBlazorApp.Services.AdventureWorksAudit;

public static class TransactionHistoryAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(TransactionHistory e) => new(e);

    public static TransactionHistoryAuditLog RecordCreate(TransactionHistory e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static TransactionHistoryAuditLog RecordUpdate(Snapshot before, TransactionHistory after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static TransactionHistoryAuditLog RecordDelete(TransactionHistory e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static TransactionHistoryAuditLog BuildLog(TransactionHistory e, string action, string? by, string? summary)
        => new()
        {
            TransactionId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            ProductId = e.ProductId,
            ReferenceOrderId = e.ReferenceOrderId,
            ReferenceOrderLineId = e.ReferenceOrderLineId,
            TransactionDate = e.TransactionDate,
            TransactionType = e.TransactionType,
            Quantity = e.Quantity,
            ActualCost = e.ActualCost,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, TransactionHistory after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "ProductId", before.ProductId, after.ProductId);
        AuditDiffHelpers.AppendIfChanged(sb, "ReferenceOrderId", before.ReferenceOrderId, after.ReferenceOrderId);
        AuditDiffHelpers.AppendIfChanged(sb, "ReferenceOrderLineId", before.ReferenceOrderLineId, after.ReferenceOrderLineId);
        AuditDiffHelpers.AppendIfChanged(sb, "TransactionDate", before.TransactionDate, after.TransactionDate);
        AuditDiffHelpers.AppendIfChanged(sb, "TransactionType", before.TransactionType, after.TransactionType);
        AuditDiffHelpers.AppendIfChanged(sb, "Quantity", before.Quantity, after.Quantity);
        AuditDiffHelpers.AppendIfChanged(sb, "ActualCost", before.ActualCost, after.ActualCost);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        int ProductId, int ReferenceOrderId, int ReferenceOrderLineId,
        DateTime TransactionDate, string TransactionType, int Quantity, decimal ActualCost)
    {
        public Snapshot(TransactionHistory e) : this(
            e.ProductId, e.ReferenceOrderId, e.ReferenceOrderLineId,
            e.TransactionDate, e.TransactionType, e.Quantity, e.ActualCost)
        { }
    }
}
