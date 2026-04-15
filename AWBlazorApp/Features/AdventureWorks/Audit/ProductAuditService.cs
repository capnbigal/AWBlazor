using System.Text;
using AWBlazorApp.Features.AdventureWorks.Domain;

namespace AWBlazorApp.Features.AdventureWorks.Audit;

public static class ProductAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(Product e) => new(e);

    public static ProductAuditLog RecordCreate(Product e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static ProductAuditLog RecordUpdate(Snapshot before, Product after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static ProductAuditLog RecordDelete(Product e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static ProductAuditLog BuildLog(Product e, string action, string? by, string? summary)
        => new()
        {
            ProductId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            Name = e.Name,
            ProductNumber = e.ProductNumber,
            MakeFlag = e.MakeFlag,
            FinishedGoodsFlag = e.FinishedGoodsFlag,
            Color = e.Color,
            SafetyStockLevel = e.SafetyStockLevel,
            ReorderPoint = e.ReorderPoint,
            StandardCost = e.StandardCost,
            ListPrice = e.ListPrice,
            Size = e.Size,
            SizeUnitMeasureCode = e.SizeUnitMeasureCode,
            WeightUnitMeasureCode = e.WeightUnitMeasureCode,
            Weight = e.Weight,
            DaysToManufacture = e.DaysToManufacture,
            ProductLine = e.ProductLine,
            Class = e.Class,
            Style = e.Style,
            ProductSubcategoryId = e.ProductSubcategoryId,
            ProductModelId = e.ProductModelId,
            SellStartDate = e.SellStartDate,
            SellEndDate = e.SellEndDate,
            DiscontinuedDate = e.DiscontinuedDate,
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, Product after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "ProductNumber", before.ProductNumber, after.ProductNumber);
        AuditDiffHelpers.AppendIfChanged(sb, "MakeFlag", before.MakeFlag, after.MakeFlag);
        AuditDiffHelpers.AppendIfChanged(sb, "FinishedGoodsFlag", before.FinishedGoodsFlag, after.FinishedGoodsFlag);
        AuditDiffHelpers.AppendIfChanged(sb, "Color", before.Color, after.Color);
        AuditDiffHelpers.AppendIfChanged(sb, "SafetyStockLevel", before.SafetyStockLevel, after.SafetyStockLevel);
        AuditDiffHelpers.AppendIfChanged(sb, "ReorderPoint", before.ReorderPoint, after.ReorderPoint);
        AuditDiffHelpers.AppendIfChanged(sb, "StandardCost", before.StandardCost, after.StandardCost);
        AuditDiffHelpers.AppendIfChanged(sb, "ListPrice", before.ListPrice, after.ListPrice);
        AuditDiffHelpers.AppendIfChanged(sb, "Size", before.Size, after.Size);
        AuditDiffHelpers.AppendIfChanged(sb, "Weight", before.Weight, after.Weight);
        AuditDiffHelpers.AppendIfChanged(sb, "ProductLine", before.ProductLine, after.ProductLine);
        AuditDiffHelpers.AppendIfChanged(sb, "Class", before.Class, after.Class);
        AuditDiffHelpers.AppendIfChanged(sb, "Style", before.Style, after.Style);
        AuditDiffHelpers.AppendIfChanged(sb, "ProductSubcategoryId", before.ProductSubcategoryId, after.ProductSubcategoryId);
        AuditDiffHelpers.AppendIfChanged(sb, "ProductModelId", before.ProductModelId, after.ProductModelId);
        AuditDiffHelpers.AppendIfChanged(sb, "SellStartDate", before.SellStartDate, after.SellStartDate);
        AuditDiffHelpers.AppendIfChanged(sb, "SellEndDate", before.SellEndDate, after.SellEndDate);
        AuditDiffHelpers.AppendIfChanged(sb, "DiscontinuedDate", before.DiscontinuedDate, after.DiscontinuedDate);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        string Name, string ProductNumber, bool MakeFlag, bool FinishedGoodsFlag,
        string? Color, short SafetyStockLevel, short ReorderPoint, decimal StandardCost, decimal ListPrice,
        string? Size, decimal? Weight, string? ProductLine, string? Class, string? Style,
        int? ProductSubcategoryId, int? ProductModelId,
        DateTime SellStartDate, DateTime? SellEndDate, DateTime? DiscontinuedDate)
    {
        public Snapshot(Product e) : this(
            e.Name, e.ProductNumber, e.MakeFlag, e.FinishedGoodsFlag,
            e.Color, e.SafetyStockLevel, e.ReorderPoint, e.StandardCost, e.ListPrice,
            e.Size, e.Weight, e.ProductLine, e.Class, e.Style,
            e.ProductSubcategoryId, e.ProductModelId,
            e.SellStartDate, e.SellEndDate, e.DiscontinuedDate)
        { }
    }
}
