using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Domain; using AWBlazorApp.Features.Sales.CreditCards.Domain; using AWBlazorApp.Features.Sales.Currencies.Domain; using AWBlazorApp.Features.Sales.CurrencyRates.Domain; using AWBlazorApp.Features.Sales.Customers.Domain; using AWBlazorApp.Features.Sales.PersonCreditCards.Domain; using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesPeople.Domain; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Domain; using AWBlazorApp.Features.Sales.SalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesTaxRates.Domain; using AWBlazorApp.Features.Sales.SalesTerritories.Domain; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Domain; using AWBlazorApp.Features.Sales.ShoppingCartItems.Domain; using AWBlazorApp.Features.Sales.SpecialOffers.Domain; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Domain; using AWBlazorApp.Features.Sales.Stores.Domain; 

namespace AWBlazorApp.Features.Sales.ShoppingCartItems.Dtos;

public sealed record ShoppingCartItemDto(
    int Id, string ShoppingCartId, int Quantity, int ProductId, DateTime DateCreated, DateTime ModifiedDate);

public sealed record CreateShoppingCartItemRequest
{
    public string? ShoppingCartId { get; set; }
    public int Quantity { get; set; }
    public int ProductId { get; set; }
}

public sealed record UpdateShoppingCartItemRequest
{
    public string? ShoppingCartId { get; set; }
    public int? Quantity { get; set; }
    public int? ProductId { get; set; }
}

public sealed record ShoppingCartItemAuditLogDto(
    int Id, int ShoppingCartItemId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? ShoppingCartId, int Quantity, int ProductId,
    DateTime DateCreated, DateTime SourceModifiedDate);

public static class ShoppingCartItemMappings
{
    public static ShoppingCartItemDto ToDto(this ShoppingCartItem e)
        => new(e.Id, e.ShoppingCartId, e.Quantity, e.ProductId, e.DateCreated, e.ModifiedDate);

    public static ShoppingCartItem ToEntity(this CreateShoppingCartItemRequest r)
    {
        var now = DateTime.UtcNow;
        return new ShoppingCartItem
        {
            ShoppingCartId = (r.ShoppingCartId ?? string.Empty).Trim(),
            Quantity = r.Quantity,
            ProductId = r.ProductId,
            DateCreated = now,
            ModifiedDate = now,
        };
    }

    public static void ApplyTo(this UpdateShoppingCartItemRequest r, ShoppingCartItem e)
    {
        if (r.ShoppingCartId is not null) e.ShoppingCartId = r.ShoppingCartId.Trim();
        if (r.Quantity.HasValue) e.Quantity = r.Quantity.Value;
        if (r.ProductId.HasValue) e.ProductId = r.ProductId.Value;
        e.ModifiedDate = DateTime.UtcNow;
        // DateCreated is preserved — it's a create-time anchor, not a last-modified timestamp.
    }

    public static ShoppingCartItemAuditLogDto ToDto(this ShoppingCartItemAuditLog a) => new(
        a.Id, a.ShoppingCartItemId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.ShoppingCartId, a.Quantity, a.ProductId, a.DateCreated, a.SourceModifiedDate);
}
