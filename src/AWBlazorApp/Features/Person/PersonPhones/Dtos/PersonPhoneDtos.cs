using AWBlazorApp.Features.Person.Addresses.Domain; using AWBlazorApp.Features.Person.AddressTypes.Domain; using AWBlazorApp.Features.Person.BusinessEntities.Domain; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Domain; using AWBlazorApp.Features.Person.BusinessEntityContacts.Domain; using AWBlazorApp.Features.Person.ContactTypes.Domain; using AWBlazorApp.Features.Person.CountryRegions.Domain; using AWBlazorApp.Features.Person.EmailAddresses.Domain; using AWBlazorApp.Features.Person.Persons.Domain; using AWBlazorApp.Features.Person.PersonPhones.Domain; using AWBlazorApp.Features.Person.PhoneNumberTypes.Domain; using AWBlazorApp.Features.Person.StateProvinces.Domain; 

namespace AWBlazorApp.Features.Person.PersonPhones.Dtos;

public sealed record PersonPhoneDto(
    int BusinessEntityId, string PhoneNumber, int PhoneNumberTypeId, DateTime ModifiedDate);

public sealed record CreatePersonPhoneRequest
{
    public int BusinessEntityId { get; set; }
    public string? PhoneNumber { get; set; }
    public int PhoneNumberTypeId { get; set; }
}

/// <summary>
/// PersonPhone has no non-key columns to update — all three columns are part of the composite
/// PK. Touching this row updates ModifiedDate; to actually change the phone number itself the
/// caller must DELETE + POST a new row.
/// </summary>
public sealed record UpdatePersonPhoneRequest;

public static class PersonPhoneMappings
{
    public static PersonPhoneDto ToDto(this PersonPhone e) => new(
        e.BusinessEntityId, e.PhoneNumber, e.PhoneNumberTypeId, e.ModifiedDate);

    public static PersonPhone ToEntity(this CreatePersonPhoneRequest r) => new()
    {
        BusinessEntityId = r.BusinessEntityId,
        PhoneNumber = (r.PhoneNumber ?? string.Empty).Trim(),
        PhoneNumberTypeId = r.PhoneNumberTypeId,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdatePersonPhoneRequest _, PersonPhone e)
    {
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
