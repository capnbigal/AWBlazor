using AWBlazorApp.Features.Production.Domain; using AWBlazorApp.Features.Production.Cultures.Domain; using AWBlazorApp.Features.Production.Documents.Domain; using AWBlazorApp.Features.Production.Illustrations.Domain; using AWBlazorApp.Features.Production.Locations.Domain; using AWBlazorApp.Features.Production.ProductCategories.Domain; using AWBlazorApp.Features.Production.ProductCostHistories.Domain; using AWBlazorApp.Features.Production.ProductDescriptions.Domain; using AWBlazorApp.Features.Production.ProductDocuments.Domain; using AWBlazorApp.Features.Production.ProductInventories.Domain; using AWBlazorApp.Features.Production.ProductListPriceHistories.Domain; using AWBlazorApp.Features.Production.ProductModels.Domain; using AWBlazorApp.Features.Production.ProductModelIllustrations.Domain; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Domain; using AWBlazorApp.Features.Production.ProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductReviews.Domain; using AWBlazorApp.Features.Production.Products.Domain; using AWBlazorApp.Features.Production.ProductSubcategories.Domain; using AWBlazorApp.Features.Production.ScrapReasons.Domain; using AWBlazorApp.Features.Production.TransactionHistories.Domain; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Domain; using AWBlazorApp.Features.Production.UnitMeasures.Domain; using AWBlazorApp.Features.Production.WorkOrders.Domain; using AWBlazorApp.Features.Production.WorkOrderRoutings.Domain; 

namespace AWBlazorApp.Features.Production.Products.Dtos;

