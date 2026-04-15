using System.Text;
using AWBlazorApp.Features.AdventureWorks.Domain;

namespace AWBlazorApp.Features.AdventureWorks.Audit;

public static class WorkOrderRoutingAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(WorkOrderRouting e) => new(e);

    public static WorkOrderRoutingAuditLog RecordCreate(WorkOrderRouting e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static WorkOrderRoutingAuditLog RecordUpdate(Snapshot before, WorkOrderRouting after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static WorkOrderRoutingAuditLog RecordDelete(WorkOrderRouting e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static WorkOrderRoutingAuditLog BuildLog(WorkOrderRouting e, string action, string? by, string? summary)
        => new()
        {
            WorkOrderId = e.WorkOrderId,
            ProductId = e.ProductId,
            OperationSequence = e.OperationSequence,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            LocationId = e.LocationId,
            ScheduledStartDate = e.ScheduledStartDate,
            ScheduledEndDate = e.ScheduledEndDate,
            ActualStartDate = e.ActualStartDate,
            ActualEndDate = e.ActualEndDate,
            ActualResourceHrs = e.ActualResourceHrs,
            PlannedCost = e.PlannedCost,
            ActualCost = e.ActualCost,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, WorkOrderRouting after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "LocationId", before.LocationId, after.LocationId);
        AuditDiffHelpers.AppendIfChanged(sb, "ScheduledStartDate", before.ScheduledStartDate, after.ScheduledStartDate);
        AuditDiffHelpers.AppendIfChanged(sb, "ScheduledEndDate", before.ScheduledEndDate, after.ScheduledEndDate);
        AuditDiffHelpers.AppendIfChanged(sb, "ActualStartDate", before.ActualStartDate, after.ActualStartDate);
        AuditDiffHelpers.AppendIfChanged(sb, "ActualEndDate", before.ActualEndDate, after.ActualEndDate);
        AuditDiffHelpers.AppendIfChanged(sb, "ActualResourceHrs", before.ActualResourceHrs, after.ActualResourceHrs);
        AuditDiffHelpers.AppendIfChanged(sb, "PlannedCost", before.PlannedCost, after.PlannedCost);
        AuditDiffHelpers.AppendIfChanged(sb, "ActualCost", before.ActualCost, after.ActualCost);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        short LocationId, DateTime ScheduledStartDate, DateTime ScheduledEndDate,
        DateTime? ActualStartDate, DateTime? ActualEndDate, decimal? ActualResourceHrs,
        decimal PlannedCost, decimal? ActualCost)
    {
        public Snapshot(WorkOrderRouting e) : this(
            e.LocationId, e.ScheduledStartDate, e.ScheduledEndDate,
            e.ActualStartDate, e.ActualEndDate, e.ActualResourceHrs,
            e.PlannedCost, e.ActualCost)
        { }
    }
}
