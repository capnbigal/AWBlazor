using AWBlazorApp.Data.Entities.AdventureWorks;

namespace AWBlazorApp.Models.AdventureWorks;

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

public sealed record ProductReviewAuditLogDto(
    int Id, int ProductReviewId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, int ProductId, string? ReviewerName, DateTime ReviewDate,
    string? EmailAddress, int Rating, string? Comments, DateTime SourceModifiedDate);

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

    public static ProductReviewAuditLogDto ToDto(this ProductReviewAuditLog a) => new(
        a.Id, a.ProductReviewId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.ProductId, a.ReviewerName, a.ReviewDate, a.EmailAddress,
        a.Rating, a.Comments, a.SourceModifiedDate);
}
