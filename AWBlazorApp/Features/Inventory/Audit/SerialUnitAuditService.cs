using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Features.Inventory.Domain;
using System.Text;

namespace AWBlazorApp.Features.Inventory.Audit;

public static class SerialUnitAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(SerialUnit e) => new(e);

    public static SerialUnitAuditLog RecordCreate(SerialUnit e, string? by) => BuildLog(e, ActionCreated, by, "Created");
    public static SerialUnitAuditLog RecordUpdate(Snapshot before, SerialUnit after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));
    public static SerialUnitAuditLog RecordDelete(SerialUnit e, string? by) => BuildLog(e, ActionDeleted, by, "Deleted");

    private static SerialUnitAuditLog BuildLog(SerialUnit e, string action, string? by, string? summary)
        => new()
        {
            SerialUnitId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            InventoryItemId = e.InventoryItemId,
            LotId = e.LotId,
            SerialNumber = e.SerialNumber,
            Status = e.Status,
            CurrentLocationId = e.CurrentLocationId,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot b, SerialUnit a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "LotId", b.LotId, a.LotId);
        AuditDiffHelpers.AppendIfChanged(sb, "SerialNumber", b.SerialNumber, a.SerialNumber);
        AuditDiffHelpers.AppendIfChanged(sb, "Status", b.Status, a.Status);
        AuditDiffHelpers.AppendIfChanged(sb, "CurrentLocationId", b.CurrentLocationId, a.CurrentLocationId);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(int? LotId, string SerialNumber, SerialUnitStatus Status, int? CurrentLocationId)
    {
        public Snapshot(SerialUnit e) : this(e.LotId, e.SerialNumber, e.Status, e.CurrentLocationId) { }
    }
}
