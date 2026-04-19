using AWBlazorApp.Shared.Audit;
using System.Text;
using AWBlazorApp.Features.Production.Domain; using AWBlazorApp.Features.Production.Cultures.Domain; using AWBlazorApp.Features.Production.Documents.Domain; using AWBlazorApp.Features.Production.Illustrations.Domain; using AWBlazorApp.Features.Production.Locations.Domain; using AWBlazorApp.Features.Production.ProductCategories.Domain; using AWBlazorApp.Features.Production.ProductCostHistories.Domain; using AWBlazorApp.Features.Production.ProductDescriptions.Domain; using AWBlazorApp.Features.Production.ProductDocuments.Domain; using AWBlazorApp.Features.Production.ProductInventories.Domain; using AWBlazorApp.Features.Production.ProductListPriceHistories.Domain; using AWBlazorApp.Features.Production.ProductModels.Domain; using AWBlazorApp.Features.Production.ProductModelIllustrations.Domain; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Domain; using AWBlazorApp.Features.Production.ProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductReviews.Domain; using AWBlazorApp.Features.Production.Products.Domain; using AWBlazorApp.Features.Production.ProductSubcategories.Domain; using AWBlazorApp.Features.Production.ScrapReasons.Domain; using AWBlazorApp.Features.Production.TransactionHistories.Domain; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Domain; using AWBlazorApp.Features.Production.UnitMeasures.Domain; using AWBlazorApp.Features.Production.WorkOrders.Domain; using AWBlazorApp.Features.Production.WorkOrderRoutings.Domain; 

namespace AWBlazorApp.Features.Production.Audit;

public static class BillOfMaterialsAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(BillOfMaterials e) => new(e);

    public static BillOfMaterialsAuditLog RecordCreate(BillOfMaterials e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static BillOfMaterialsAuditLog RecordUpdate(Snapshot before, BillOfMaterials after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static BillOfMaterialsAuditLog RecordDelete(BillOfMaterials e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static BillOfMaterialsAuditLog BuildLog(BillOfMaterials e, string action, string? by, string? summary)
        => new()
        {
            BillOfMaterialsId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            ProductAssemblyId = e.ProductAssemblyId,
            ComponentId = e.ComponentId,
            StartDate = e.StartDate,
            EndDate = e.EndDate,
            UnitMeasureCode = e.UnitMeasureCode,
            BomLevel = e.BomLevel,
            PerAssemblyQty = e.PerAssemblyQty,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, BillOfMaterials after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "ProductAssemblyId", before.ProductAssemblyId, after.ProductAssemblyId);
        AuditDiffHelpers.AppendIfChanged(sb, "ComponentId", before.ComponentId, after.ComponentId);
        AuditDiffHelpers.AppendIfChanged(sb, "StartDate", before.StartDate, after.StartDate);
        AuditDiffHelpers.AppendIfChanged(sb, "EndDate", before.EndDate, after.EndDate);
        AuditDiffHelpers.AppendIfChanged(sb, "UnitMeasureCode", before.UnitMeasureCode, after.UnitMeasureCode);
        AuditDiffHelpers.AppendIfChanged(sb, "BomLevel", before.BomLevel, after.BomLevel);
        AuditDiffHelpers.AppendIfChanged(sb, "PerAssemblyQty", before.PerAssemblyQty, after.PerAssemblyQty);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        int? ProductAssemblyId, int ComponentId, DateTime StartDate, DateTime? EndDate,
        string UnitMeasureCode, short BomLevel, decimal PerAssemblyQty)
    {
        public Snapshot(BillOfMaterials e) : this(
            e.ProductAssemblyId, e.ComponentId, e.StartDate, e.EndDate,
            e.UnitMeasureCode, e.BomLevel, e.PerAssemblyQty)
        { }
    }
}
