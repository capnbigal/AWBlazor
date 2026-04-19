using AWBlazorApp.Shared.Audit;
using System.Text;
using AWBlazorApp.Features.Production.Domain; using AWBlazorApp.Features.Production.Cultures.Domain; using AWBlazorApp.Features.Production.Documents.Domain; using AWBlazorApp.Features.Production.Illustrations.Domain; using AWBlazorApp.Features.Production.Locations.Domain; using AWBlazorApp.Features.Production.ProductCategories.Domain; using AWBlazorApp.Features.Production.ProductCostHistories.Domain; using AWBlazorApp.Features.Production.ProductDescriptions.Domain; using AWBlazorApp.Features.Production.ProductDocuments.Domain; using AWBlazorApp.Features.Production.ProductInventories.Domain; using AWBlazorApp.Features.Production.ProductListPriceHistories.Domain; using AWBlazorApp.Features.Production.ProductModels.Domain; using AWBlazorApp.Features.Production.ProductModelIllustrations.Domain; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Domain; using AWBlazorApp.Features.Production.ProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductReviews.Domain; using AWBlazorApp.Features.Production.Products.Domain; using AWBlazorApp.Features.Production.ProductSubcategories.Domain; using AWBlazorApp.Features.Production.ScrapReasons.Domain; using AWBlazorApp.Features.Production.TransactionHistories.Domain; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Domain; using AWBlazorApp.Features.Production.UnitMeasures.Domain; using AWBlazorApp.Features.Production.WorkOrders.Domain; using AWBlazorApp.Features.Production.WorkOrderRoutings.Domain; 

namespace AWBlazorApp.Features.Production.Documents.Application.Services;

public static class DocumentAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(Document e) => new(e);

    public static DocumentAuditLog RecordCreate(Document e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static DocumentAuditLog RecordUpdate(Snapshot before, Document after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static DocumentAuditLog RecordDelete(Document e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static DocumentAuditLog BuildLog(Document e, string action, string? by, string? summary)
        => new()
        {
            DocumentNode = e.DocumentNode.ToString(),
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            Title = e.Title,
            Owner = e.Owner,
            FolderFlag = e.FolderFlag,
            FileName = e.FileName,
            FileExtension = e.FileExtension,
            Revision = e.Revision,
            ChangeNumber = e.ChangeNumber,
            Status = e.Status,
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, Document after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Title", before.Title, after.Title);
        AuditDiffHelpers.AppendIfChanged(sb, "Owner", before.Owner, after.Owner);
        AuditDiffHelpers.AppendIfChanged(sb, "FolderFlag", before.FolderFlag, after.FolderFlag);
        AuditDiffHelpers.AppendIfChanged(sb, "FileName", before.FileName, after.FileName);
        AuditDiffHelpers.AppendIfChanged(sb, "FileExtension", before.FileExtension, after.FileExtension);
        AuditDiffHelpers.AppendIfChanged(sb, "Revision", before.Revision, after.Revision);
        AuditDiffHelpers.AppendIfChanged(sb, "ChangeNumber", before.ChangeNumber, after.ChangeNumber);
        AuditDiffHelpers.AppendIfChanged(sb, "Status", before.Status, after.Status);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        string Title, int Owner, bool FolderFlag, string FileName, string FileExtension,
        string Revision, int ChangeNumber, byte Status)
    {
        public Snapshot(Document e) : this(
            e.Title, e.Owner, e.FolderFlag, e.FileName, e.FileExtension,
            e.Revision, e.ChangeNumber, e.Status)
        { }
    }
}
