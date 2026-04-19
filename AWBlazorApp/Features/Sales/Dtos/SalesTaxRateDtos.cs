using AWBlazorApp.Features.Sales.Domain;

namespace AWBlazorApp.Features.Sales.Dtos;

public sealed record SalesTaxRateDto(
    int Id, int StateProvinceId, byte TaxType, decimal TaxRate, string Name, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateSalesTaxRateRequest
{
    public int StateProvinceId { get; set; }
    public byte TaxType { get; set; }
    public decimal TaxRate { get; set; }
    public string? Name { get; set; }
}

public sealed record UpdateSalesTaxRateRequest
{
    public int? StateProvinceId { get; set; }
    public byte? TaxType { get; set; }
    public decimal? TaxRate { get; set; }
    public string? Name { get; set; }
}

public sealed record SalesTaxRateAuditLogDto(
    int Id, int SalesTaxRateId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, int StateProvinceId, byte TaxType, decimal TaxRate, string? Name,
    Guid RowGuid, DateTime SourceModifiedDate);

public static class SalesTaxRateMappings
{
    public static SalesTaxRateDto ToDto(this SalesTaxRate e)
        => new(e.Id, e.StateProvinceId, e.TaxType, e.TaxRate, e.Name, e.RowGuid, e.ModifiedDate);

    public static SalesTaxRate ToEntity(this CreateSalesTaxRateRequest r) => new()
    {
        StateProvinceId = r.StateProvinceId,
        TaxType = r.TaxType,
        TaxRate = r.TaxRate,
        Name = (r.Name ?? string.Empty).Trim(),
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateSalesTaxRateRequest r, SalesTaxRate e)
    {
        if (r.StateProvinceId.HasValue) e.StateProvinceId = r.StateProvinceId.Value;
        if (r.TaxType.HasValue) e.TaxType = r.TaxType.Value;
        if (r.TaxRate.HasValue) e.TaxRate = r.TaxRate.Value;
        if (r.Name is not null) e.Name = r.Name.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static SalesTaxRateAuditLogDto ToDto(this SalesTaxRateAuditLog a) => new(
        a.Id, a.SalesTaxRateId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.StateProvinceId, a.TaxType, a.TaxRate, a.Name, a.RowGuid, a.SourceModifiedDate);
}
