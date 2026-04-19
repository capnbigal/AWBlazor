using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Features.Person.Domain;

namespace AWBlazorApp.Features.Person.Audit;

public static class BusinessEntityAddressAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static BusinessEntityAddressAuditLog RecordCreate(BusinessEntityAddress e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static BusinessEntityAddressAuditLog RecordUpdate(BusinessEntityAddress e, string? by)
        => BuildLog(e, ActionUpdated, by, "Touched");

    public static BusinessEntityAddressAuditLog RecordDelete(BusinessEntityAddress e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static BusinessEntityAddressAuditLog BuildLog(
        BusinessEntityAddress e, string action, string? by, string? summary)
        => new()
        {
            BusinessEntityId = e.BusinessEntityId,
            AddressId = e.AddressId,
            AddressTypeId = e.AddressTypeId,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };
}
