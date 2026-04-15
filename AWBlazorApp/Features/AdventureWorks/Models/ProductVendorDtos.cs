using AWBlazorApp.Features.AdventureWorks.Domain;

namespace AWBlazorApp.Features.AdventureWorks.Models;

public sealed record ProductVendorDto(
    int ProductId, int BusinessEntityId, int AverageLeadTime, decimal StandardPrice,
    decimal? LastReceiptCost, DateTime? LastReceiptDate, int MinOrderQty, int MaxOrderQty,
    int? OnOrderQty, string UnitMeasureCode, DateTime ModifiedDate);

public sealed record CreateProductVendorRequest
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
    public string? UnitMeasureCode { get; set; }
}

public sealed record UpdateProductVendorRequest
{
    public int? AverageLeadTime { get; set; }
    public decimal? StandardPrice { get; set; }
    public decimal? LastReceiptCost { get; set; }
    public DateTime? LastReceiptDate { get; set; }
    public int? MinOrderQty { get; set; }
    public int? MaxOrderQty { get; set; }
    public int? OnOrderQty { get; set; }
    public string? UnitMeasureCode { get; set; }
}

public sealed record ProductVendorAuditLogDto(
    int Id, int ProductId, int BusinessEntityId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, int AverageLeadTime, decimal StandardPrice,
    decimal? LastReceiptCost, DateTime? LastReceiptDate, int MinOrderQty, int MaxOrderQty,
    int? OnOrderQty, string? UnitMeasureCode, DateTime SourceModifiedDate);

public static class ProductVendorMappings
{
    public static ProductVendorDto ToDto(this ProductVendor e) => new(
        e.ProductId, e.BusinessEntityId, e.AverageLeadTime, e.StandardPrice,
        e.LastReceiptCost, e.LastReceiptDate, e.MinOrderQty, e.MaxOrderQty,
        e.OnOrderQty, e.UnitMeasureCode, e.ModifiedDate);

    public static ProductVendor ToEntity(this CreateProductVendorRequest r) => new()
    {
        ProductId = r.ProductId,
        BusinessEntityId = r.BusinessEntityId,
        AverageLeadTime = r.AverageLeadTime,
        StandardPrice = r.StandardPrice,
        LastReceiptCost = r.LastReceiptCost,
        LastReceiptDate = r.LastReceiptDate,
        MinOrderQty = r.MinOrderQty,
        MaxOrderQty = r.MaxOrderQty,
        OnOrderQty = r.OnOrderQty,
        UnitMeasureCode = (r.UnitMeasureCode ?? string.Empty).Trim(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateProductVendorRequest r, ProductVendor e)
    {
        if (r.AverageLeadTime.HasValue) e.AverageLeadTime = r.AverageLeadTime.Value;
        if (r.StandardPrice.HasValue) e.StandardPrice = r.StandardPrice.Value;
        if (r.LastReceiptCost.HasValue) e.LastReceiptCost = r.LastReceiptCost.Value;
        if (r.LastReceiptDate.HasValue) e.LastReceiptDate = r.LastReceiptDate.Value;
        if (r.MinOrderQty.HasValue) e.MinOrderQty = r.MinOrderQty.Value;
        if (r.MaxOrderQty.HasValue) e.MaxOrderQty = r.MaxOrderQty.Value;
        if (r.OnOrderQty.HasValue) e.OnOrderQty = r.OnOrderQty.Value;
        if (r.UnitMeasureCode is not null) e.UnitMeasureCode = r.UnitMeasureCode.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static ProductVendorAuditLogDto ToDto(this ProductVendorAuditLog a) => new(
        a.Id, a.ProductId, a.BusinessEntityId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.AverageLeadTime, a.StandardPrice,
        a.LastReceiptCost, a.LastReceiptDate, a.MinOrderQty, a.MaxOrderQty,
        a.OnOrderQty, a.UnitMeasureCode, a.SourceModifiedDate);
}
