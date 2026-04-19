using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Features.Performance.Domain;
using System.Text;

namespace AWBlazorApp.Features.Performance.Audit;

public static class KpiDefinitionAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(KpiDefinition e) => new(e);
    public static KpiDefinitionAuditLog RecordCreate(KpiDefinition e, string? by) => Build(e, ActionCreated, by, "Created");
    public static KpiDefinitionAuditLog RecordUpdate(Snapshot b, KpiDefinition a, string? by) => Build(a, ActionUpdated, by, Diff(b, a));
    public static KpiDefinitionAuditLog RecordDelete(KpiDefinition e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static KpiDefinitionAuditLog Build(KpiDefinition e, string action, string? by, string? summary) => new()
    {
        KpiDefinitionId = e.Id,
        Action = action, ChangedBy = by, ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        Code = e.Code, Name = e.Name, Description = e.Description, Unit = e.Unit,
        Source = e.Source, Aggregation = e.Aggregation,
        TargetValue = e.TargetValue,
        WarningThreshold = e.WarningThreshold, CriticalThreshold = e.CriticalThreshold,
        Direction = e.Direction, IsActive = e.IsActive,
        SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, KpiDefinition a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", b.Name, a.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "TargetValue", b.TargetValue, a.TargetValue);
        AuditDiffHelpers.AppendIfChanged(sb, "Aggregation", b.Aggregation, a.Aggregation);
        AuditDiffHelpers.AppendIfChanged(sb, "IsActive", b.IsActive, a.IsActive);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name, decimal? TargetValue, KpiAggregation Aggregation, bool IsActive)
    {
        public Snapshot(KpiDefinition e) : this(e.Name, e.TargetValue, e.Aggregation, e.IsActive) { }
    }
}

public static class ScorecardDefinitionAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(ScorecardDefinition e) => new(e);
    public static ScorecardDefinitionAuditLog RecordCreate(ScorecardDefinition e, string? by) => Build(e, ActionCreated, by, "Created");
    public static ScorecardDefinitionAuditLog RecordUpdate(Snapshot b, ScorecardDefinition a, string? by) => Build(a, ActionUpdated, by, Diff(b, a));
    public static ScorecardDefinitionAuditLog RecordDelete(ScorecardDefinition e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static ScorecardDefinitionAuditLog Build(ScorecardDefinition e, string action, string? by, string? summary) => new()
    {
        ScorecardDefinitionId = e.Id,
        Action = action, ChangedBy = by, ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        Code = e.Code, Name = e.Name, Description = e.Description,
        OwnerUserId = e.OwnerUserId, IsActive = e.IsActive,
        SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, ScorecardDefinition a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", b.Name, a.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "IsActive", b.IsActive, a.IsActive);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name, bool IsActive)
    {
        public Snapshot(ScorecardDefinition e) : this(e.Name, e.IsActive) { }
    }
}

public static class PerformanceReportAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(PerformanceReport e) => new(e);
    public static PerformanceReportAuditLog RecordCreate(PerformanceReport e, string? by) => Build(e, ActionCreated, by, "Created");
    public static PerformanceReportAuditLog RecordUpdate(Snapshot b, PerformanceReport a, string? by) => Build(a, ActionUpdated, by, Diff(b, a));
    public static PerformanceReportAuditLog RecordDelete(PerformanceReport e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static PerformanceReportAuditLog Build(PerformanceReport e, string action, string? by, string? summary) => new()
    {
        PerformanceReportId = e.Id,
        Action = action, ChangedBy = by, ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        Code = e.Code, Name = e.Name, Description = e.Description,
        Kind = e.Kind,
        RangePreset = e.RangePreset, StationId = e.StationId, AssetId = e.AssetId,
        DefinitionJson = e.DefinitionJson, LastRunAt = e.LastRunAt,
        IsActive = e.IsActive, SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, PerformanceReport a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", b.Name, a.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "Kind", b.Kind, a.Kind);
        AuditDiffHelpers.AppendIfChanged(sb, "RangePreset", b.RangePreset, a.RangePreset);
        AuditDiffHelpers.AppendIfChanged(sb, "StationId", b.StationId, a.StationId);
        AuditDiffHelpers.AppendIfChanged(sb, "AssetId", b.AssetId, a.AssetId);
        AuditDiffHelpers.AppendIfChanged(sb, "IsActive", b.IsActive, a.IsActive);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name, PerformanceReportKind Kind, ReportRangePreset RangePreset, int? StationId, int? AssetId, bool IsActive)
    {
        public Snapshot(PerformanceReport e) : this(e.Name, e.Kind, e.RangePreset, e.StationId, e.AssetId, e.IsActive) { }
    }
}
