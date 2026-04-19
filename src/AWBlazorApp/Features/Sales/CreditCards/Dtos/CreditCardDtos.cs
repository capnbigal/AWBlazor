using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Domain; using AWBlazorApp.Features.Sales.CreditCards.Domain; using AWBlazorApp.Features.Sales.Currencies.Domain; using AWBlazorApp.Features.Sales.CurrencyRates.Domain; using AWBlazorApp.Features.Sales.Customers.Domain; using AWBlazorApp.Features.Sales.PersonCreditCards.Domain; using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesPeople.Domain; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Domain; using AWBlazorApp.Features.Sales.SalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesTaxRates.Domain; using AWBlazorApp.Features.Sales.SalesTerritories.Domain; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Domain; using AWBlazorApp.Features.Sales.ShoppingCartItems.Domain; using AWBlazorApp.Features.Sales.SpecialOffers.Domain; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Domain; using AWBlazorApp.Features.Sales.Stores.Domain; 

namespace AWBlazorApp.Features.Sales.CreditCards.Dtos;

public sealed record CreditCardDto(
    int Id, string CardType, string CardNumber, byte ExpMonth, short ExpYear, DateTime ModifiedDate);

public sealed record CreateCreditCardRequest
{
    public string? CardType { get; set; }
    public string? CardNumber { get; set; }
    public byte ExpMonth { get; set; }
    public short ExpYear { get; set; }
}

public sealed record UpdateCreditCardRequest
{
    public string? CardType { get; set; }
    public string? CardNumber { get; set; }
    public byte? ExpMonth { get; set; }
    public short? ExpYear { get; set; }
}

public sealed record CreditCardAuditLogDto(
    int Id, int CreditCardId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? CardType, string? CardNumber, byte ExpMonth, short ExpYear,
    DateTime SourceModifiedDate);

public static class CreditCardMappings
{
    public static CreditCardDto ToDto(this CreditCard e) => new(
        e.Id, e.CardType, e.CardNumber, e.ExpMonth, e.ExpYear, e.ModifiedDate);

    public static CreditCard ToEntity(this CreateCreditCardRequest r) => new()
    {
        CardType = (r.CardType ?? string.Empty).Trim(),
        CardNumber = (r.CardNumber ?? string.Empty).Trim(),
        ExpMonth = r.ExpMonth,
        ExpYear = r.ExpYear,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateCreditCardRequest r, CreditCard e)
    {
        if (r.CardType is not null) e.CardType = r.CardType.Trim();
        if (r.CardNumber is not null) e.CardNumber = r.CardNumber.Trim();
        if (r.ExpMonth.HasValue) e.ExpMonth = r.ExpMonth.Value;
        if (r.ExpYear.HasValue) e.ExpYear = r.ExpYear.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static CreditCardAuditLogDto ToDto(this CreditCardAuditLog a) => new(
        a.Id, a.CreditCardId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.CardType, a.CardNumber, a.ExpMonth, a.ExpYear, a.SourceModifiedDate);
}
