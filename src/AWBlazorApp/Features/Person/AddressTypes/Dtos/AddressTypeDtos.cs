using AWBlazorApp.Features.Person.Addresses.Domain; using AWBlazorApp.Features.Person.AddressTypes.Domain; using AWBlazorApp.Features.Person.BusinessEntities.Domain; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Domain; using AWBlazorApp.Features.Person.BusinessEntityContacts.Domain; using AWBlazorApp.Features.Person.ContactTypes.Domain; using AWBlazorApp.Features.Person.CountryRegions.Domain; using AWBlazorApp.Features.Person.EmailAddresses.Domain; using AWBlazorApp.Features.Person.Persons.Domain; using AWBlazorApp.Features.Person.PersonPhones.Domain; using AWBlazorApp.Features.Person.PhoneNumberTypes.Domain; using AWBlazorApp.Features.Person.StateProvinces.Domain; 

namespace AWBlazorApp.Features.Person.AddressTypes.Dtos;

public sealed record AddressTypeDto(int Id, string Name, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateAddressTypeRequest
{
    public string? Name { get; set; }
}

public sealed record UpdateAddressTypeRequest
{
    public string? Name { get; set; }
}

public static class AddressTypeMappings
{
    public static AddressTypeDto ToDto(this AddressType e)
        => new(e.Id, e.Name, e.RowGuid, e.ModifiedDate);

    public static AddressType ToEntity(this CreateAddressTypeRequest r) => new()
    {
        Name = (r.Name ?? string.Empty).Trim(),
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateAddressTypeRequest r, AddressType e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
