using AWBlazorApp.Features.Engineering.Boms.Domain; using AWBlazorApp.Features.Engineering.Deviations.Domain; using AWBlazorApp.Features.Engineering.Documents.Domain; using AWBlazorApp.Features.Engineering.Ecos.Domain; using AWBlazorApp.Features.Engineering.Routings.Domain; 

namespace AWBlazorApp.Features.Engineering.Boms.Dtos;

public sealed record BomHeaderDto(
    int Id, string Code, string Name, string? Description,
    int ProductId, int RevisionNumber, bool IsActive, DateTime ModifiedDate);

public sealed record CreateBomHeaderRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int ProductId { get; set; }
    public int RevisionNumber { get; set; } = 1;
    public bool IsActive { get; set; } = false;
}

public sealed record UpdateBomHeaderRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? RevisionNumber { get; set; }
    public bool? IsActive { get; set; }
}

public sealed record BomHeaderAuditLogDto(
    int Id, int BomHeaderId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    string? Code, string? Name, string? Description,
    int ProductId, int RevisionNumber, bool IsActive, DateTime SourceModifiedDate);

public sealed record BomLineDto(
    int Id, int BomHeaderId, int ComponentProductId, decimal Quantity,
    string UnitMeasureCode, decimal ScrapPercentage, string? Notes, DateTime ModifiedDate);

public sealed record CreateBomLineRequest
{
    public int BomHeaderId { get; set; }
    public int ComponentProductId { get; set; }
    public decimal Quantity { get; set; }
    public string UnitMeasureCode { get; set; } = "EA";
    public decimal ScrapPercentage { get; set; }
    public string? Notes { get; set; }
}

public sealed record UpdateBomLineRequest
{
    public decimal? Quantity { get; set; }
    public string? UnitMeasureCode { get; set; }
    public decimal? ScrapPercentage { get; set; }
    public string? Notes { get; set; }
}

public static class BomMappings
{
    public static BomHeaderDto ToDto(this BomHeader e) => new(
        e.Id, e.Code, e.Name, e.Description,
        e.ProductId, e.RevisionNumber, e.IsActive, e.ModifiedDate);

    public static BomHeader ToEntity(this CreateBomHeaderRequest r) => new()
    {
        Code = (r.Code ?? string.Empty).Trim().ToUpperInvariant(),
        Name = (r.Name ?? string.Empty).Trim(),
        Description = r.Description?.Trim(),
        ProductId = r.ProductId,
        RevisionNumber = r.RevisionNumber,
        IsActive = r.IsActive,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateBomHeaderRequest r, BomHeader e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.Description is not null) e.Description = r.Description.Trim();
        if (r.RevisionNumber is not null) e.RevisionNumber = r.RevisionNumber.Value;
        if (r.IsActive is not null) e.IsActive = r.IsActive.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static BomHeaderAuditLogDto ToDto(this BomHeaderAuditLog a) => new(
        a.Id, a.BomHeaderId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Code, a.Name, a.Description, a.ProductId, a.RevisionNumber, a.IsActive, a.SourceModifiedDate);

    public static BomLineDto ToDto(this BomLine e) => new(
        e.Id, e.BomHeaderId, e.ComponentProductId, e.Quantity,
        e.UnitMeasureCode, e.ScrapPercentage, e.Notes, e.ModifiedDate);

    public static BomLine ToEntity(this CreateBomLineRequest r) => new()
    {
        BomHeaderId = r.BomHeaderId,
        ComponentProductId = r.ComponentProductId,
        Quantity = r.Quantity,
        UnitMeasureCode = (r.UnitMeasureCode ?? "EA").Trim().ToUpperInvariant(),
        ScrapPercentage = r.ScrapPercentage,
        Notes = r.Notes?.Trim(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateBomLineRequest r, BomLine e)
    {
        if (r.Quantity is not null) e.Quantity = r.Quantity.Value;
        if (r.UnitMeasureCode is not null) e.UnitMeasureCode = r.UnitMeasureCode.Trim().ToUpperInvariant();
        if (r.ScrapPercentage is not null) e.ScrapPercentage = r.ScrapPercentage.Value;
        if (r.Notes is not null) e.Notes = r.Notes.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }
}
