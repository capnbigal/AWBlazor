using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Features.Inventory.Adjustments.Domain; using AWBlazorApp.Features.Inventory.Items.Domain; using AWBlazorApp.Features.Inventory.Locations.Domain; using AWBlazorApp.Features.Inventory.Lots.Domain; using AWBlazorApp.Features.Inventory.Outbox.Domain; using AWBlazorApp.Features.Inventory.Queue.Domain; using AWBlazorApp.Features.Inventory.Reports.Domain; using AWBlazorApp.Features.Inventory.Serials.Domain; using AWBlazorApp.Features.Inventory.Transactions.Domain; using AWBlazorApp.Features.Inventory.Types.Domain; 
using System.Text;

namespace AWBlazorApp.Features.Inventory.Locations.Application.Services;

public static class InventoryLocationAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(InventoryLocation e) => new(e);

    public static InventoryLocationAuditLog RecordCreate(InventoryLocation e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static InventoryLocationAuditLog RecordUpdate(Snapshot before, InventoryLocation after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static InventoryLocationAuditLog RecordDelete(InventoryLocation e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static InventoryLocationAuditLog BuildLog(InventoryLocation e, string action, string? by, string? summary)
        => new()
        {
            InventoryLocationId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            OrganizationId = e.OrganizationId,
            OrgUnitId = e.OrgUnitId,
            Code = e.Code,
            Name = e.Name,
            Kind = e.Kind,
            ParentLocationId = e.ParentLocationId,
            Path = e.Path,
            Depth = e.Depth,
            ProductionLocationId = e.ProductionLocationId,
            IsActive = e.IsActive,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot b, InventoryLocation a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "OrgUnitId", b.OrgUnitId, a.OrgUnitId);
        AuditDiffHelpers.AppendIfChanged(sb, "Code", b.Code, a.Code);
        AuditDiffHelpers.AppendIfChanged(sb, "Name", b.Name, a.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "Kind", b.Kind, a.Kind);
        AuditDiffHelpers.AppendIfChanged(sb, "ParentLocationId", b.ParentLocationId, a.ParentLocationId);
        AuditDiffHelpers.AppendIfChanged(sb, "ProductionLocationId", b.ProductionLocationId, a.ProductionLocationId);
        AuditDiffHelpers.AppendIfChanged(sb, "IsActive", b.IsActive, a.IsActive);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        int? OrgUnitId, string Code, string Name, InventoryLocationKind Kind,
        int? ParentLocationId, short? ProductionLocationId, bool IsActive)
    {
        public Snapshot(InventoryLocation e)
            : this(e.OrgUnitId, e.Code, e.Name, e.Kind, e.ParentLocationId, e.ProductionLocationId, e.IsActive) { }
    }
}