public sealed record ProductDto(
    int Id, string Name, string ProductNumber, bool MakeFlag, bool FinishedGoodsFlag,
    string? Color, short SafetyStockLevel, short ReorderPoint,
    decimal StandardCost, decimal ListPrice,
    string? Size, string? SizeUnitMeasureCode, string? WeightUnitMeasureCode, decimal? Weight,
    int DaysToManufacture, string? ProductLine, string? Class, string? Style,
    int? ProductSubcategoryId, int? ProductModelId,
    DateTime SellStartDate, DateTime? SellEndDate, DateTime? DiscontinuedDate,
    Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateProductRequest
{
    public string? Name { get; set; }
    public string? ProductNumber { get; set; }
    public bool MakeFlag { get; set; }
    public bool FinishedGoodsFlag { get; set; }
    public string? Color { get; set; }
    public short SafetyStockLevel { get; set; }
    public short ReorderPoint { get; set; }
    public decimal StandardCost { get; set; }
    public decimal ListPrice { get; set; }
    public string? Size { get; set; }
    public string? SizeUnitMeasureCode { get; set; }
    public string? WeightUnitMeasureCode { get; set; }
    public decimal? Weight { get; set; }
    public int DaysToManufacture { get; set; }
    public string? ProductLine { get; set; }
    public string? Class { get; set; }
    public string? Style { get; set; }
    public int? ProductSubcategoryId { get; set; }
    public int? ProductModelId { get; set; }
    public DateTime SellStartDate { get; set; }
    public DateTime? SellEndDate { get; set; }
    public DateTime? DiscontinuedDate { get; set; }
}

public sealed record UpdateProductRequest
{
    public string? Name { get; set; }
    public string? ProductNumber { get; set; }
    public bool? MakeFlag { get; set; }
    public bool? FinishedGoodsFlag { get; set; }
    public string? Color { get; set; }
    public short? SafetyStockLevel { get; set; }
    public short? ReorderPoint { get; set; }
    public decimal? StandardCost { get; set; }
    public decimal? ListPrice { get; set; }
    public string? Size { get; set; }
    public string? SizeUnitMeasureCode { get; set; }
    public string? WeightUnitMeasureCode { get; set; }
    public decimal? Weight { get; set; }
    public int? DaysToManufacture { get; set; }
    public string? ProductLine { get; set; }
    public string? Class { get; set; }
    public string? Style { get; set; }
    public int? ProductSubcategoryId { get; set; }
    public int? ProductModelId { get; set; }
    public DateTime? SellStartDate { get; set; }
    public DateTime? SellEndDate { get; set; }
    public DateTime? DiscontinuedDate { get; set; }
}

public static class ProductMappings
{
    public static ProductDto ToDto(this Product e) => new(
        e.Id, e.Name, e.ProductNumber, e.MakeFlag, e.FinishedGoodsFlag,
        e.Color, e.SafetyStockLevel, e.ReorderPoint, e.StandardCost, e.ListPrice,
        e.Size, e.SizeUnitMeasureCode, e.WeightUnitMeasureCode, e.Weight,
        e.DaysToManufacture, e.ProductLine, e.Class, e.Style,
        e.ProductSubcategoryId, e.ProductModelId,
        e.SellStartDate, e.SellEndDate, e.DiscontinuedDate,
        e.RowGuid, e.ModifiedDate);

    public static Product ToEntity(this CreateProductRequest r) => new()
    {
        Name = (r.Name ?? string.Empty).Trim(),
        ProductNumber = (r.ProductNumber ?? string.Empty).Trim(),
        MakeFlag = r.MakeFlag,
        FinishedGoodsFlag = r.FinishedGoodsFlag,
        Color = TrimToNull(r.Color),
        SafetyStockLevel = r.SafetyStockLevel,
        ReorderPoint = r.ReorderPoint,
        StandardCost = r.StandardCost,
        ListPrice = r.ListPrice,
        Size = TrimToNull(r.Size),
        SizeUnitMeasureCode = TrimToNull(r.SizeUnitMeasureCode),
        WeightUnitMeasureCode = TrimToNull(r.WeightUnitMeasureCode),
        Weight = r.Weight,
        DaysToManufacture = r.DaysToManufacture,
        ProductLine = TrimToNull(r.ProductLine),
        Class = TrimToNull(r.Class),
        Style = TrimToNull(r.Style),
        ProductSubcategoryId = r.ProductSubcategoryId,
        ProductModelId = r.ProductModelId,
        SellStartDate = r.SellStartDate,
        SellEndDate = r.SellEndDate,
        DiscontinuedDate = r.DiscontinuedDate,
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateProductRequest r, Product e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.ProductNumber is not null) e.ProductNumber = r.ProductNumber.Trim();
        if (r.MakeFlag.HasValue) e.MakeFlag = r.MakeFlag.Value;
        if (r.FinishedGoodsFlag.HasValue) e.FinishedGoodsFlag = r.FinishedGoodsFlag.Value;
        if (r.Color is not null) e.Color = TrimToNull(r.Color);
        if (r.SafetyStockLevel.HasValue) e.SafetyStockLevel = r.SafetyStockLevel.Value;
        if (r.ReorderPoint.HasValue) e.ReorderPoint = r.ReorderPoint.Value;
        if (r.StandardCost.HasValue) e.StandardCost = r.StandardCost.Value;
        if (r.ListPrice.HasValue) e.ListPrice = r.ListPrice.Value;
        if (r.Size is not null) e.Size = TrimToNull(r.Size);
        if (r.SizeUnitMeasureCode is not null) e.SizeUnitMeasureCode = TrimToNull(r.SizeUnitMeasureCode);
        if (r.WeightUnitMeasureCode is not null) e.WeightUnitMeasureCode = TrimToNull(r.WeightUnitMeasureCode);
        if (r.Weight.HasValue) e.Weight = r.Weight.Value;
        if (r.DaysToManufacture.HasValue) e.DaysToManufacture = r.DaysToManufacture.Value;
        if (r.ProductLine is not null) e.ProductLine = TrimToNull(r.ProductLine);
        if (r.Class is not null) e.Class = TrimToNull(r.Class);
        if (r.Style is not null) e.Style = TrimToNull(r.Style);
        if (r.ProductSubcategoryId.HasValue) e.ProductSubcategoryId = r.ProductSubcategoryId.Value;
        if (r.ProductModelId.HasValue) e.ProductModelId = r.ProductModelId.Value;
        if (r.SellStartDate.HasValue) e.SellStartDate = r.SellStartDate.Value;
        if (r.SellEndDate.HasValue) e.SellEndDate = r.SellEndDate.Value;
        if (r.DiscontinuedDate.HasValue) e.DiscontinuedDate = r.DiscontinuedDate.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    private static string? TrimToNull(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
