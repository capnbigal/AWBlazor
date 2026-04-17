using AWBlazorApp.Features.AdventureWorks.Audit;
using AWBlazorApp.Features.Enterprise.Domain;
using System.Text;

namespace AWBlazorApp.Features.Enterprise.Audit;

public static class CostCenterAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(CostCenter e) => new(e);

    public static CostCenterAuditLog RecordCreate(CostCenter e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static CostCenterAuditLog RecordUpdate(Snapshot before, CostCenter after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static CostCenterAuditLog RecordDelete(CostCenter e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static CostCenterAuditLog BuildLog(CostCenter e, string action, string? by, string? summary)
        => new()
        {
            CostCenterId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            OrganizationId = e.OrganizationId,
            Code = e.Code,
            Name = e.Name,
            OwnerBusinessEntityId = e.OwnerBusinessEntityId,
            IsActive = e.IsActive,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, CostCenter after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Code", before.Code, after.Code);
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "OwnerBusinessEntityId", before.OwnerBusinessEntityId, after.OwnerBusinessEntityId);
        AuditDiffHelpers.AppendIfChanged(sb, "IsActive", before.IsActive, after.IsActive);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Code, string Name, int? OwnerBusinessEntityId, bool IsActive)
    {
        public Snapshot(CostCenter e) : this(e.Code, e.Name, e.OwnerBusinessEntityId, e.IsActive) { }
    }
}
