using System.Text;
using AWBlazorApp.Data.Entities.AdventureWorks;

namespace AWBlazorApp.Services.AdventureWorksAudit;

public static class StoreAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(Store e) => new(e);

    public static StoreAuditLog RecordCreate(Store e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static StoreAuditLog RecordUpdate(Snapshot before, Store after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static StoreAuditLog RecordDelete(Store e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static StoreAuditLog BuildLog(Store e, string action, string? by, string? summary)
        => new()
        {
            StoreId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            Name = e.Name,
            SalesPersonId = e.SalesPersonId,
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, Store after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "SalesPersonId", before.SalesPersonId, after.SalesPersonId);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name, int? SalesPersonId)
    {
        public Snapshot(Store e) : this(e.Name, e.SalesPersonId) { }
    }
}
