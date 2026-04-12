using System.Text;
using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Services.AdventureWorksAudit;

public static class WorkOrderAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(WorkOrder e) => new(e);

    public static WorkOrderAuditLog RecordCreate(WorkOrder e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static WorkOrderAuditLog RecordUpdate(Snapshot before, WorkOrder after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static WorkOrderAuditLog RecordDelete(WorkOrder e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static WorkOrderAuditLog BuildLog(WorkOrder e, string action, string? by, string? summary)
        => new()
        {
            WorkOrderId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            ProductId = e.ProductId,
            OrderQty = e.OrderQty,
            StockedQty = e.StockedQty,
            ScrappedQty = e.ScrappedQty,
            StartDate = e.StartDate,
            EndDate = e.EndDate,
            DueDate = e.DueDate,
            ScrapReasonId = e.ScrapReasonId,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, WorkOrder after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "ProductId", before.ProductId, after.ProductId);
        AuditDiffHelpers.AppendIfChanged(sb, "OrderQty", before.OrderQty, after.OrderQty);
        AuditDiffHelpers.AppendIfChanged(sb, "ScrappedQty", before.ScrappedQty, after.ScrappedQty);
        AuditDiffHelpers.AppendIfChanged(sb, "StartDate", before.StartDate, after.StartDate);
        AuditDiffHelpers.AppendIfChanged(sb, "EndDate", before.EndDate, after.EndDate);
        AuditDiffHelpers.AppendIfChanged(sb, "DueDate", before.DueDate, after.DueDate);
        AuditDiffHelpers.AppendIfChanged(sb, "ScrapReasonId", before.ScrapReasonId, after.ScrapReasonId);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        int ProductId, int OrderQty, short ScrappedQty,
        DateTime StartDate, DateTime? EndDate, DateTime DueDate, short? ScrapReasonId)
    {
        public Snapshot(WorkOrder e) : this(
            e.ProductId, e.OrderQty, e.ScrappedQty,
            e.StartDate, e.EndDate, e.DueDate, e.ScrapReasonId)
        { }
    }
}
