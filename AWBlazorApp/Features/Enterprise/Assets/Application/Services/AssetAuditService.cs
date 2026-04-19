using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Features.Enterprise.Assets.Domain; using AWBlazorApp.Features.Enterprise.CostCenters.Domain; using AWBlazorApp.Features.Enterprise.OrgUnits.Domain; using AWBlazorApp.Features.Enterprise.Organizations.Domain; using AWBlazorApp.Features.Enterprise.ProductLines.Domain; using AWBlazorApp.Features.Enterprise.Stations.Domain; 
using System.Text;

namespace AWBlazorApp.Features.Enterprise.Assets.Application.Services;

public static class AssetAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(Asset e) => new(e);

    public static AssetAuditLog RecordCreate(Asset e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static AssetAuditLog RecordUpdate(Snapshot before, Asset after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static AssetAuditLog RecordDelete(Asset e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static AssetAuditLog BuildLog(Asset e, string action, string? by, string? summary)
        => new()
        {
            AssetId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            OrganizationId = e.OrganizationId,
            OrgUnitId = e.OrgUnitId,
            AssetTag = e.AssetTag,
            Name = e.Name,
            Manufacturer = e.Manufacturer,
            Model = e.Model,
            SerialNumber = e.SerialNumber,
            AssetType = e.AssetType,
            CommissionedAt = e.CommissionedAt,
            DecommissionedAt = e.DecommissionedAt,
            Status = e.Status,
            ParentAssetId = e.ParentAssetId,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, Asset after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "OrgUnitId", before.OrgUnitId, after.OrgUnitId);
        AuditDiffHelpers.AppendIfChanged(sb, "AssetTag", before.AssetTag, after.AssetTag);
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "Manufacturer", before.Manufacturer, after.Manufacturer);
        AuditDiffHelpers.AppendIfChanged(sb, "Model", before.Model, after.Model);
        AuditDiffHelpers.AppendIfChanged(sb, "SerialNumber", before.SerialNumber, after.SerialNumber);
        AuditDiffHelpers.AppendIfChanged(sb, "AssetType", before.AssetType, after.AssetType);
        AuditDiffHelpers.AppendIfChanged(sb, "CommissionedAt", before.CommissionedAt, after.CommissionedAt);
        AuditDiffHelpers.AppendIfChanged(sb, "DecommissionedAt", before.DecommissionedAt, after.DecommissionedAt);
        AuditDiffHelpers.AppendIfChanged(sb, "Status", before.Status, after.Status);
        AuditDiffHelpers.AppendIfChanged(sb, "ParentAssetId", before.ParentAssetId, after.ParentAssetId);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        int? OrgUnitId, string AssetTag, string Name, string? Manufacturer, string? Model,
        string? SerialNumber, AssetType AssetType, DateTime? CommissionedAt, DateTime? DecommissionedAt,
        AssetStatus Status, int? ParentAssetId)
    {
        public Snapshot(Asset e) : this(
            e.OrgUnitId, e.AssetTag, e.Name, e.Manufacturer, e.Model,
            e.SerialNumber, e.AssetType, e.CommissionedAt, e.DecommissionedAt,
            e.Status, e.ParentAssetId) { }
    }
}
