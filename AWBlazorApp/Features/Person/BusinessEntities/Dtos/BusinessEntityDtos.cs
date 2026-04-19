using AWBlazorApp.Features.Person.Addresses.Domain; using AWBlazorApp.Features.Person.AddressTypes.Domain; using AWBlazorApp.Features.Person.BusinessEntities.Domain; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Domain; using AWBlazorApp.Features.Person.BusinessEntityContacts.Domain; using AWBlazorApp.Features.Person.ContactTypes.Domain; using AWBlazorApp.Features.Person.CountryRegions.Domain; using AWBlazorApp.Features.Person.EmailAddresses.Domain; using AWBlazorApp.Features.Person.Persons.Domain; using AWBlazorApp.Features.Person.PersonPhones.Domain; using AWBlazorApp.Features.Person.PhoneNumberTypes.Domain; using AWBlazorApp.Features.Person.StateProvinces.Domain; 

namespace AWBlazorApp.Features.Person.BusinessEntities.Dtos;

public sealed record BusinessEntityDto(int Id, Guid RowGuid, DateTime ModifiedDate);

/// <summary>
/// BusinessEntity has no editable data of its own — it's just a surrogate-key holder shared by
/// Person, Store, Vendor, etc. Create requests therefore carry no fields, but the row still
/// gets a fresh rowguid + ModifiedDate stamp on insert.
/// </summary>
public sealed record CreateBusinessEntityRequest;

public sealed record UpdateBusinessEntityRequest;

public sealed record BusinessEntityAuditLogDto(
    int Id, int BusinessEntityId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, Guid RowGuid, DateTime SourceModifiedDate);

public static class BusinessEntityMappings
{
    public static BusinessEntityDto ToDto(this BusinessEntity e) => new(e.Id, e.RowGuid, e.ModifiedDate);

    public static BusinessEntity ToEntity(this CreateBusinessEntityRequest _) => new()
    {
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateBusinessEntityRequest _, BusinessEntity e)
    {
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static BusinessEntityAuditLogDto ToDto(this BusinessEntityAuditLog a) => new(
        a.Id, a.BusinessEntityId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.RowGuid, a.SourceModifiedDate);
}
