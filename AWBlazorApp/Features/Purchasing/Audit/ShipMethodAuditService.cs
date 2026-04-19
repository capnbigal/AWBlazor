using AWBlazorApp.Shared.Audit;
using System.Text;
using AWBlazorApp.Features.Purchasing.Domain;

namespace AWBlazorApp.Features.Purchasing.Audit;

public static class ShipMethodAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(ShipMethod e) => new(e);

    public static ShipMethodAuditLog RecordCreate(ShipMethod e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static ShipMethodAuditLog RecordUpdate(Snapshot before, ShipMethod after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static ShipMethodAuditLog RecordDelete(ShipMethod e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static ShipMethodAuditLog BuildLog(ShipMethod e, string action, string? by, string? summary)
        => new()
        {
            ShipMethodId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            Name = e.Name,
            ShipBase = e.ShipBase,
            ShipRate = e.ShipRate,
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, ShipMethod after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "ShipBase", before.ShipBase, after.ShipBase);
        AuditDiffHelpers.AppendIfChanged(sb, "ShipRate", before.ShipRate, after.ShipRate);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name, decimal ShipBase, decimal ShipRate)
    {
        public Snapshot(ShipMethod e) : this(e.Name, e.ShipBase, e.ShipRate) { }
    }
}
