using AWBlazorApp.Features.Person.Addresses.Domain; using AWBlazorApp.Features.Person.AddressTypes.Domain; using AWBlazorApp.Features.Person.BusinessEntities.Domain; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Domain; using AWBlazorApp.Features.Person.BusinessEntityContacts.Domain; using AWBlazorApp.Features.Person.ContactTypes.Domain; using AWBlazorApp.Features.Person.CountryRegions.Domain; using AWBlazorApp.Features.Person.EmailAddresses.Domain; using AWBlazorApp.Features.Person.Persons.Domain; using AWBlazorApp.Features.Person.PersonPhones.Domain; using AWBlazorApp.Features.Person.PhoneNumberTypes.Domain; using AWBlazorApp.Features.Person.StateProvinces.Domain; 

namespace AWBlazorApp.Features.Person.Addresses.Dtos;

public sealed record AddressDto(
    int Id, string AddressLine1, string? AddressLine2, string City,
    int StateProvinceId, string PostalCode, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateAddressRequest
{
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public int StateProvinceId { get; set; }
    public string? PostalCode { get; set; }
}

public sealed record UpdateAddressRequest
{
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public int? StateProvinceId { get; set; }
    public string? PostalCode { get; set; }
}

public static class AddressMappings
{
    public static AddressDto ToDto(this Address e) => new(
        e.Id, e.AddressLine1, e.AddressLine2, e.City,
        e.StateProvinceId, e.PostalCode, e.RowGuid, e.ModifiedDate);

    public static Address ToEntity(this CreateAddressRequest r) => new()
    {
        AddressLine1 = (r.AddressLine1 ?? string.Empty).Trim(),
        AddressLine2 = string.IsNullOrWhiteSpace(r.AddressLine2) ? null : r.AddressLine2.Trim(),
        City = (r.City ?? string.Empty).Trim(),
        StateProvinceId = r.StateProvinceId,
        PostalCode = (r.PostalCode ?? string.Empty).Trim(),
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateAddressRequest r, Address e)
    {
        if (r.AddressLine1 is not null) e.AddressLine1 = r.AddressLine1.Trim();
        if (r.AddressLine2 is not null) e.AddressLine2 = string.IsNullOrWhiteSpace(r.AddressLine2) ? null : r.AddressLine2.Trim();
        if (r.City is not null) e.City = r.City.Trim();
        if (r.StateProvinceId.HasValue) e.StateProvinceId = r.StateProvinceId.Value;
        if (r.PostalCode is not null) e.PostalCode = r.PostalCode.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
