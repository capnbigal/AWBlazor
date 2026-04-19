using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Domain; using AWBlazorApp.Features.Sales.CreditCards.Domain; using AWBlazorApp.Features.Sales.Currencies.Domain; using AWBlazorApp.Features.Sales.CurrencyRates.Domain; using AWBlazorApp.Features.Sales.Customers.Domain; using AWBlazorApp.Features.Sales.PersonCreditCards.Domain; using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesPeople.Domain; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Domain; using AWBlazorApp.Features.Sales.SalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesTaxRates.Domain; using AWBlazorApp.Features.Sales.SalesTerritories.Domain; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Domain; using AWBlazorApp.Features.Sales.ShoppingCartItems.Domain; using AWBlazorApp.Features.Sales.SpecialOffers.Domain; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Domain; using AWBlazorApp.Features.Sales.Stores.Domain; 

namespace AWBlazorApp.Features.Sales.CurrencyRates.Dtos;

public sealed record CurrencyRateDto(
    int Id, DateTime CurrencyRateDate, string FromCurrencyCode, string ToCurrencyCode,
    decimal AverageRate, decimal EndOfDayRate, DateTime ModifiedDate);

public sealed record CreateCurrencyRateRequest
{
    public DateTime CurrencyRateDate { get; set; }
    public string? FromCurrencyCode { get; set; }
    public string? ToCurrencyCode { get; set; }
    public decimal AverageRate { get; set; }
    public decimal EndOfDayRate { get; set; }
}

public sealed record UpdateCurrencyRateRequest
{
    public DateTime? CurrencyRateDate { get; set; }
    public string? FromCurrencyCode { get; set; }
    public string? ToCurrencyCode { get; set; }
    public decimal? AverageRate { get; set; }
    public decimal? EndOfDayRate { get; set; }
}

public sealed record CurrencyRateAuditLogDto(
    int Id, int CurrencyRateId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, DateTime CurrencyRateDate, string? FromCurrencyCode, string? ToCurrencyCode,
    decimal AverageRate, decimal EndOfDayRate, DateTime SourceModifiedDate);

public static class CurrencyRateMappings
{
    public static CurrencyRateDto ToDto(this CurrencyRate e) => new(
        e.Id, e.CurrencyRateDate, e.FromCurrencyCode, e.ToCurrencyCode,
        e.AverageRate, e.EndOfDayRate, e.ModifiedDate);

    public static CurrencyRate ToEntity(this CreateCurrencyRateRequest r) => new()
    {
        CurrencyRateDate = r.CurrencyRateDate,
        FromCurrencyCode = (r.FromCurrencyCode ?? string.Empty).Trim(),
        ToCurrencyCode = (r.ToCurrencyCode ?? string.Empty).Trim(),
        AverageRate = r.AverageRate,
        EndOfDayRate = r.EndOfDayRate,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateCurrencyRateRequest r, CurrencyRate e)
    {
        if (r.CurrencyRateDate.HasValue) e.CurrencyRateDate = r.CurrencyRateDate.Value;
        if (r.FromCurrencyCode is not null) e.FromCurrencyCode = r.FromCurrencyCode.Trim();
        if (r.ToCurrencyCode is not null) e.ToCurrencyCode = r.ToCurrencyCode.Trim();
        if (r.AverageRate.HasValue) e.AverageRate = r.AverageRate.Value;
        if (r.EndOfDayRate.HasValue) e.EndOfDayRate = r.EndOfDayRate.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static CurrencyRateAuditLogDto ToDto(this CurrencyRateAuditLog a) => new(
        a.Id, a.CurrencyRateId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.CurrencyRateDate, a.FromCurrencyCode, a.ToCurrencyCode,
        a.AverageRate, a.EndOfDayRate, a.SourceModifiedDate);
}
