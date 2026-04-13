using AWBlazorApp.Data.Entities.AdventureWorks;

namespace AWBlazorApp.Services.AdventureWorksAudit;

public static class PersonPhoneAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static PersonPhoneAuditLog RecordCreate(PersonPhone e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static PersonPhoneAuditLog RecordUpdate(PersonPhone e, string? by)
        => BuildLog(e, ActionUpdated, by, "Touched");

    public static PersonPhoneAuditLog RecordDelete(PersonPhone e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static PersonPhoneAuditLog BuildLog(PersonPhone e, string action, string? by, string? summary)
        => new()
        {
            BusinessEntityId = e.BusinessEntityId,
            PhoneNumber = e.PhoneNumber,
            PhoneNumberTypeId = e.PhoneNumberTypeId,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            SourceModifiedDate = e.ModifiedDate,
        };
}
