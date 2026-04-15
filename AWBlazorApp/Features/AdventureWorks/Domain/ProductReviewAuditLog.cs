using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.AdventureWorks.Domain;

/// <summary>Audit log for <see cref="ProductReview"/>. EF-managed table <c>dbo.ProductReviewAuditLogs</c>.</summary>
public class ProductReviewAuditLog : AdventureWorksAuditLogBase
{
    public int ProductReviewId { get; set; }

    public int ProductId { get; set; }
    [MaxLength(50)] public string? ReviewerName { get; set; }
    public DateTime ReviewDate { get; set; }
    [MaxLength(50)] public string? EmailAddress { get; set; }
    public int Rating { get; set; }
    [MaxLength(3850)] public string? Comments { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
