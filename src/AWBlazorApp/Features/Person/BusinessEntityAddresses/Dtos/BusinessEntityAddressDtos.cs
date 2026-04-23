using AWBlazorApp.Features.Person.Addresses.Domain; using AWBlazorApp.Features.Person.AddressTypes.Domain; using AWBlazorApp.Features.Person.BusinessEntities.Domain; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Domain; using AWBlazorApp.Features.Person.BusinessEntityContacts.Domain; using AWBlazorApp.Features.Person.ContactTypes.Domain; using AWBlazorApp.Features.Person.CountryRegions.Domain; using AWBlazorApp.Features.Person.EmailAddresses.Domain; using AWBlazorApp.Features.Person.Persons.Domain; using AWBlazorApp.Features.Person.PersonPhones.Domain; using AWBlazorApp.Features.Person.PhoneNumberTypes.Domain; using AWBlazorApp.Features.Person.StateProvinces.Domain; 

namespace AWBlazorApp.Features.Person.BusinessEntityAddresses.Dtos;

public sealed record BusinessEntityAddressDto(
    int BusinessEntityId, int AddressId, int AddressTypeId, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateBusinessEntityAddressRequest
{
    public int BusinessEntityId { get; set; }
    public int AddressId { get; set; }
    public int AddressTypeId { get; set; }
}

/// <summary>
/// Pure junction — there are no non-key columns to update beyond ModifiedDate.
/// </summary>
public sealed record UpdateBusinessEntityAddressRequest;

public static class BusinessEntityAddressMappings
{
    public static BusinessEntityAddressDto ToDto(this BusinessEntityAddress e) => new(
        e.BusinessEntityId, e.AddressId, e.AddressTypeId, e.RowGuid, e.ModifiedDate);

    public static BusinessEntityAddress ToEntity(this CreateBusinessEntityAddressRequest r) => new()
    {
        BusinessEntityId = r.BusinessEntityId,
        AddressId = r.AddressId,
        AddressTypeId = r.AddressTypeId,
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateBusinessEntityAddressRequest _, BusinessEntityAddress e)
    {
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
