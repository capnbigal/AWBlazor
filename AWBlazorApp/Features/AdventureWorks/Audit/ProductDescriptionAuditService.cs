using System.Text;
using AWBlazorApp.Features.AdventureWorks.Domain;

namespace AWBlazorApp.Features.AdventureWorks.Audit;

public static class ProductDescriptionAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(ProductDescription e) => new(e);

    public static ProductDescriptionAuditLog RecordCreate(ProductDescription e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static ProductDescriptionAuditLog RecordUpdate(Snapshot before, ProductDescription after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static ProductDescriptionAuditLog RecordDelete(ProductDescription e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static ProductDescriptionAuditLog BuildLog(ProductDescription e, string action, string? by, string? summary)
        => new()
        {
            ProductDescriptionId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            Description = e.Description,
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, ProductDescription after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Description", before.Description, after.Description);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Description)
    {
        public Snapshot(ProductDescription e) : this(e.Description) { }
    }
}
