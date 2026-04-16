using System.Text;
using AWBlazorApp.Features.AdventureWorks.Audit;
using AWBlazorApp.Features.Person.Domain;
using PersonEntity = AWBlazorApp.Features.Person.Domain.Person;

namespace AWBlazorApp.Features.Person.Audit;

public static class PersonAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(PersonEntity e) => new(e);

    public static PersonAuditLog RecordCreate(PersonEntity e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static PersonAuditLog RecordUpdate(Snapshot before, PersonEntity after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static PersonAuditLog RecordDelete(PersonEntity e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static PersonAuditLog BuildLog(PersonEntity e, string action, string? by, string? summary)
        => new()
        {
            PersonId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            PersonType = e.PersonType,
            NameStyle = e.NameStyle,
            Title = e.Title,
            FirstName = e.FirstName,
            MiddleName = e.MiddleName,
            LastName = e.LastName,
            Suffix = e.Suffix,
            EmailPromotion = e.EmailPromotion,
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, PersonEntity after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "PersonType", before.PersonType, after.PersonType);
        AuditDiffHelpers.AppendIfChanged(sb, "NameStyle", before.NameStyle, after.NameStyle);
        AuditDiffHelpers.AppendIfChanged(sb, "Title", before.Title, after.Title);
        AuditDiffHelpers.AppendIfChanged(sb, "FirstName", before.FirstName, after.FirstName);
        AuditDiffHelpers.AppendIfChanged(sb, "MiddleName", before.MiddleName, after.MiddleName);
        AuditDiffHelpers.AppendIfChanged(sb, "LastName", before.LastName, after.LastName);
        AuditDiffHelpers.AppendIfChanged(sb, "Suffix", before.Suffix, after.Suffix);
        AuditDiffHelpers.AppendIfChanged(sb, "EmailPromotion", before.EmailPromotion, after.EmailPromotion);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        string PersonType, bool NameStyle, string? Title,
        string FirstName, string? MiddleName, string LastName, string? Suffix, int EmailPromotion)
    {
        public Snapshot(PersonEntity e) : this(
            e.PersonType, e.NameStyle, e.Title,
            e.FirstName, e.MiddleName, e.LastName, e.Suffix, e.EmailPromotion)
        { }
    }
}
