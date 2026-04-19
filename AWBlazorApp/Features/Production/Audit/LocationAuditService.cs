using AWBlazorApp.Shared.Audit;
using System.Text;
using AWBlazorApp.Features.Production.Domain;

namespace AWBlazorApp.Features.Production.Audit;

public static class LocationAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(Location e) => new(e);

    public static LocationAuditLog RecordCreate(Location e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static LocationAuditLog RecordUpdate(Snapshot before, Location after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static LocationAuditLog RecordDelete(Location e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static LocationAuditLog BuildLog(Location e, string action, string? by, string? summary)
        => new()
        {
            LocationId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            Name = e.Name,
            CostRate = e.CostRate,
            Availability = e.Availability,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, Location after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "CostRate", before.CostRate, after.CostRate);
        AuditDiffHelpers.AppendIfChanged(sb, "Availability", before.Availability, after.Availability);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name, decimal CostRate, decimal Availability)
    {
        public Snapshot(Location e) : this(e.Name, e.CostRate, e.Availability) { }
    }
}
