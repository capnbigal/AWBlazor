using AWBlazorApp.Features.AdventureWorks.Audit;
using AWBlazorApp.Features.Mes.Domain;
using System.Text;

namespace AWBlazorApp.Features.Mes.Audit;

public static class ProductionRunAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(ProductionRun e) => new(e);
    public static ProductionRunAuditLog RecordCreate(ProductionRun e, string? by) => Build(e, ActionCreated, by, "Created");
    public static ProductionRunAuditLog RecordUpdate(Snapshot before, ProductionRun after, string? by) => Build(after, ActionUpdated, by, Diff(before, after));
    public static ProductionRunAuditLog RecordDelete(ProductionRun e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static ProductionRunAuditLog Build(ProductionRun e, string action, string? by, string? summary) => new()
    {
        ProductionRunId = e.Id,
        Action = action,
        ChangedBy = by,
        ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        RunNumber = e.RunNumber,
        Kind = e.Kind,
        WorkOrderId = e.WorkOrderId,
        StationId = e.StationId,
        AssetId = e.AssetId,
        Status = e.Status,
        PlannedStartAt = e.PlannedStartAt,
        ActualStartAt = e.ActualStartAt,
        ActualEndAt = e.ActualEndAt,
        QuantityPlanned = e.QuantityPlanned,
        QuantityProduced = e.QuantityProduced,
        QuantityScrapped = e.QuantityScrapped,
        Notes = e.Notes,
        PostedByUserId = e.PostedByUserId,
        SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, ProductionRun a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Kind", b.Kind, a.Kind);
        AuditDiffHelpers.AppendIfChanged(sb, "WorkOrderId", b.WorkOrderId, a.WorkOrderId);
        AuditDiffHelpers.AppendIfChanged(sb, "StationId", b.StationId, a.StationId);
        AuditDiffHelpers.AppendIfChanged(sb, "AssetId", b.AssetId, a.AssetId);
        AuditDiffHelpers.AppendIfChanged(sb, "Status", b.Status, a.Status);
        AuditDiffHelpers.AppendIfChanged(sb, "QuantityPlanned", b.QuantityPlanned, a.QuantityPlanned);
        AuditDiffHelpers.AppendIfChanged(sb, "QuantityProduced", b.QuantityProduced, a.QuantityProduced);
        AuditDiffHelpers.AppendIfChanged(sb, "QuantityScrapped", b.QuantityScrapped, a.QuantityScrapped);
        AuditDiffHelpers.AppendIfChanged(sb, "Notes", b.Notes, a.Notes);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        ProductionRunKind Kind, int? WorkOrderId, int? StationId, int? AssetId, ProductionRunStatus Status,
        decimal QuantityPlanned, decimal QuantityProduced, decimal QuantityScrapped, string? Notes)
    {
        public Snapshot(ProductionRun e) : this(
            e.Kind, e.WorkOrderId, e.StationId, e.AssetId, e.Status,
            e.QuantityPlanned, e.QuantityProduced, e.QuantityScrapped, e.Notes) { }
    }
}

public static class ProductionRunOperationAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(ProductionRunOperation e) => new(e);
    public static ProductionRunOperationAuditLog RecordCreate(ProductionRunOperation e, string? by) => Build(e, ActionCreated, by, "Created");
    public static ProductionRunOperationAuditLog RecordUpdate(Snapshot before, ProductionRunOperation after, string? by) => Build(after, ActionUpdated, by, Diff(before, after));
    public static ProductionRunOperationAuditLog RecordDelete(ProductionRunOperation e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static ProductionRunOperationAuditLog Build(ProductionRunOperation e, string action, string? by, string? summary) => new()
    {
        ProductionRunOperationId = e.Id,
        Action = action,
        ChangedBy = by,
        ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        ProductionRunId = e.ProductionRunId,
        OperationSequence = e.OperationSequence,
        SequenceNumber = e.SequenceNumber,
        OperationDescription = e.OperationDescription,
        StartAt = e.StartAt,
        EndAt = e.EndAt,
        ActualHours = e.ActualHours,
        SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, ProductionRunOperation a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "StartAt", b.StartAt, a.StartAt);
        AuditDiffHelpers.AppendIfChanged(sb, "EndAt", b.EndAt, a.EndAt);
        AuditDiffHelpers.AppendIfChanged(sb, "ActualHours", b.ActualHours, a.ActualHours);
        AuditDiffHelpers.AppendIfChanged(sb, "OperationDescription", b.OperationDescription, a.OperationDescription);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(DateTime? StartAt, DateTime? EndAt, decimal ActualHours, string OperationDescription)
    {
        public Snapshot(ProductionRunOperation e) : this(e.StartAt, e.EndAt, e.ActualHours, e.OperationDescription) { }
    }
}
