using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Features.Enterprise.Assets.Domain; using AWBlazorApp.Features.Enterprise.CostCenters.Domain; using AWBlazorApp.Features.Enterprise.OrgUnits.Domain; using AWBlazorApp.Features.Enterprise.Organizations.Domain; using AWBlazorApp.Features.Enterprise.ProductLines.Domain; using AWBlazorApp.Features.Enterprise.Stations.Domain; 
using System.Text;

namespace AWBlazorApp.Features.Enterprise.Organizations.Application.Services;

public static class OrganizationAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(Organization e) => new(e);

    public static OrganizationAuditLog RecordCreate(Organization e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static OrganizationAuditLog RecordUpdate(Snapshot before, Organization after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static OrganizationAuditLog RecordDelete(Organization e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static OrganizationAuditLog BuildLog(Organization e, string action, string? by, string? summary)
        => new()
        {
            OrganizationId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            Code = e.Code,
            Name = e.Name,
            IsPrimary = e.IsPrimary,
            ParentOrganizationId = e.ParentOrganizationId,
            ExternalRef = e.ExternalRef,
            IsActive = e.IsActive,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, Organization after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Code", before.Code, after.Code);
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "IsPrimary", before.IsPrimary, after.IsPrimary);
        AuditDiffHelpers.AppendIfChanged(sb, "ParentOrganizationId", before.ParentOrganizationId, after.ParentOrganizationId);
        AuditDiffHelpers.AppendIfChanged(sb, "ExternalRef", before.ExternalRef, after.ExternalRef);
        AuditDiffHelpers.AppendIfChanged(sb, "IsActive", before.IsActive, after.IsActive);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        string Code, string Name, bool IsPrimary, int? ParentOrganizationId, string? ExternalRef, bool IsActive)
    {
        public Snapshot(Organization e) : this(e.Code, e.Name, e.IsPrimary, e.ParentOrganizationId, e.ExternalRef, e.IsActive) { }
    }
}
