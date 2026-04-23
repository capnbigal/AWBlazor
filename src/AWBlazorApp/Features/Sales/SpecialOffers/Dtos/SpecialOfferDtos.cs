using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Domain; using AWBlazorApp.Features.Sales.CreditCards.Domain; using AWBlazorApp.Features.Sales.Currencies.Domain; using AWBlazorApp.Features.Sales.CurrencyRates.Domain; using AWBlazorApp.Features.Sales.Customers.Domain; using AWBlazorApp.Features.Sales.PersonCreditCards.Domain; using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesPeople.Domain; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Domain; using AWBlazorApp.Features.Sales.SalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesTaxRates.Domain; using AWBlazorApp.Features.Sales.SalesTerritories.Domain; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Domain; using AWBlazorApp.Features.Sales.ShoppingCartItems.Domain; using AWBlazorApp.Features.Sales.SpecialOffers.Domain; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Domain; using AWBlazorApp.Features.Sales.Stores.Domain; 

namespace AWBlazorApp.Features.Sales.SpecialOffers.Dtos;

public sealed record SpecialOfferDto(
    int Id,
    string Description,
    decimal DiscountPct,
    string OfferType,
    string Category,
    DateTime StartDate,
    DateTime EndDate,
    int MinQty,
    int? MaxQty,
    Guid RowGuid,
    DateTime ModifiedDate);

public sealed record CreateSpecialOfferRequest
{
    public string? Description { get; set; }
    public decimal DiscountPct { get; set; }
    public string? OfferType { get; set; }
    public string? Category { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int MinQty { get; set; }
    public int? MaxQty { get; set; }
}

public sealed record UpdateSpecialOfferRequest
{
    public string? Description { get; set; }
    public decimal? DiscountPct { get; set; }
    public string? OfferType { get; set; }
    public string? Category { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? MinQty { get; set; }
    public int? MaxQty { get; set; }
}

public static class SpecialOfferMappings
{
    public static SpecialOfferDto ToDto(this SpecialOffer e) => new(
        e.Id, e.Description, e.DiscountPct, e.OfferType, e.Category,
        e.StartDate, e.EndDate, e.MinQty, e.MaxQty, e.RowGuid, e.ModifiedDate);

    public static SpecialOffer ToEntity(this CreateSpecialOfferRequest r) => new()
    {
        Description = (r.Description ?? string.Empty).Trim(),
        DiscountPct = r.DiscountPct,
        OfferType = (r.OfferType ?? string.Empty).Trim(),
        Category = (r.Category ?? string.Empty).Trim(),
        StartDate = r.StartDate,
        EndDate = r.EndDate,
        MinQty = r.MinQty,
        MaxQty = r.MaxQty,
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateSpecialOfferRequest r, SpecialOffer e)
    {
        if (r.Description is not null) e.Description = r.Description.Trim();
        if (r.DiscountPct.HasValue) e.DiscountPct = r.DiscountPct.Value;
        if (r.OfferType is not null) e.OfferType = r.OfferType.Trim();
        if (r.Category is not null) e.Category = r.Category.Trim();
        if (r.StartDate.HasValue) e.StartDate = r.StartDate.Value;
        if (r.EndDate.HasValue) e.EndDate = r.EndDate.Value;
        if (r.MinQty.HasValue) e.MinQty = r.MinQty.Value;
        // MaxQty is nullable — an explicit null in the request means "clear it". Distinguishing
        // "not sent" from "clear it" would need JsonPatch; for Tool-Slot-style PATCH we only
        // apply MaxQty when it's present.
        if (r.MaxQty.HasValue) e.MaxQty = r.MaxQty.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
