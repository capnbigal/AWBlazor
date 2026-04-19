using AWBlazorApp.Features.Person.Addresses.Domain; using AWBlazorApp.Features.Person.AddressTypes.Domain; using AWBlazorApp.Features.Person.BusinessEntities.Domain; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Domain; using AWBlazorApp.Features.Person.BusinessEntityContacts.Domain; using AWBlazorApp.Features.Person.ContactTypes.Domain; using AWBlazorApp.Features.Person.CountryRegions.Domain; using AWBlazorApp.Features.Person.EmailAddresses.Domain; using AWBlazorApp.Features.Person.Persons.Domain; using AWBlazorApp.Features.Person.PersonPhones.Domain; using AWBlazorApp.Features.Person.PhoneNumberTypes.Domain; using AWBlazorApp.Features.Person.StateProvinces.Domain; 

namespace AWBlazorApp.Features.Person.ContactTypes.Dtos;

public sealed record ContactTypeDto(int Id, string Name, DateTime ModifiedDate);

public sealed record CreateContactTypeRequest
{
    public string? Name { get; set; }
}

public sealed record UpdateContactTypeRequest
{
    public string? Name { get; set; }
}

public sealed record ContactTypeAuditLogDto(
    int Id, int ContactTypeId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? Name, DateTime SourceModifiedDate);

public static class ContactTypeMappings
{
    public static ContactTypeDto ToDto(this ContactType e) => new(e.Id, e.Name, e.ModifiedDate);

    public static ContactType ToEntity(this CreateContactTypeRequest r) => new()
    {
        Name = (r.Name ?? string.Empty).Trim(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateContactTypeRequest r, ContactType e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static ContactTypeAuditLogDto ToDto(this ContactTypeAuditLog a) => new(
        a.Id, a.ContactTypeId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Name, a.SourceModifiedDate);
}
