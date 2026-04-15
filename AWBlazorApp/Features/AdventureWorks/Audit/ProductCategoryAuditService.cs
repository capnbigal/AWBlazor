using System.Text;
using AWBlazorApp.Features.AdventureWorks.Domain;

namespace AWBlazorApp.Features.AdventureWorks.Audit;

public static class ProductCategoryAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(ProductCategory e) => new(e);

    public static ProductCategoryAuditLog RecordCreate(ProductCategory e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static ProductCategoryAuditLog RecordUpdate(Snapshot before, ProductCategory after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static ProductCategoryAuditLog RecordDelete(ProductCategory e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static ProductCategoryAuditLog BuildLog(ProductCategory e, string action, string? by, string? summary)
        => new()
        {
            ProductCategoryId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            Name = e.Name,
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, ProductCategory after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name)
    {
        public Snapshot(ProductCategory e) : this(e.Name) { }
    }
}
