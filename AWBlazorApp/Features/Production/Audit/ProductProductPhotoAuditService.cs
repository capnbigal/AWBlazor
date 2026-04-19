using AWBlazorApp.Shared.Audit;
using System.Text;
using AWBlazorApp.Features.Production.Domain;

namespace AWBlazorApp.Features.Production.Audit;

public static class ProductProductPhotoAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(ProductProductPhoto e) => new(e);

    public static ProductProductPhotoAuditLog RecordCreate(ProductProductPhoto e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static ProductProductPhotoAuditLog RecordUpdate(Snapshot before, ProductProductPhoto after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static ProductProductPhotoAuditLog RecordDelete(ProductProductPhoto e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static ProductProductPhotoAuditLog BuildLog(
        ProductProductPhoto e, string action, string? by, string? summary)
        => new()
        {
            ProductId = e.ProductId,
            ProductPhotoId = e.ProductPhotoId,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            Primary = e.Primary,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, ProductProductPhoto after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Primary", before.Primary, after.Primary);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(bool Primary)
    {
        public Snapshot(ProductProductPhoto e) : this(e.Primary) { }
    }
}
