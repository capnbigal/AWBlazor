using AWBlazorApp.Features.Enterprise.Assets.Domain; using AWBlazorApp.Features.Enterprise.CostCenters.Domain; using AWBlazorApp.Features.Enterprise.OrgUnits.Domain; using AWBlazorApp.Features.Enterprise.Organizations.Domain; using AWBlazorApp.Features.Enterprise.ProductLines.Domain; using AWBlazorApp.Features.Enterprise.Stations.Domain; 

namespace AWBlazorApp.Features.Enterprise.Organizations.Dtos;

public sealed record OrganizationDto(
    int Id, string Code, string Name, bool IsPrimary,
    int? ParentOrganizationId, string? ExternalRef, bool IsActive, DateTime ModifiedDate);

public sealed record CreateOrganizationRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public bool IsPrimary { get; set; }
    public int? ParentOrganizationId { get; set; }
    public string? ExternalRef { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed record UpdateOrganizationRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public bool? IsPrimary { get; set; }
    public int? ParentOrganizationId { get; set; }
    public string? ExternalRef { get; set; }
    public bool? IsActive { get; set; }
}

public static class OrganizationMappings
{
    public static OrganizationDto ToDto(this Organization e) => new(
        e.Id, e.Code, e.Name, e.IsPrimary, e.ParentOrganizationId, e.ExternalRef, e.IsActive, e.ModifiedDate);

    public static Organization ToEntity(this CreateOrganizationRequest r) => new()
    {
        Code = (r.Code ?? string.Empty).Trim().ToUpperInvariant(),
        Name = (r.Name ?? string.Empty).Trim(),
        IsPrimary = r.IsPrimary,
        ParentOrganizationId = r.ParentOrganizationId,
        ExternalRef = r.ExternalRef?.Trim(),
        IsActive = r.IsActive,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateOrganizationRequest r, Organization e)
    {
        if (r.Code is not null) e.Code = r.Code.Trim().ToUpperInvariant();
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.IsPrimary.HasValue) e.IsPrimary = r.IsPrimary.Value;
        if (r.ParentOrganizationId is not null) e.ParentOrganizationId = r.ParentOrganizationId;
        if (r.ExternalRef is not null) e.ExternalRef = r.ExternalRef.Trim();
        if (r.IsActive.HasValue) e.IsActive = r.IsActive.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
