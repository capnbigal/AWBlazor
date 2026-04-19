using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Production.Domain;

/// <summary>
/// Audit log for <see cref="ProductPhoto"/>. EF-managed table <c>dbo.ProductPhotoAuditLogs</c>.
/// We deliberately do NOT snapshot the image bytes themselves (the audit row would be huge
/// and not useful) — only the filenames and the timestamp.
/// </summary>
public class ProductPhotoAuditLog : AdventureWorksAuditLogBase
{
    public int ProductPhotoId { get; set; }

    [MaxLength(50)] public string? ThumbnailPhotoFileName { get; set; }
    [MaxLength(50)] public string? LargePhotoFileName { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
