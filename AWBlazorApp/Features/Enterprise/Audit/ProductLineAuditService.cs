using AWBlazorApp.Features.AdventureWorks.Audit;
using AWBlazorApp.Features.Enterprise.Domain;
using System.Text;

namespace AWBlazorApp.Features.Enterprise.Audit;

public static class ProductLineAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(ProductLine e) => new(e);

    public static ProductLineAuditLog RecordCreate(ProductLine e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static ProductLineAuditLog RecordUpdate(Snapshot before, ProductLine after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static ProductLineAuditLog RecordDelete(ProductLine e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static ProductLineAuditLog BuildLog(ProductLine e, string action, string? by, string? summary)
        => new()
        {
            ProductLineId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            OrganizationId = e.OrganizationId,
            Code = e.Code,
            Name = e.Name,
            Description = e.Description,
            IsActive = e.IsActive,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, ProductLine after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Code", before.Code, after.Code);
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "Description", before.Description, after.Description);
        AuditDiffHelpers.AppendIfChanged(sb, "IsActive", before.IsActive, after.IsActive);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Code, string Name, string? Description, bool IsActive)
    {
        public Snapshot(ProductLine e) : this(e.Code, e.Name, e.Description, e.IsActive) { }
    }
}
