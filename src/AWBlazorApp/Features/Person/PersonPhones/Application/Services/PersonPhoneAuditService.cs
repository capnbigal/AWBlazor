using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Features.Person.Addresses.Domain; using AWBlazorApp.Features.Person.AddressTypes.Domain; using AWBlazorApp.Features.Person.BusinessEntities.Domain; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Domain; using AWBlazorApp.Features.Person.BusinessEntityContacts.Domain; using AWBlazorApp.Features.Person.ContactTypes.Domain; using AWBlazorApp.Features.Person.CountryRegions.Domain; using AWBlazorApp.Features.Person.EmailAddresses.Domain; using AWBlazorApp.Features.Person.Persons.Domain; using AWBlazorApp.Features.Person.PersonPhones.Domain; using AWBlazorApp.Features.Person.PhoneNumberTypes.Domain; using AWBlazorApp.Features.Person.StateProvinces.Domain; 

namespace AWBlazorApp.Features.Person.PersonPhones.Application.Services;

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
