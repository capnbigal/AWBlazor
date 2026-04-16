using AWBlazorApp.Features.AdventureWorks.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Purchasing.Domain;

/// <summary>Audit log for <see cref="ProductVendor"/>. EF-managed table <c>dbo.ProductVendorAuditLogs</c>.</summary>
public class ProductVendorAuditLog : AdventureWorksAuditLogBase
{
    public int ProductId { get; set; }
    public int BusinessEntityId { get; set; }

    public int AverageLeadTime { get; set; }
    public decimal StandardPrice { get; set; }
    public decimal? LastReceiptCost { get; set; }
    public DateTime? LastReceiptDate { get; set; }
    public int MinOrderQty { get; set; }
    public int MaxOrderQty { get; set; }
    public int? OnOrderQty { get; set; }
    [MaxLength(3)] public string? UnitMeasureCode { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
