using AWBlazorApp.Data.Entities.AdventureWorks;

namespace AWBlazorApp.Models.AdventureWorks;

public sealed record SalesTerritoryHistoryDto(
    int BusinessEntityId, int TerritoryId, DateTime StartDate, DateTime? EndDate,
    Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateSalesTerritoryHistoryRequest
{
    public int BusinessEntityId { get; set; }
    public int TerritoryId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public sealed record UpdateSalesTerritoryHistoryRequest
{
    public DateTime? EndDate { get; set; }
}

public sealed record SalesTerritoryHistoryAuditLogDto(
    int Id, int BusinessEntityId, int TerritoryId, DateTime StartDate,
    string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    DateTime? EndDate, Guid RowGuid, DateTime SourceModifiedDate);

public static class SalesTerritoryHistoryMappings
{
    public static SalesTerritoryHistoryDto ToDto(this SalesTerritoryHistory e) => new(
        e.BusinessEntityId, e.TerritoryId, e.StartDate, e.EndDate, e.RowGuid, e.ModifiedDate);

    public static SalesTerritoryHistory ToEntity(this CreateSalesTerritoryHistoryRequest r) => new()
    {
        BusinessEntityId = r.BusinessEntityId,
        TerritoryId = r.TerritoryId,
        StartDate = r.StartDate,
        EndDate = r.EndDate,
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateSalesTerritoryHistoryRequest r, SalesTerritoryHistory e)
    {
        if (r.EndDate.HasValue) e.EndDate = r.EndDate.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static SalesTerritoryHistoryAuditLogDto ToDto(this SalesTerritoryHistoryAuditLog a) => new(
        a.Id, a.BusinessEntityId, a.TerritoryId, a.StartDate,
        a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.EndDate, a.RowGuid, a.SourceModifiedDate);
}
