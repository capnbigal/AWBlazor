using System.Text;
using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Services.AdventureWorksAudit;

public static class EmailAddressAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(EmailAddress e) => new(e);

    public static EmailAddressAuditLog RecordCreate(EmailAddress e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static EmailAddressAuditLog RecordUpdate(Snapshot before, EmailAddress after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static EmailAddressAuditLog RecordDelete(EmailAddress e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static EmailAddressAuditLog BuildLog(EmailAddress e, string action, string? by, string? summary)
        => new()
        {
            BusinessEntityId = e.BusinessEntityId,
            EmailAddressId = e.EmailAddressId,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            EmailAddressValue = e.EmailAddressValue,
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, EmailAddress after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "EmailAddressValue", before.EmailAddressValue, after.EmailAddressValue);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string? EmailAddressValue)
    {
        public Snapshot(EmailAddress e) : this(e.EmailAddressValue) { }
    }
}
