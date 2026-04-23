using AWBlazorApp.Features.Person.Addresses.Domain; using AWBlazorApp.Features.Person.AddressTypes.Domain; using AWBlazorApp.Features.Person.BusinessEntities.Domain; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Domain; using AWBlazorApp.Features.Person.BusinessEntityContacts.Domain; using AWBlazorApp.Features.Person.ContactTypes.Domain; using AWBlazorApp.Features.Person.CountryRegions.Domain; using AWBlazorApp.Features.Person.EmailAddresses.Domain; using AWBlazorApp.Features.Person.Persons.Domain; using AWBlazorApp.Features.Person.PersonPhones.Domain; using AWBlazorApp.Features.Person.PhoneNumberTypes.Domain; using AWBlazorApp.Features.Person.StateProvinces.Domain; 

namespace AWBlazorApp.Features.Person.BusinessEntityContacts.Dtos;

public sealed record BusinessEntityContactDto(
    int BusinessEntityId, int PersonId, int ContactTypeId, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateBusinessEntityContactRequest
{
    public int BusinessEntityId { get; set; }
    public int PersonId { get; set; }
    public int ContactTypeId { get; set; }
}

/// <summary>
/// Pure junction — there are no non-key columns to update beyond ModifiedDate.
/// </summary>
public sealed record UpdateBusinessEntityContactRequest;

public static class BusinessEntityContactMappings
{
    public static BusinessEntityContactDto ToDto(this BusinessEntityContact e) => new(
        e.BusinessEntityId, e.PersonId, e.ContactTypeId, e.RowGuid, e.ModifiedDate);

    public static BusinessEntityContact ToEntity(this CreateBusinessEntityContactRequest r) => new()
    {
        BusinessEntityId = r.BusinessEntityId,
        PersonId = r.PersonId,
        ContactTypeId = r.ContactTypeId,
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateBusinessEntityContactRequest _, BusinessEntityContact e)
    {
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
