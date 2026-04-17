using AWBlazorApp.Features.AdventureWorks.Audit;
using AWBlazorApp.Features.Quality.Domain;
using System.Text;

namespace AWBlazorApp.Features.Quality.Audit;

public static class InspectionPlanAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(InspectionPlan e) => new(e);
    public static InspectionPlanAuditLog RecordCreate(InspectionPlan e, string? by) => Build(e, ActionCreated, by, "Created");
    public static InspectionPlanAuditLog RecordUpdate(Snapshot before, InspectionPlan after, string? by) => Build(after, ActionUpdated, by, Diff(before, after));
    public static InspectionPlanAuditLog RecordDelete(InspectionPlan e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static InspectionPlanAuditLog Build(InspectionPlan e, string action, string? by, string? summary) => new()
    {
        InspectionPlanId = e.Id,
        Action = action,
        ChangedBy = by,
        ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        PlanCode = e.PlanCode,
        Name = e.Name,
        Description = e.Description,
        Scope = e.Scope,
        ProductId = e.ProductId,
        WorkOrderRoutingId = e.WorkOrderRoutingId,
        VendorBusinessEntityId = e.VendorBusinessEntityId,
        SamplingRule = e.SamplingRule,
        AutoTriggerOnReceipt = e.AutoTriggerOnReceipt,
        AutoTriggerOnShipment = e.AutoTriggerOnShipment,
        AutoTriggerOnProductionRun = e.AutoTriggerOnProductionRun,
        IsActive = e.IsActive,
        SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, InspectionPlan a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", b.Name, a.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "Scope", b.Scope, a.Scope);
        AuditDiffHelpers.AppendIfChanged(sb, "ProductId", b.ProductId, a.ProductId);
        AuditDiffHelpers.AppendIfChanged(sb, "VendorBusinessEntityId", b.VendorBusinessEntityId, a.VendorBusinessEntityId);
        AuditDiffHelpers.AppendIfChanged(sb, "AutoTriggerOnReceipt", b.AutoTriggerOnReceipt, a.AutoTriggerOnReceipt);
        AuditDiffHelpers.AppendIfChanged(sb, "AutoTriggerOnShipment", b.AutoTriggerOnShipment, a.AutoTriggerOnShipment);
        AuditDiffHelpers.AppendIfChanged(sb, "AutoTriggerOnProductionRun", b.AutoTriggerOnProductionRun, a.AutoTriggerOnProductionRun);
        AuditDiffHelpers.AppendIfChanged(sb, "IsActive", b.IsActive, a.IsActive);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        string Name, InspectionScope Scope, int? ProductId, int? VendorBusinessEntityId,
        bool AutoTriggerOnReceipt, bool AutoTriggerOnShipment, bool AutoTriggerOnProductionRun, bool IsActive)
    {
        public Snapshot(InspectionPlan e) : this(e.Name, e.Scope, e.ProductId, e.VendorBusinessEntityId,
            e.AutoTriggerOnReceipt, e.AutoTriggerOnShipment, e.AutoTriggerOnProductionRun, e.IsActive) { }
    }
}

public static class InspectionPlanCharacteristicAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(InspectionPlanCharacteristic e) => new(e);
    public static InspectionPlanCharacteristicAuditLog RecordCreate(InspectionPlanCharacteristic e, string? by) => Build(e, ActionCreated, by, "Created");
    public static InspectionPlanCharacteristicAuditLog RecordUpdate(Snapshot before, InspectionPlanCharacteristic after, string? by) => Build(after, ActionUpdated, by, Diff(before, after));
    public static InspectionPlanCharacteristicAuditLog RecordDelete(InspectionPlanCharacteristic e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static InspectionPlanCharacteristicAuditLog Build(InspectionPlanCharacteristic e, string action, string? by, string? summary) => new()
    {
        InspectionPlanCharacteristicId = e.Id,
        Action = action,
        ChangedBy = by,
        ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        InspectionPlanId = e.InspectionPlanId,
        SequenceNumber = e.SequenceNumber,
        Name = e.Name,
        Kind = e.Kind,
        MinValue = e.MinValue,
        MaxValue = e.MaxValue,
        TargetValue = e.TargetValue,
        UnitMeasureCode = e.UnitMeasureCode,
        ExpectedValue = e.ExpectedValue,
        IsCritical = e.IsCritical,
        SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, InspectionPlanCharacteristic a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", b.Name, a.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "MinValue", b.MinValue, a.MinValue);
        AuditDiffHelpers.AppendIfChanged(sb, "MaxValue", b.MaxValue, a.MaxValue);
        AuditDiffHelpers.AppendIfChanged(sb, "TargetValue", b.TargetValue, a.TargetValue);
        AuditDiffHelpers.AppendIfChanged(sb, "ExpectedValue", b.ExpectedValue, a.ExpectedValue);
        AuditDiffHelpers.AppendIfChanged(sb, "IsCritical", b.IsCritical, a.IsCritical);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name, decimal? MinValue, decimal? MaxValue, decimal? TargetValue, string? ExpectedValue, bool IsCritical)
    {
        public Snapshot(InspectionPlanCharacteristic e) : this(e.Name, e.MinValue, e.MaxValue, e.TargetValue, e.ExpectedValue, e.IsCritical) { }
    }
}

public static class InspectionAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(Inspection e) => new(e);
    public static InspectionAuditLog RecordCreate(Inspection e, string? by) => Build(e, ActionCreated, by, "Created");
    public static InspectionAuditLog RecordUpdate(Snapshot before, Inspection after, string? by) => Build(after, ActionUpdated, by, Diff(before, after));
    public static InspectionAuditLog RecordDelete(Inspection e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static InspectionAuditLog Build(Inspection e, string action, string? by, string? summary) => new()
    {
        InspectionId = e.Id,
        Action = action,
        ChangedBy = by,
        ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        InspectionNumber = e.InspectionNumber,
        InspectionPlanId = e.InspectionPlanId,
        Status = e.Status,
        SourceKind = e.SourceKind,
        SourceId = e.SourceId,
        InspectorBusinessEntityId = e.InspectorBusinessEntityId,
        InspectedAt = e.InspectedAt,
        InventoryItemId = e.InventoryItemId,
        LotId = e.LotId,
        Quantity = e.Quantity,
        UnitMeasureCode = e.UnitMeasureCode,
        Notes = e.Notes,
        PostedByUserId = e.PostedByUserId,
        SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, Inspection a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Status", b.Status, a.Status);
        AuditDiffHelpers.AppendIfChanged(sb, "InspectorBusinessEntityId", b.InspectorBusinessEntityId, a.InspectorBusinessEntityId);
        AuditDiffHelpers.AppendIfChanged(sb, "InspectedAt", b.InspectedAt, a.InspectedAt);
        AuditDiffHelpers.AppendIfChanged(sb, "Notes", b.Notes, a.Notes);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(InspectionStatus Status, int? InspectorBusinessEntityId, DateTime? InspectedAt, string? Notes)
    {
        public Snapshot(Inspection e) : this(e.Status, e.InspectorBusinessEntityId, e.InspectedAt, e.Notes) { }
    }
}
