using System.Text;
using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Services.AdventureWorksAudit;

public static class SalesTerritoryHistoryAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(SalesTerritoryHistory e) => new(e);

    public static SalesTerritoryHistoryAuditLog RecordCreate(SalesTerritoryHistory e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static SalesTerritoryHistoryAuditLog RecordUpdate(Snapshot before, SalesTerritoryHistory after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static SalesTerritoryHistoryAuditLog RecordDelete(SalesTerritoryHistory e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static SalesTerritoryHistoryAuditLog BuildLog(SalesTerritoryHistory e, string action, string? by, string? summary)
        => new()
        {
            BusinessEntityId = e.BusinessEntityId,
            TerritoryId = e.TerritoryId,
            StartDate = e.StartDate,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            EndDate = e.EndDate,
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, SalesTerritoryHistory after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "EndDate", before.EndDate, after.EndDate);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(DateTime? EndDate)
    {
        public Snapshot(SalesTerritoryHistory e) : this(e.EndDate) { }
    }
}
