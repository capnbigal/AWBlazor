using AWBlazorApp.Features.AdventureWorks.Audit;
using AWBlazorApp.Features.Person.Domain;

namespace AWBlazorApp.Features.Person.Audit;

public static class BusinessEntityContactAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static BusinessEntityContactAuditLog RecordCreate(BusinessEntityContact e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static BusinessEntityContactAuditLog RecordUpdate(BusinessEntityContact e, string? by)
        => BuildLog(e, ActionUpdated, by, "Touched");

    public static BusinessEntityContactAuditLog RecordDelete(BusinessEntityContact e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static BusinessEntityContactAuditLog BuildLog(
        BusinessEntityContact e, string action, string? by, string? summary)
        => new()
        {
            BusinessEntityId = e.BusinessEntityId,
            PersonId = e.PersonId,
            ContactTypeId = e.ContactTypeId,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };
}
