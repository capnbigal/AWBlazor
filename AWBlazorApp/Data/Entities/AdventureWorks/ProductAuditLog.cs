using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Data.Entities.AdventureWorks;

/// <summary>Audit log for <see cref="Product"/>. EF-managed table <c>dbo.ProductAuditLogs</c>.</summary>
public class ProductAuditLog : AdventureWorksAuditLogBase
{
    public int ProductId { get; set; }

    [MaxLength(50)] public string? Name { get; set; }
    [MaxLength(25)] public string? ProductNumber { get; set; }
    public bool MakeFlag { get; set; }
    public bool FinishedGoodsFlag { get; set; }
    [MaxLength(15)] public string? Color { get; set; }
    public short SafetyStockLevel { get; set; }
    public short ReorderPoint { get; set; }
    public decimal StandardCost { get; set; }
    public decimal ListPrice { get; set; }
    [MaxLength(5)] public string? Size { get; set; }
    [MaxLength(3)] public string? SizeUnitMeasureCode { get; set; }
    [MaxLength(3)] public string? WeightUnitMeasureCode { get; set; }
    public decimal? Weight { get; set; }
    public int DaysToManufacture { get; set; }
    [MaxLength(2)] public string? ProductLine { get; set; }
    [MaxLength(2)] public string? Class { get; set; }
    [MaxLength(2)] public string? Style { get; set; }
    public int? ProductSubcategoryId { get; set; }
    public int? ProductModelId { get; set; }
    public DateTime SellStartDate { get; set; }
    public DateTime? SellEndDate { get; set; }
    public DateTime? DiscontinuedDate { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
