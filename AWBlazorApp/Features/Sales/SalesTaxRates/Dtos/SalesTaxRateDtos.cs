using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Domain; using AWBlazorApp.Features.Sales.CreditCards.Domain; using AWBlazorApp.Features.Sales.Currencies.Domain; using AWBlazorApp.Features.Sales.CurrencyRates.Domain; using AWBlazorApp.Features.Sales.Customers.Domain; using AWBlazorApp.Features.Sales.PersonCreditCards.Domain; using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesPeople.Domain; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Domain; using AWBlazorApp.Features.Sales.SalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesTaxRates.Domain; using AWBlazorApp.Features.Sales.SalesTerritories.Domain; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Domain; using AWBlazorApp.Features.Sales.ShoppingCartItems.Domain; using AWBlazorApp.Features.Sales.SpecialOffers.Domain; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Domain; using AWBlazorApp.Features.Sales.Stores.Domain; 

namespace AWBlazorApp.Features.Sales.SalesTaxRates.Dtos;

public sealed record SalesTaxRateDto(
    int Id, int StateProvinceId, byte TaxType, decimal TaxRate, string Name, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateSalesTaxRateRequest
{
    public int StateProvinceId { get; set; }
    public byte TaxType { get; set; }
    public decimal TaxRate { get; set; }
    public string? Name { get; set; }
}

public sealed record UpdateSalesTaxRateRequest
{
    public int? StateProvinceId { get; set; }
    public byte? TaxType { get; set; }
    public decimal? TaxRate { get; set; }
    public string? Name { get; set; }
}

public sealed record SalesTaxRateAuditLogDto(
    int Id, int SalesTaxRateId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, int StateProvinceId, byte TaxType, decimal TaxRate, string? Name,
    Guid RowGuid, DateTime SourceModifiedDate);

public static class SalesTaxRateMappings
{
    public static SalesTaxRateDto ToDto(this SalesTaxRate e)
        => new(e.Id, e.StateProvinceId, e.TaxType, e.TaxRate, e.Name, e.RowGuid, e.ModifiedDate);

    public static SalesTaxRate ToEntity(this CreateSalesTaxRateRequest r) => new()
    {
        StateProvinceId = r.StateProvinceId,
        TaxType = r.TaxType,
        TaxRate = r.TaxRate,
        Name = (r.Name ?? string.Empty).Trim(),
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateSalesTaxRateRequest r, SalesTaxRate e)
    {
        if (r.StateProvinceId.HasValue) e.StateProvinceId = r.StateProvinceId.Value;
        if (r.TaxType.HasValue) e.TaxType = r.TaxType.Value;
        if (r.TaxRate.HasValue) e.TaxRate = r.TaxRate.Value;
        if (r.Name is not null) e.Name = r.Name.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static SalesTaxRateAuditLogDto ToDto(this SalesTaxRateAuditLog a) => new(
        a.Id, a.SalesTaxRateId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.StateProvinceId, a.TaxType, a.TaxRate, a.Name, a.RowGuid, a.SourceModifiedDate);
}
