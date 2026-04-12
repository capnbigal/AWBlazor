using System.Text;
using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Services.AdventureWorksAudit;

public static class ProductSubcategoryAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(ProductSubcategory e) => new(e);

    public static ProductSubcategoryAuditLog RecordCreate(ProductSubcategory e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static ProductSubcategoryAuditLog RecordUpdate(Snapshot before, ProductSubcategory after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static ProductSubcategoryAuditLog RecordDelete(ProductSubcategory e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static ProductSubcategoryAuditLog BuildLog(ProductSubcategory e, string action, string? by, string? summary)
        => new()
        {
            ProductSubcategoryId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            ProductCategoryId = e.ProductCategoryId,
            Name = e.Name,
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, ProductSubcategory after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "ProductCategoryId", before.ProductCategoryId, after.ProductCategoryId);
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(int ProductCategoryId, string Name)
    {
        public Snapshot(ProductSubcategory e) : this(e.ProductCategoryId, e.Name) { }
    }
}
