using AWBlazorApp.Features.Production.Domain; using AWBlazorApp.Features.Production.Cultures.Domain; using AWBlazorApp.Features.Production.Documents.Domain; using AWBlazorApp.Features.Production.Illustrations.Domain; using AWBlazorApp.Features.Production.Locations.Domain; using AWBlazorApp.Features.Production.ProductCategories.Domain; using AWBlazorApp.Features.Production.ProductCostHistories.Domain; using AWBlazorApp.Features.Production.ProductDescriptions.Domain; using AWBlazorApp.Features.Production.ProductDocuments.Domain; using AWBlazorApp.Features.Production.ProductInventories.Domain; using AWBlazorApp.Features.Production.ProductListPriceHistories.Domain; using AWBlazorApp.Features.Production.ProductModels.Domain; using AWBlazorApp.Features.Production.ProductModelIllustrations.Domain; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Domain; using AWBlazorApp.Features.Production.ProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductReviews.Domain; using AWBlazorApp.Features.Production.Products.Domain; using AWBlazorApp.Features.Production.ProductSubcategories.Domain; using AWBlazorApp.Features.Production.ScrapReasons.Domain; using AWBlazorApp.Features.Production.TransactionHistories.Domain; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Domain; using AWBlazorApp.Features.Production.UnitMeasures.Domain; using AWBlazorApp.Features.Production.WorkOrders.Domain; using AWBlazorApp.Features.Production.WorkOrderRoutings.Domain; 

namespace AWBlazorApp.Features.Production.TransactionHistoryArchives.Dtos;

public sealed record TransactionHistoryArchiveDto(
    int Id, int ProductId, int ReferenceOrderId, int ReferenceOrderLineId,
    DateTime TransactionDate, string TransactionType, int Quantity,
    decimal ActualCost, DateTime ModifiedDate);

public sealed record CreateTransactionHistoryArchiveRequest
{
    /// <summary>PK is NOT identity — archive rows carry their original TransactionID.</summary>
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int ReferenceOrderId { get; set; }
    public int ReferenceOrderLineId { get; set; }
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal ActualCost { get; set; }
}

public sealed record UpdateTransactionHistoryArchiveRequest
{
    public int? ProductId { get; set; }
    public int? ReferenceOrderId { get; set; }
    public int? ReferenceOrderLineId { get; set; }
    public DateTime? TransactionDate { get; set; }
    public string? TransactionType { get; set; }
    public int? Quantity { get; set; }
    public decimal? ActualCost { get; set; }
}

public static class TransactionHistoryArchiveMappings
{
    public static TransactionHistoryArchiveDto ToDto(this TransactionHistoryArchive e) => new(
        e.Id, e.ProductId, e.ReferenceOrderId, e.ReferenceOrderLineId,
        e.TransactionDate, e.TransactionType, e.Quantity, e.ActualCost, e.ModifiedDate);

    public static TransactionHistoryArchive ToEntity(this CreateTransactionHistoryArchiveRequest r) => new()
    {
        Id = r.Id,
        ProductId = r.ProductId,
        ReferenceOrderId = r.ReferenceOrderId,
        ReferenceOrderLineId = r.ReferenceOrderLineId,
        TransactionDate = r.TransactionDate,
        TransactionType = r.TransactionType,
        Quantity = r.Quantity,
        ActualCost = r.ActualCost,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateTransactionHistoryArchiveRequest r, TransactionHistoryArchive e)
    {
        if (r.ProductId.HasValue) e.ProductId = r.ProductId.Value;
        if (r.ReferenceOrderId.HasValue) e.ReferenceOrderId = r.ReferenceOrderId.Value;
        if (r.ReferenceOrderLineId.HasValue) e.ReferenceOrderLineId = r.ReferenceOrderLineId.Value;
        if (r.TransactionDate.HasValue) e.TransactionDate = r.TransactionDate.Value;
        if (r.TransactionType is not null) e.TransactionType = r.TransactionType;
        if (r.Quantity.HasValue) e.Quantity = r.Quantity.Value;
        if (r.ActualCost.HasValue) e.ActualCost = r.ActualCost.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
