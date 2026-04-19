using AWBlazorApp.Shared.Audit;
using System.Text;
using AWBlazorApp.Features.Person.Domain;

namespace AWBlazorApp.Features.Person.Audit;

public static class ContactTypeAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(ContactType e) => new(e);

    public static ContactTypeAuditLog RecordCreate(ContactType e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static ContactTypeAuditLog RecordUpdate(Snapshot before, ContactType after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static ContactTypeAuditLog RecordDelete(ContactType e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static ContactTypeAuditLog BuildLog(ContactType e, string action, string? by, string? summary)
        => new()
        {
            ContactTypeId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            Name = e.Name,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, ContactType after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name)
    {
        public Snapshot(ContactType e) : this(e.Name) { }
    }
}
