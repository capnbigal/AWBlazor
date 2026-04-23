using AWBlazorApp.Features.Production.Domain; using AWBlazorApp.Features.Production.Cultures.Domain; using AWBlazorApp.Features.Production.Documents.Domain; using AWBlazorApp.Features.Production.Illustrations.Domain; using AWBlazorApp.Features.Production.Locations.Domain; using AWBlazorApp.Features.Production.ProductCategories.Domain; using AWBlazorApp.Features.Production.ProductCostHistories.Domain; using AWBlazorApp.Features.Production.ProductDescriptions.Domain; using AWBlazorApp.Features.Production.ProductDocuments.Domain; using AWBlazorApp.Features.Production.ProductInventories.Domain; using AWBlazorApp.Features.Production.ProductListPriceHistories.Domain; using AWBlazorApp.Features.Production.ProductModels.Domain; using AWBlazorApp.Features.Production.ProductModelIllustrations.Domain; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Domain; using AWBlazorApp.Features.Production.ProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductReviews.Domain; using AWBlazorApp.Features.Production.Products.Domain; using AWBlazorApp.Features.Production.ProductSubcategories.Domain; using AWBlazorApp.Features.Production.ScrapReasons.Domain; using AWBlazorApp.Features.Production.TransactionHistories.Domain; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Domain; using AWBlazorApp.Features.Production.UnitMeasures.Domain; using AWBlazorApp.Features.Production.WorkOrders.Domain; using AWBlazorApp.Features.Production.WorkOrderRoutings.Domain; 

namespace AWBlazorApp.Features.Production.WorkOrders.Dtos;

public sealed record WorkOrderDto(
    int Id, int ProductId, int OrderQty, int StockedQty, short ScrappedQty,
    DateTime StartDate, DateTime? EndDate, DateTime DueDate,
    short? ScrapReasonId, DateTime ModifiedDate);

public sealed record CreateWorkOrderRequest
{
    public int ProductId { get; set; }
    public int OrderQty { get; set; }
    public short ScrappedQty { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime DueDate { get; set; }
    public short? ScrapReasonId { get; set; }
}

public sealed record UpdateWorkOrderRequest
{
    public int? ProductId { get; set; }
    public int? OrderQty { get; set; }
    public short? ScrappedQty { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? DueDate { get; set; }
    public short? ScrapReasonId { get; set; }
}

public static class WorkOrderMappings
{
    public static WorkOrderDto ToDto(this WorkOrder e) => new(
        e.Id, e.ProductId, e.OrderQty, e.StockedQty, e.ScrappedQty,
        e.StartDate, e.EndDate, e.DueDate, e.ScrapReasonId, e.ModifiedDate);

    public static WorkOrder ToEntity(this CreateWorkOrderRequest r) => new()
    {
        ProductId = r.ProductId,
        OrderQty = r.OrderQty,
        ScrappedQty = r.ScrappedQty,
        StartDate = r.StartDate,
        EndDate = r.EndDate,
        DueDate = r.DueDate,
        ScrapReasonId = r.ScrapReasonId,
        // StockedQty is computed by SQL Server (ISNULL(OrderQty - ScrappedQty, 0)) — leave default.
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateWorkOrderRequest r, WorkOrder e)
    {
        if (r.ProductId.HasValue) e.ProductId = r.ProductId.Value;
        if (r.OrderQty.HasValue) e.OrderQty = r.OrderQty.Value;
        if (r.ScrappedQty.HasValue) e.ScrappedQty = r.ScrappedQty.Value;
        if (r.StartDate.HasValue) e.StartDate = r.StartDate.Value;
        if (r.EndDate.HasValue) e.EndDate = r.EndDate.Value;
        if (r.DueDate.HasValue) e.DueDate = r.DueDate.Value;
        if (r.ScrapReasonId.HasValue) e.ScrapReasonId = r.ScrapReasonId.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
