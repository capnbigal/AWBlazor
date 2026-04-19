using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Features.Person.Addresses.Domain; using AWBlazorApp.Features.Person.AddressTypes.Domain; using AWBlazorApp.Features.Person.BusinessEntities.Domain; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Domain; using AWBlazorApp.Features.Person.BusinessEntityContacts.Domain; using AWBlazorApp.Features.Person.ContactTypes.Domain; using AWBlazorApp.Features.Person.CountryRegions.Domain; using AWBlazorApp.Features.Person.EmailAddresses.Domain; using AWBlazorApp.Features.Person.Persons.Domain; using AWBlazorApp.Features.Person.PersonPhones.Domain; using AWBlazorApp.Features.Person.PhoneNumberTypes.Domain; using AWBlazorApp.Features.Person.StateProvinces.Domain; 

namespace AWBlazorApp.Features.Person.BusinessEntities.Application.Services;

public static class BusinessEntityAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static BusinessEntityAuditLog RecordCreate(BusinessEntity e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static BusinessEntityAuditLog RecordUpdate(BusinessEntity e, string? by)
        => BuildLog(e, ActionUpdated, by, "Touched");

    public static BusinessEntityAuditLog RecordDelete(BusinessEntity e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static BusinessEntityAuditLog BuildLog(BusinessEntity e, string action, string? by, string? summary)
        => new()
        {
            BusinessEntityId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };
}
