using System.Text;
using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Services.AdventureWorksAudit;

public static class ProductReviewAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(ProductReview e) => new(e);

    public static ProductReviewAuditLog RecordCreate(ProductReview e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static ProductReviewAuditLog RecordUpdate(Snapshot before, ProductReview after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static ProductReviewAuditLog RecordDelete(ProductReview e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static ProductReviewAuditLog BuildLog(ProductReview e, string action, string? by, string? summary)
        => new()
        {
            ProductReviewId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            ProductId = e.ProductId,
            ReviewerName = e.ReviewerName,
            ReviewDate = e.ReviewDate,
            EmailAddress = e.EmailAddress,
            Rating = e.Rating,
            Comments = e.Comments,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, ProductReview after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "ProductId", before.ProductId, after.ProductId);
        AuditDiffHelpers.AppendIfChanged(sb, "ReviewerName", before.ReviewerName, after.ReviewerName);
        AuditDiffHelpers.AppendIfChanged(sb, "EmailAddress", before.EmailAddress, after.EmailAddress);
        AuditDiffHelpers.AppendIfChanged(sb, "Rating", before.Rating, after.Rating);
        AuditDiffHelpers.AppendIfChanged(sb, "Comments", before.Comments, after.Comments);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        int ProductId, string ReviewerName, string EmailAddress, int Rating, string? Comments)
    {
        public Snapshot(ProductReview e) : this(e.ProductId, e.ReviewerName, e.EmailAddress, e.Rating, e.Comments) { }
    }
}
