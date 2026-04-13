using System.Text;
using AWBlazorApp.Data.Entities.AdventureWorks;

namespace AWBlazorApp.Services.AdventureWorksAudit;

public static class PhoneNumberTypeAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(PhoneNumberType e) => new(e);

    public static PhoneNumberTypeAuditLog RecordCreate(PhoneNumberType e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static PhoneNumberTypeAuditLog RecordUpdate(Snapshot before, PhoneNumberType after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static PhoneNumberTypeAuditLog RecordDelete(PhoneNumberType e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static PhoneNumberTypeAuditLog BuildLog(PhoneNumberType e, string action, string? by, string? summary)
        => new()
        {
            PhoneNumberTypeId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            Name = e.Name,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, PhoneNumberType after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name)
    {
        public Snapshot(PhoneNumberType e) : this(e.Name) { }
    }
}
