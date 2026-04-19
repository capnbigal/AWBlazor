using AWBlazorApp.Features.Enterprise.Assets.Domain; using AWBlazorApp.Features.Enterprise.CostCenters.Domain; using AWBlazorApp.Features.Enterprise.OrgUnits.Domain; using AWBlazorApp.Features.Enterprise.Organizations.Domain; using AWBlazorApp.Features.Enterprise.ProductLines.Domain; using AWBlazorApp.Features.Enterprise.Stations.Domain; 

namespace AWBlazorApp.Features.Enterprise.CostCenters.Dtos;

public sealed record CostCenterDto(
    int Id, int OrganizationId, string Code, string Name,
    int? OwnerBusinessEntityId, bool IsActive, DateTime ModifiedDate);

public sealed record CreateCostCenterRequest
{
    public int OrganizationId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public int? OwnerBusinessEntityId { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed record UpdateCostCenterRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public int? OwnerBusinessEntityId { get; set; }
    public bool? IsActive { get; set; }
}

public sealed record CostCenterAuditLogDto(
    int Id, int CostCenterId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, int OrganizationId, string? Code, string? Name,
    int? OwnerBusinessEntityId, bool IsActive, DateTime SourceModifiedDate);

public static class CostCenterMappings
{
    public static CostCenterDto ToDto(this CostCenter e) => new(
        e.Id, e.OrganizationId, e.Code, e.Name, e.OwnerBusinessEntityId, e.IsActive, e.ModifiedDate);

    public static CostCenter ToEntity(this CreateCostCenterRequest r) => new()
    {
        OrganizationId = r.OrganizationId,
        Code = (r.Code ?? string.Empty).Trim().ToUpperInvariant(),
        Name = (r.Name ?? string.Empty).Trim(),
        OwnerBusinessEntityId = r.OwnerBusinessEntityId,
        IsActive = r.IsActive,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateCostCenterRequest r, CostCenter e)
    {
        if (r.Code is not null) e.Code = r.Code.Trim().ToUpperInvariant();
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.OwnerBusinessEntityId is not null) e.OwnerBusinessEntityId = r.OwnerBusinessEntityId;
        if (r.IsActive.HasValue) e.IsActive = r.IsActive.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static CostCenterAuditLogDto ToDto(this CostCenterAuditLog a) => new(
        a.Id, a.CostCenterId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.OrganizationId, a.Code, a.Name, a.OwnerBusinessEntityId, a.IsActive, a.SourceModifiedDate);
}
