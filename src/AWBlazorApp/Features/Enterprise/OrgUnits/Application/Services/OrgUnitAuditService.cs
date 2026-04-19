using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Features.Enterprise.Assets.Domain; using AWBlazorApp.Features.Enterprise.CostCenters.Domain; using AWBlazorApp.Features.Enterprise.OrgUnits.Domain; using AWBlazorApp.Features.Enterprise.Organizations.Domain; using AWBlazorApp.Features.Enterprise.ProductLines.Domain; using AWBlazorApp.Features.Enterprise.Stations.Domain; 
using System.Text;

namespace AWBlazorApp.Features.Enterprise.OrgUnits.Application.Services;

public static class OrgUnitAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(OrgUnit e) => new(e);

    public static OrgUnitAuditLog RecordCreate(OrgUnit e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static OrgUnitAuditLog RecordUpdate(Snapshot before, OrgUnit after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static OrgUnitAuditLog RecordDelete(OrgUnit e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static OrgUnitAuditLog BuildLog(OrgUnit e, string action, string? by, string? summary)
        => new()
        {
            OrgUnitId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            OrganizationId = e.OrganizationId,
            ParentOrgUnitId = e.ParentOrgUnitId,
            Kind = e.Kind,
            Code = e.Code,
            Name = e.Name,
            Path = e.Path,
            Depth = e.Depth,
            CostCenterId = e.CostCenterId,
            ManagerBusinessEntityId = e.ManagerBusinessEntityId,
            IsActive = e.IsActive,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, OrgUnit after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Kind", before.Kind, after.Kind);
        AuditDiffHelpers.AppendIfChanged(sb, "ParentOrgUnitId", before.ParentOrgUnitId, after.ParentOrgUnitId);
        AuditDiffHelpers.AppendIfChanged(sb, "Code", before.Code, after.Code);
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "CostCenterId", before.CostCenterId, after.CostCenterId);
        AuditDiffHelpers.AppendIfChanged(sb, "ManagerBusinessEntityId", before.ManagerBusinessEntityId, after.ManagerBusinessEntityId);
        AuditDiffHelpers.AppendIfChanged(sb, "IsActive", before.IsActive, after.IsActive);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        int? ParentOrgUnitId, OrgUnitKind Kind, string Code, string Name,
        int? CostCenterId, int? ManagerBusinessEntityId, bool IsActive)
    {
        public Snapshot(OrgUnit e) : this(
            e.ParentOrgUnitId, e.Kind, e.Code, e.Name,
            e.CostCenterId, e.ManagerBusinessEntityId, e.IsActive) { }
    }
}
