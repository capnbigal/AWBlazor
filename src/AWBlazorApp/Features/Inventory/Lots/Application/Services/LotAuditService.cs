using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Features.Inventory.Adjustments.Domain; using AWBlazorApp.Features.Inventory.Items.Domain; using AWBlazorApp.Features.Inventory.Locations.Domain; using AWBlazorApp.Features.Inventory.Lots.Domain; using AWBlazorApp.Features.Inventory.Outbox.Domain; using AWBlazorApp.Features.Inventory.Queue.Domain; using AWBlazorApp.Features.Inventory.Reports.Domain; using AWBlazorApp.Features.Inventory.Serials.Domain; using AWBlazorApp.Features.Inventory.Transactions.Domain; using AWBlazorApp.Features.Inventory.Types.Domain; 
using System.Text;

namespace AWBlazorApp.Features.Inventory.Lots.Application.Services;

public static class LotAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(Lot e) => new(e);

    public static LotAuditLog RecordCreate(Lot e, string? by) => BuildLog(e, ActionCreated, by, "Created");
    public static LotAuditLog RecordUpdate(Snapshot before, Lot after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));
    public static LotAuditLog RecordDelete(Lot e, string? by) => BuildLog(e, ActionDeleted, by, "Deleted");

    private static LotAuditLog BuildLog(Lot e, string action, string? by, string? summary)
        => new()
        {
            LotId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            InventoryItemId = e.InventoryItemId,
            LotCode = e.LotCode,
            ManufacturedAt = e.ManufacturedAt,
            ReceivedAt = e.ReceivedAt,
            VendorBusinessEntityId = e.VendorBusinessEntityId,
            Status = e.Status,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot b, Lot a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "LotCode", b.LotCode, a.LotCode);
        AuditDiffHelpers.AppendIfChanged(sb, "ManufacturedAt", b.ManufacturedAt, a.ManufacturedAt);
        AuditDiffHelpers.AppendIfChanged(sb, "ReceivedAt", b.ReceivedAt, a.ReceivedAt);
        AuditDiffHelpers.AppendIfChanged(sb, "VendorBusinessEntityId", b.VendorBusinessEntityId, a.VendorBusinessEntityId);
        AuditDiffHelpers.AppendIfChanged(sb, "Status", b.Status, a.Status);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        string LotCode, DateTime? ManufacturedAt, DateTime? ReceivedAt, int? VendorBusinessEntityId, LotStatus Status)
    {
        public Snapshot(Lot e) : this(e.LotCode, e.ManufacturedAt, e.ReceivedAt, e.VendorBusinessEntityId, e.Status) { }
    }
}
