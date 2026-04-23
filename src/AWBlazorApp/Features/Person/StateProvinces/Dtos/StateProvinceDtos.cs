using AWBlazorApp.Features.Person.Addresses.Domain; using AWBlazorApp.Features.Person.AddressTypes.Domain; using AWBlazorApp.Features.Person.BusinessEntities.Domain; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Domain; using AWBlazorApp.Features.Person.BusinessEntityContacts.Domain; using AWBlazorApp.Features.Person.ContactTypes.Domain; using AWBlazorApp.Features.Person.CountryRegions.Domain; using AWBlazorApp.Features.Person.EmailAddresses.Domain; using AWBlazorApp.Features.Person.Persons.Domain; using AWBlazorApp.Features.Person.PersonPhones.Domain; using AWBlazorApp.Features.Person.PhoneNumberTypes.Domain; using AWBlazorApp.Features.Person.StateProvinces.Domain; 

namespace AWBlazorApp.Features.Person.StateProvinces.Dtos;

public sealed record StateProvinceDto(
    int Id, string StateProvinceCode, string CountryRegionCode, bool IsOnlyStateProvinceFlag,
    string Name, int TerritoryId, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateStateProvinceRequest
{
    public string? StateProvinceCode { get; set; }
    public string? CountryRegionCode { get; set; }
    public bool IsOnlyStateProvinceFlag { get; set; }
    public string? Name { get; set; }
    public int TerritoryId { get; set; }
}

public sealed record UpdateStateProvinceRequest
{
    public string? StateProvinceCode { get; set; }
    public string? CountryRegionCode { get; set; }
    public bool? IsOnlyStateProvinceFlag { get; set; }
    public string? Name { get; set; }
    public int? TerritoryId { get; set; }
}

public static class StateProvinceMappings
{
    public static StateProvinceDto ToDto(this StateProvince e) => new(
        e.Id, e.StateProvinceCode, e.CountryRegionCode, e.IsOnlyStateProvinceFlag,
        e.Name, e.TerritoryId, e.RowGuid, e.ModifiedDate);

    public static StateProvince ToEntity(this CreateStateProvinceRequest r) => new()
    {
        StateProvinceCode = (r.StateProvinceCode ?? string.Empty).Trim(),
        CountryRegionCode = (r.CountryRegionCode ?? string.Empty).Trim(),
        IsOnlyStateProvinceFlag = r.IsOnlyStateProvinceFlag,
        Name = (r.Name ?? string.Empty).Trim(),
        TerritoryId = r.TerritoryId,
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateStateProvinceRequest r, StateProvince e)
    {
        if (r.StateProvinceCode is not null) e.StateProvinceCode = r.StateProvinceCode.Trim();
        if (r.CountryRegionCode is not null) e.CountryRegionCode = r.CountryRegionCode.Trim();
        if (r.IsOnlyStateProvinceFlag.HasValue) e.IsOnlyStateProvinceFlag = r.IsOnlyStateProvinceFlag.Value;
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.TerritoryId.HasValue) e.TerritoryId = r.TerritoryId.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
