using System.Text;
using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Services.AdventureWorksAudit;

public static class CurrencyAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(Currency e) => new(e);

    public static CurrencyAuditLog RecordCreate(Currency e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static CurrencyAuditLog RecordUpdate(Snapshot before, Currency after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static CurrencyAuditLog RecordDelete(Currency e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static CurrencyAuditLog BuildLog(Currency e, string action, string? by, string? summary)
        => new()
        {
            CurrencyCode = e.CurrencyCode,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            Name = e.Name,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, Currency after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name)
    {
        public Snapshot(Currency e) : this(e.Name) { }
    }
}
