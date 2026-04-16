using AWBlazorApp.Features.Person.Domain;

namespace AWBlazorApp.Features.Person.Models;

public sealed record AddressTypeDto(int Id, string Name, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateAddressTypeRequest
{
    public string? Name { get; set; }
}

public sealed record UpdateAddressTypeRequest
{
    public string? Name { get; set; }
}

public sealed record AddressTypeAuditLogDto(
    int Id,
    int AddressTypeId,
    string Action,
    string? ChangedBy,
    DateTime ChangedDate,
    string? ChangeSummary,
    string? Name,
    Guid RowGuid,
    DateTime SourceModifiedDate);

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

    public static AddressTypeAuditLogDto ToDto(this AddressTypeAuditLog a) => new(
        a.Id, a.AddressTypeId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Name, a.RowGuid, a.SourceModifiedDate);
}
