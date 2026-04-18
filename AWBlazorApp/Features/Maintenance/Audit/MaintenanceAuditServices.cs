using AWBlazorApp.Features.AdventureWorks.Audit;
using AWBlazorApp.Features.Maintenance.Domain;
using System.Text;

namespace AWBlazorApp.Features.Maintenance.Audit;

public static class AssetMaintenanceProfileAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(AssetMaintenanceProfile e) => new(e);
    public static AssetMaintenanceProfileAuditLog RecordCreate(AssetMaintenanceProfile e, string? by) => Build(e, ActionCreated, by, "Created");
    public static AssetMaintenanceProfileAuditLog RecordUpdate(Snapshot b, AssetMaintenanceProfile a, string? by) => Build(a, ActionUpdated, by, Diff(b, a));
    public static AssetMaintenanceProfileAuditLog RecordDelete(AssetMaintenanceProfile e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static AssetMaintenanceProfileAuditLog Build(AssetMaintenanceProfile e, string action, string? by, string? summary) => new()
    {
        AssetMaintenanceProfileId = e.Id,
        Action = action, ChangedBy = by, ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        AssetId = e.AssetId, Criticality = e.Criticality,
        OwnerBusinessEntityId = e.OwnerBusinessEntityId,
        TargetMtbfHours = e.TargetMtbfHours, NextPmDueAt = e.NextPmDueAt,
        Notes = e.Notes, SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, AssetMaintenanceProfile a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Criticality", b.Criticality, a.Criticality);
        AuditDiffHelpers.AppendIfChanged(sb, "OwnerBusinessEntityId", b.OwnerBusinessEntityId, a.OwnerBusinessEntityId);
        AuditDiffHelpers.AppendIfChanged(sb, "TargetMtbfHours", b.TargetMtbfHours, a.TargetMtbfHours);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(AssetCriticality Criticality, int? OwnerBusinessEntityId, int? TargetMtbfHours)
    {
        public Snapshot(AssetMaintenanceProfile e) : this(e.Criticality, e.OwnerBusinessEntityId, e.TargetMtbfHours) { }
    }
}

public static class PmScheduleAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(PmSchedule e) => new(e);
    public static PmScheduleAuditLog RecordCreate(PmSchedule e, string? by) => Build(e, ActionCreated, by, "Created");
    public static PmScheduleAuditLog RecordUpdate(Snapshot b, PmSchedule a, string? by) => Build(a, ActionUpdated, by, Diff(b, a));
    public static PmScheduleAuditLog RecordDelete(PmSchedule e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static PmScheduleAuditLog Build(PmSchedule e, string action, string? by, string? summary) => new()
    {
        PmScheduleId = e.Id,
        Action = action, ChangedBy = by, ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        Code = e.Code, Name = e.Name, Description = e.Description,
        AssetId = e.AssetId, IntervalKind = e.IntervalKind, IntervalValue = e.IntervalValue,
        DefaultPriority = e.DefaultPriority, EstimatedMinutes = e.EstimatedMinutes,
        IsActive = e.IsActive,
        LastCompletedAt = e.LastCompletedAt, LastCompletedMeterValue = e.LastCompletedMeterValue,
        SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, PmSchedule a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", b.Name, a.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "IntervalKind", b.IntervalKind, a.IntervalKind);
        AuditDiffHelpers.AppendIfChanged(sb, "IntervalValue", b.IntervalValue, a.IntervalValue);
        AuditDiffHelpers.AppendIfChanged(sb, "IsActive", b.IsActive, a.IsActive);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name, PmIntervalKind IntervalKind, int IntervalValue, bool IsActive)
    {
        public Snapshot(PmSchedule e) : this(e.Name, e.IntervalKind, e.IntervalValue, e.IsActive) { }
    }
}

public static class MaintenanceWorkOrderAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(MaintenanceWorkOrder e) => new(e);
    public static MaintenanceWorkOrderAuditLog RecordCreate(MaintenanceWorkOrder e, string? by) => Build(e, ActionCreated, by, "Created");
    public static MaintenanceWorkOrderAuditLog RecordUpdate(Snapshot b, MaintenanceWorkOrder a, string? by) => Build(a, ActionUpdated, by, Diff(b, a));
    public static MaintenanceWorkOrderAuditLog RecordDelete(MaintenanceWorkOrder e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static MaintenanceWorkOrderAuditLog Build(MaintenanceWorkOrder e, string action, string? by, string? summary) => new()
    {
        MaintenanceWorkOrderId = e.Id,
        Action = action, ChangedBy = by, ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        WorkOrderNumber = e.WorkOrderNumber, Title = e.Title, Description = e.Description,
        AssetId = e.AssetId, Type = e.Type, Status = e.Status, Priority = e.Priority,
        PmScheduleId = e.PmScheduleId, ScheduledFor = e.ScheduledFor,
        AssignedBusinessEntityId = e.AssignedBusinessEntityId,
        StartedAt = e.StartedAt, CompletedAt = e.CompletedAt,
        HeldAt = e.HeldAt, CancelledAt = e.CancelledAt,
        CompletionNotes = e.CompletionNotes,
        RaisedByUserId = e.RaisedByUserId, RaisedAt = e.RaisedAt,
        CompletedMeterValue = e.CompletedMeterValue,
        SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, MaintenanceWorkOrder a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Status", b.Status, a.Status);
        AuditDiffHelpers.AppendIfChanged(sb, "Priority", b.Priority, a.Priority);
        AuditDiffHelpers.AppendIfChanged(sb, "AssignedBusinessEntityId", b.AssignedBusinessEntityId, a.AssignedBusinessEntityId);
        AuditDiffHelpers.AppendIfChanged(sb, "ScheduledFor", b.ScheduledFor, a.ScheduledFor);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(WorkOrderStatus Status, WorkOrderPriority Priority, int? AssignedBusinessEntityId, DateTime? ScheduledFor)
    {
        public Snapshot(MaintenanceWorkOrder e) : this(e.Status, e.Priority, e.AssignedBusinessEntityId, e.ScheduledFor) { }
    }
}

public static class SparePartAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(SparePart e) => new(e);
    public static SparePartAuditLog RecordCreate(SparePart e, string? by) => Build(e, ActionCreated, by, "Created");
    public static SparePartAuditLog RecordUpdate(Snapshot b, SparePart a, string? by) => Build(a, ActionUpdated, by, Diff(b, a));
    public static SparePartAuditLog RecordDelete(SparePart e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static SparePartAuditLog Build(SparePart e, string action, string? by, string? summary) => new()
    {
        SparePartId = e.Id,
        Action = action, ChangedBy = by, ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        PartNumber = e.PartNumber, Name = e.Name, Description = e.Description,
        ProductId = e.ProductId, UnitMeasureCode = e.UnitMeasureCode,
        StandardCost = e.StandardCost,
        ReorderPoint = e.ReorderPoint, ReorderQuantity = e.ReorderQuantity,
        IsActive = e.IsActive, SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, SparePart a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", b.Name, a.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "StandardCost", b.StandardCost, a.StandardCost);
        AuditDiffHelpers.AppendIfChanged(sb, "ReorderPoint", b.ReorderPoint, a.ReorderPoint);
        AuditDiffHelpers.AppendIfChanged(sb, "IsActive", b.IsActive, a.IsActive);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name, decimal? StandardCost, int? ReorderPoint, bool IsActive)
    {
        public Snapshot(SparePart e) : this(e.Name, e.StandardCost, e.ReorderPoint, e.IsActive) { }
    }
}
