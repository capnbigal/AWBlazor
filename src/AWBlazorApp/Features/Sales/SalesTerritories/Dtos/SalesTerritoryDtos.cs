using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Domain; using AWBlazorApp.Features.Sales.CreditCards.Domain; using AWBlazorApp.Features.Sales.Currencies.Domain; using AWBlazorApp.Features.Sales.CurrencyRates.Domain; using AWBlazorApp.Features.Sales.Customers.Domain; using AWBlazorApp.Features.Sales.PersonCreditCards.Domain; using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesPeople.Domain; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Domain; using AWBlazorApp.Features.Sales.SalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesTaxRates.Domain; using AWBlazorApp.Features.Sales.SalesTerritories.Domain; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Domain; using AWBlazorApp.Features.Sales.ShoppingCartItems.Domain; using AWBlazorApp.Features.Sales.SpecialOffers.Domain; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Domain; using AWBlazorApp.Features.Sales.Stores.Domain; 

namespace AWBlazorApp.Features.Sales.SalesTerritories.Dtos;

public sealed record SalesTerritoryDto(
    int Id, string Name, string CountryRegionCode, string GroupName,
    decimal SalesYtd, decimal SalesLastYear, decimal CostYtd, decimal CostLastYear,
    Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateSalesTerritoryRequest
{
    public string? Name { get; set; }
    public string? CountryRegionCode { get; set; }
    public string? GroupName { get; set; }
}

public sealed record UpdateSalesTerritoryRequest
{
    public string? Name { get; set; }
    public string? CountryRegionCode { get; set; }
    public string? GroupName { get; set; }
    public decimal? SalesYtd { get; set; }
    public decimal? SalesLastYear { get; set; }
    public decimal? CostYtd { get; set; }
    public decimal? CostLastYear { get; set; }
}

public sealed record SalesTerritoryAuditLogDto(
    int Id, int SalesTerritoryId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? Name, string? CountryRegionCode, string? GroupName,
    decimal SalesYtd, decimal SalesLastYear, decimal CostYtd, decimal CostLastYear,
    Guid RowGuid, DateTime SourceModifiedDate);

public static class SalesTerritoryMappings
{
    public static SalesTerritoryDto ToDto(this SalesTerritory e) => new(
        e.Id, e.Name, e.CountryRegionCode, e.GroupName,
        e.SalesYtd, e.SalesLastYear, e.CostYtd, e.CostLastYear,
        e.RowGuid, e.ModifiedDate);

    public static SalesTerritory ToEntity(this CreateSalesTerritoryRequest r) => new()
    {
        Name = (r.Name ?? string.Empty).Trim(),
        CountryRegionCode = (r.CountryRegionCode ?? string.Empty).Trim(),
        GroupName = (r.GroupName ?? string.Empty).Trim(),
        // Sales / cost YTD columns start at 0 for a new territory.
        SalesYtd = 0m,
        SalesLastYear = 0m,
        CostYtd = 0m,
        CostLastYear = 0m,
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateSalesTerritoryRequest r, SalesTerritory e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.CountryRegionCode is not null) e.CountryRegionCode = r.CountryRegionCode.Trim();
        if (r.GroupName is not null) e.GroupName = r.GroupName.Trim();
        if (r.SalesYtd.HasValue) e.SalesYtd = r.SalesYtd.Value;
        if (r.SalesLastYear.HasValue) e.SalesLastYear = r.SalesLastYear.Value;
        if (r.CostYtd.HasValue) e.CostYtd = r.CostYtd.Value;
        if (r.CostLastYear.HasValue) e.CostLastYear = r.CostLastYear.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static SalesTerritoryAuditLogDto ToDto(this SalesTerritoryAuditLog a) => new(
        a.Id, a.SalesTerritoryId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Name, a.CountryRegionCode, a.GroupName,
        a.SalesYtd, a.SalesLastYear, a.CostYtd, a.CostLastYear,
        a.RowGuid, a.SourceModifiedDate);
}
