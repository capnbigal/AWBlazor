using AWBlazorApp.Features.AdventureWorks.Domain;

namespace AWBlazorApp.Features.AdventureWorks.Audit;

public static class PersonCreditCardAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static PersonCreditCardAuditLog RecordCreate(PersonCreditCard e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static PersonCreditCardAuditLog RecordUpdate(PersonCreditCard e, string? by)
        => BuildLog(e, ActionUpdated, by, "Touched");

    public static PersonCreditCardAuditLog RecordDelete(PersonCreditCard e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static PersonCreditCardAuditLog BuildLog(
        PersonCreditCard e, string action, string? by, string? summary)
        => new()
        {
            BusinessEntityId = e.BusinessEntityId,
            CreditCardId = e.CreditCardId,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            SourceModifiedDate = e.ModifiedDate,
        };
}
