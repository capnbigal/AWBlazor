using AWBlazorApp.Features.Production.Domain; using AWBlazorApp.Features.Production.Cultures.Domain; using AWBlazorApp.Features.Production.Documents.Domain; using AWBlazorApp.Features.Production.Illustrations.Domain; using AWBlazorApp.Features.Production.Locations.Domain; using AWBlazorApp.Features.Production.ProductCategories.Domain; using AWBlazorApp.Features.Production.ProductCostHistories.Domain; using AWBlazorApp.Features.Production.ProductDescriptions.Domain; using AWBlazorApp.Features.Production.ProductDocuments.Domain; using AWBlazorApp.Features.Production.ProductInventories.Domain; using AWBlazorApp.Features.Production.ProductListPriceHistories.Domain; using AWBlazorApp.Features.Production.ProductModels.Domain; using AWBlazorApp.Features.Production.ProductModelIllustrations.Domain; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Domain; using AWBlazorApp.Features.Production.ProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductReviews.Domain; using AWBlazorApp.Features.Production.Products.Domain; using AWBlazorApp.Features.Production.ProductSubcategories.Domain; using AWBlazorApp.Features.Production.ScrapReasons.Domain; using AWBlazorApp.Features.Production.TransactionHistories.Domain; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Domain; using AWBlazorApp.Features.Production.UnitMeasures.Domain; using AWBlazorApp.Features.Production.WorkOrders.Domain; using AWBlazorApp.Features.Production.WorkOrderRoutings.Domain; 

namespace AWBlazorApp.Features.Production.ProductReviews.Dtos;

public sealed record ProductReviewDto(
    int Id, int ProductId, string ReviewerName, DateTime ReviewDate, string EmailAddress,
    int Rating, string? Comments, DateTime ModifiedDate);

public sealed record CreateProductReviewRequest
{
    public int ProductId { get; set; }
    public string? ReviewerName { get; set; }
    public DateTime ReviewDate { get; set; }
    public string? EmailAddress { get; set; }
    public int Rating { get; set; }
    public string? Comments { get; set; }
}

public sealed record UpdateProductReviewRequest
{
    public int? ProductId { get; set; }
    public string? ReviewerName { get; set; }
    public DateTime? ReviewDate { get; set; }
    public string? EmailAddress { get; set; }
    public int? Rating { get; set; }
    public string? Comments { get; set; }
}

public static class ProductReviewMappings
{
    public static ProductReviewDto ToDto(this ProductReview e) => new(
        e.Id, e.ProductId, e.ReviewerName, e.ReviewDate, e.EmailAddress,
        e.Rating, e.Comments, e.ModifiedDate);

    public static ProductReview ToEntity(this CreateProductReviewRequest r) => new()
    {
        ProductId = r.ProductId,
        ReviewerName = (r.ReviewerName ?? string.Empty).Trim(),
        ReviewDate = r.ReviewDate == default ? DateTime.UtcNow : r.ReviewDate,
        EmailAddress = (r.EmailAddress ?? string.Empty).Trim(),
        Rating = r.Rating,
        Comments = string.IsNullOrWhiteSpace(r.Comments) ? null : r.Comments.Trim(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateProductReviewRequest r, ProductReview e)
    {
        if (r.ProductId.HasValue) e.ProductId = r.ProductId.Value;
        if (r.ReviewerName is not null) e.ReviewerName = r.ReviewerName.Trim();
        if (r.ReviewDate.HasValue) e.ReviewDate = r.ReviewDate.Value;
        if (r.EmailAddress is not null) e.EmailAddress = r.EmailAddress.Trim();
        if (r.Rating.HasValue) e.Rating = r.Rating.Value;
        if (r.Comments is not null) e.Comments = string.IsNullOrWhiteSpace(r.Comments) ? null : r.Comments.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
