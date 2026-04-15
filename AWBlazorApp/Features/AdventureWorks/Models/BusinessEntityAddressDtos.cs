using AWBlazorApp.Features.AdventureWorks.Domain;

namespace AWBlazorApp.Features.AdventureWorks.Models;

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

public sealed record BusinessEntityAddressAuditLogDto(
    int Id, int BusinessEntityId, int AddressId, int AddressTypeId,
    string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    Guid RowGuid, DateTime SourceModifiedDate);

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

    public static BusinessEntityAddressAuditLogDto ToDto(this BusinessEntityAddressAuditLog a) => new(
        a.Id, a.BusinessEntityId, a.AddressId, a.AddressTypeId,
        a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.RowGuid, a.SourceModifiedDate);
}
