using AWBlazorApp.Shared.Audit;
using System.Text;
using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Domain; using AWBlazorApp.Features.Sales.CreditCards.Domain; using AWBlazorApp.Features.Sales.Currencies.Domain; using AWBlazorApp.Features.Sales.CurrencyRates.Domain; using AWBlazorApp.Features.Sales.Customers.Domain; using AWBlazorApp.Features.Sales.PersonCreditCards.Domain; using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesPeople.Domain; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Domain; using AWBlazorApp.Features.Sales.SalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesTaxRates.Domain; using AWBlazorApp.Features.Sales.SalesTerritories.Domain; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Domain; using AWBlazorApp.Features.Sales.ShoppingCartItems.Domain; using AWBlazorApp.Features.Sales.SpecialOffers.Domain; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Domain; using AWBlazorApp.Features.Sales.Stores.Domain; 

namespace AWBlazorApp.Features.Sales.ShoppingCartItems.Application.Services;

public static class ShoppingCartItemAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(ShoppingCartItem e) => new(e);

    public static ShoppingCartItemAuditLog RecordCreate(ShoppingCartItem e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static ShoppingCartItemAuditLog RecordUpdate(Snapshot before, ShoppingCartItem after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static ShoppingCartItemAuditLog RecordDelete(ShoppingCartItem e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static ShoppingCartItemAuditLog BuildLog(ShoppingCartItem e, string action, string? by, string? summary)
        => new()
        {
            ShoppingCartItemId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            ShoppingCartId = e.ShoppingCartId,
            Quantity = e.Quantity,
            ProductId = e.ProductId,
            DateCreated = e.DateCreated,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, ShoppingCartItem after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "ShoppingCartId", before.ShoppingCartId, after.ShoppingCartId);
        AuditDiffHelpers.AppendIfChanged(sb, "Quantity", before.Quantity, after.Quantity);
        AuditDiffHelpers.AppendIfChanged(sb, "ProductId", before.ProductId, after.ProductId);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string ShoppingCartId, int Quantity, int ProductId)
    {
        public Snapshot(ShoppingCartItem e) : this(e.ShoppingCartId, e.Quantity, e.ProductId) { }
    }
}
