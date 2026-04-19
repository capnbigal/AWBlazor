using AWBlazorApp.Features.AdventureWorks.Audit;
using AWBlazorApp.Features.Enterprise.Domain;
using System.Text;

namespace AWBlazorApp.Features.Enterprise.Audit;

public static class StationAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(Station e) => new(e);

    public static StationAuditLog RecordCreate(Station e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static StationAuditLog RecordUpdate(Snapshot before, Station after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static StationAuditLog RecordDelete(Station e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static StationAuditLog BuildLog(Station e, string action, string? by, string? summary)
        => new()
        {
            StationId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            OrgUnitId = e.OrgUnitId,
            Code = e.Code,
            Name = e.Name,
            StationKind = e.StationKind,
            OperatorBusinessEntityId = e.OperatorBusinessEntityId,
            AssetId = e.AssetId,
            IdealCycleSeconds = e.IdealCycleSeconds,
            IsActive = e.IsActive,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, Station after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "OrgUnitId", before.OrgUnitId, after.OrgUnitId);
        AuditDiffHelpers.AppendIfChanged(sb, "Code", before.Code, after.Code);
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "StationKind", before.StationKind, after.StationKind);
        AuditDiffHelpers.AppendIfChanged(sb, "OperatorBusinessEntityId", before.OperatorBusinessEntityId, after.OperatorBusinessEntityId);
        AuditDiffHelpers.AppendIfChanged(sb, "AssetId", before.AssetId, after.AssetId);
        AuditDiffHelpers.AppendIfChanged(sb, "IdealCycleSeconds", before.IdealCycleSeconds, after.IdealCycleSeconds);
        AuditDiffHelpers.AppendIfChanged(sb, "IsActive", before.IsActive, after.IsActive);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        int OrgUnitId, string Code, string Name, StationKind StationKind,
        int? OperatorBusinessEntityId, int? AssetId, decimal? IdealCycleSeconds, bool IsActive)
    {
        public Snapshot(Station e) : this(
            e.OrgUnitId, e.Code, e.Name, e.StationKind,
            e.OperatorBusinessEntityId, e.AssetId, e.IdealCycleSeconds, e.IsActive) { }
    }
}
