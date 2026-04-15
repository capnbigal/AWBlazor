using System.Text;
using AWBlazorApp.Features.AdventureWorks.Domain;

namespace AWBlazorApp.Features.AdventureWorks.Audit;

public static class ProductModelAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(ProductModel e) => new(e);

    public static ProductModelAuditLog RecordCreate(ProductModel e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static ProductModelAuditLog RecordUpdate(Snapshot before, ProductModel after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static ProductModelAuditLog RecordDelete(ProductModel e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static ProductModelAuditLog BuildLog(ProductModel e, string action, string? by, string? summary)
        => new()
        {
            ProductModelId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            Name = e.Name,
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, ProductModel after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name)
    {
        public Snapshot(ProductModel e) : this(e.Name) { }
    }
}
