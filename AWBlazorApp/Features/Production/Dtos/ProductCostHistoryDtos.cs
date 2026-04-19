using AWBlazorApp.Features.Production.Domain;

namespace AWBlazorApp.Features.Production.Dtos;

public sealed record ProductCostHistoryDto(
    int ProductId, DateTime StartDate, DateTime? EndDate, decimal StandardCost, DateTime ModifiedDate);

public sealed record CreateProductCostHistoryRequest
{
    public int ProductId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal StandardCost { get; set; }
}

public sealed record UpdateProductCostHistoryRequest
{
    public DateTime? EndDate { get; set; }
    public decimal? StandardCost { get; set; }
}

public sealed record ProductCostHistoryAuditLogDto(
    int Id, int ProductId, DateTime StartDate, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, DateTime? EndDate, decimal StandardCost, DateTime SourceModifiedDate);

public static class ProductCostHistoryMappings
{
    public static ProductCostHistoryDto ToDto(this ProductCostHistory e) => new(
        e.ProductId, e.StartDate, e.EndDate, e.StandardCost, e.ModifiedDate);

    public static ProductCostHistory ToEntity(this CreateProductCostHistoryRequest r) => new()
    {
        ProductId = r.ProductId,
        StartDate = r.StartDate,
        EndDate = r.EndDate,
        StandardCost = r.StandardCost,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateProductCostHistoryRequest r, ProductCostHistory e)
    {
        if (r.EndDate.HasValue) e.EndDate = r.EndDate.Value;
        if (r.StandardCost.HasValue) e.StandardCost = r.StandardCost.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static ProductCostHistoryAuditLogDto ToDto(this ProductCostHistoryAuditLog a) => new(
        a.Id, a.ProductId, a.StartDate, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.EndDate, a.StandardCost, a.SourceModifiedDate);
}
