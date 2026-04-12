using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Services.AdventureWorksAudit;

public static class IllustrationAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static IllustrationAuditLog RecordCreate(Illustration e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static IllustrationAuditLog RecordUpdate(Illustration e, string? by)
        => BuildLog(e, ActionUpdated, by, "Touched");

    public static IllustrationAuditLog RecordDelete(Illustration e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static IllustrationAuditLog BuildLog(Illustration e, string action, string? by, string? summary)
        => new()
        {
            IllustrationId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            SourceModifiedDate = e.ModifiedDate,
        };
}
