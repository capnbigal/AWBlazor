using AWBlazorApp.Features.Sales.Domain;

namespace AWBlazorApp.Features.Sales.Models;

public sealed record CustomerDto(
    int Id, int? PersonId, int? StoreId, int? TerritoryId,
    string AccountNumber, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateCustomerRequest
{
    public int? PersonId { get; set; }
    public int? StoreId { get; set; }
    public int? TerritoryId { get; set; }
}

public sealed record UpdateCustomerRequest
{
    public int? PersonId { get; set; }
    public int? StoreId { get; set; }
    public int? TerritoryId { get; set; }
}

public sealed record CustomerAuditLogDto(
    int Id, int CustomerId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, int? PersonId, int? StoreId, int? TerritoryId,
    string? AccountNumber, Guid RowGuid, DateTime SourceModifiedDate);

public static class CustomerMappings
{
    public static CustomerDto ToDto(this Customer e) => new(
        e.Id, e.PersonId, e.StoreId, e.TerritoryId, e.AccountNumber, e.RowGuid, e.ModifiedDate);

    public static Customer ToEntity(this CreateCustomerRequest r) => new()
    {
        PersonId = r.PersonId,
        StoreId = r.StoreId,
        TerritoryId = r.TerritoryId,
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
        // AccountNumber is computed by SQL Server — leave it empty and EF won't write it.
    };

    public static void ApplyTo(this UpdateCustomerRequest r, Customer e)
    {
        // Nullable FKs: a null in the request means "don't touch". There's no "clear it back
        // to null" channel — use a dedicated endpoint or a sentinel value if you need that.
        if (r.PersonId.HasValue) e.PersonId = r.PersonId.Value;
        if (r.StoreId.HasValue) e.StoreId = r.StoreId.Value;
        if (r.TerritoryId.HasValue) e.TerritoryId = r.TerritoryId.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static CustomerAuditLogDto ToDto(this CustomerAuditLog a) => new(
        a.Id, a.CustomerId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.PersonId, a.StoreId, a.TerritoryId, a.AccountNumber, a.RowGuid, a.SourceModifiedDate);
}
