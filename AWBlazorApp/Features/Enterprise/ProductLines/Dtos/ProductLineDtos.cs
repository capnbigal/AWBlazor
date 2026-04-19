using AWBlazorApp.Features.Enterprise.Assets.Domain; using AWBlazorApp.Features.Enterprise.CostCenters.Domain; using AWBlazorApp.Features.Enterprise.OrgUnits.Domain; using AWBlazorApp.Features.Enterprise.Organizations.Domain; using AWBlazorApp.Features.Enterprise.ProductLines.Domain; using AWBlazorApp.Features.Enterprise.Stations.Domain; 

namespace AWBlazorApp.Features.Enterprise.ProductLines.Dtos;

public sealed record ProductLineDto(
    int Id, int OrganizationId, string Code, string Name, string? Description,
    bool IsActive, DateTime ModifiedDate);

public sealed record CreateProductLineRequest
{
    public int OrganizationId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed record UpdateProductLineRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

public sealed record ProductLineAuditLogDto(
    int Id, int ProductLineId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, int OrganizationId, string? Code, string? Name,
    string? Description, bool IsActive, DateTime SourceModifiedDate);

public static class ProductLineMappings
{
    public static ProductLineDto ToDto(this ProductLine e) => new(
        e.Id, e.OrganizationId, e.Code, e.Name, e.Description, e.IsActive, e.ModifiedDate);

    public static ProductLine ToEntity(this CreateProductLineRequest r) => new()
    {
        OrganizationId = r.OrganizationId,
        Code = (r.Code ?? string.Empty).Trim().ToUpperInvariant(),
        Name = (r.Name ?? string.Empty).Trim(),
        Description = r.Description?.Trim(),
        IsActive = r.IsActive,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateProductLineRequest r, ProductLine e)
    {
        if (r.Code is not null) e.Code = r.Code.Trim().ToUpperInvariant();
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.Description is not null) e.Description = r.Description.Trim();
        if (r.IsActive.HasValue) e.IsActive = r.IsActive.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static ProductLineAuditLogDto ToDto(this ProductLineAuditLog a) => new(
        a.Id, a.ProductLineId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.OrganizationId, a.Code, a.Name, a.Description, a.IsActive, a.SourceModifiedDate);
}
