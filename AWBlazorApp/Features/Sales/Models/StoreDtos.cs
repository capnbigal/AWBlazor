using AWBlazorApp.Features.Sales.Domain;

namespace AWBlazorApp.Features.Sales.Models;

public sealed record StoreDto(
    int Id, string Name, int? SalesPersonId, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateStoreRequest
{
    /// <summary>PK / FK to Person.BusinessEntity. NOT identity — caller must supply.</summary>
    public int Id { get; set; }
    public string? Name { get; set; }
    public int? SalesPersonId { get; set; }
}

public sealed record UpdateStoreRequest
{
    public string? Name { get; set; }
    public int? SalesPersonId { get; set; }
}

public sealed record StoreAuditLogDto(
    int Id, int StoreId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? Name, int? SalesPersonId,
    Guid RowGuid, DateTime SourceModifiedDate);

public static class StoreMappings
{
    public static StoreDto ToDto(this Store e) => new(
        e.Id, e.Name, e.SalesPersonId, e.RowGuid, e.ModifiedDate);

    public static Store ToEntity(this CreateStoreRequest r) => new()
    {
        Id = r.Id,
        Name = (r.Name ?? string.Empty).Trim(),
        SalesPersonId = r.SalesPersonId,
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateStoreRequest r, Store e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.SalesPersonId.HasValue) e.SalesPersonId = r.SalesPersonId.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static StoreAuditLogDto ToDto(this StoreAuditLog a) => new(
        a.Id, a.StoreId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Name, a.SalesPersonId, a.RowGuid, a.SourceModifiedDate);
}
