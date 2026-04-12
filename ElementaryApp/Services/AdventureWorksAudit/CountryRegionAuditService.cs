using System.Text;
using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Services.AdventureWorksAudit;

public static class CountryRegionAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(CountryRegion e) => new(e);

    public static CountryRegionAuditLog RecordCreate(CountryRegion e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static CountryRegionAuditLog RecordUpdate(Snapshot before, CountryRegion after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static CountryRegionAuditLog RecordDelete(CountryRegion e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static CountryRegionAuditLog BuildLog(CountryRegion e, string action, string? by, string? summary)
        => new()
        {
            CountryRegionCode = e.CountryRegionCode,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            Name = e.Name,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, CountryRegion after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name)
    {
        public Snapshot(CountryRegion e) : this(e.Name) { }
    }
}
