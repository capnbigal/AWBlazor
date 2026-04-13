using AWBlazorApp.Data.Entities.AdventureWorks;

namespace AWBlazorApp.Models.AdventureWorks;

public sealed record ProductListPriceHistoryDto(
    int ProductId, DateTime StartDate, DateTime? EndDate, decimal ListPrice, DateTime ModifiedDate);

public sealed record CreateProductListPriceHistoryRequest
{
    public int ProductId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal ListPrice { get; set; }
}

public sealed record UpdateProductListPriceHistoryRequest
{
    public DateTime? EndDate { get; set; }
    public decimal? ListPrice { get; set; }
}

public sealed record ProductListPriceHistoryAuditLogDto(
    int Id, int ProductId, DateTime StartDate, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, DateTime? EndDate, decimal ListPrice, DateTime SourceModifiedDate);

public static class ProductListPriceHistoryMappings
{
    public static ProductListPriceHistoryDto ToDto(this ProductListPriceHistory e) => new(
        e.ProductId, e.StartDate, e.EndDate, e.ListPrice, e.ModifiedDate);

    public static ProductListPriceHistory ToEntity(this CreateProductListPriceHistoryRequest r) => new()
    {
        ProductId = r.ProductId,
        StartDate = r.StartDate,
        EndDate = r.EndDate,
        ListPrice = r.ListPrice,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateProductListPriceHistoryRequest r, ProductListPriceHistory e)
    {
        if (r.EndDate.HasValue) e.EndDate = r.EndDate.Value;
        if (r.ListPrice.HasValue) e.ListPrice = r.ListPrice.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static ProductListPriceHistoryAuditLogDto ToDto(this ProductListPriceHistoryAuditLog a) => new(
        a.Id, a.ProductId, a.StartDate, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.EndDate, a.ListPrice, a.SourceModifiedDate);
}
