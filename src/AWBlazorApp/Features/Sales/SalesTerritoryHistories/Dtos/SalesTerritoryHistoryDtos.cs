using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Domain; using AWBlazorApp.Features.Sales.CreditCards.Domain; using AWBlazorApp.Features.Sales.Currencies.Domain; using AWBlazorApp.Features.Sales.CurrencyRates.Domain; using AWBlazorApp.Features.Sales.Customers.Domain; using AWBlazorApp.Features.Sales.PersonCreditCards.Domain; using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesPeople.Domain; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Domain; using AWBlazorApp.Features.Sales.SalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesTaxRates.Domain; using AWBlazorApp.Features.Sales.SalesTerritories.Domain; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Domain; using AWBlazorApp.Features.Sales.ShoppingCartItems.Domain; using AWBlazorApp.Features.Sales.SpecialOffers.Domain; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Domain; using AWBlazorApp.Features.Sales.Stores.Domain; 

namespace AWBlazorApp.Features.Sales.SalesTerritoryHistories.Dtos;

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
