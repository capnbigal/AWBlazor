using AWBlazorApp.Features.Enterprise.Assets.Domain; using AWBlazorApp.Features.Enterprise.CostCenters.Domain; using AWBlazorApp.Features.Enterprise.OrgUnits.Domain; using AWBlazorApp.Features.Enterprise.Organizations.Domain; using AWBlazorApp.Features.Enterprise.ProductLines.Domain; using AWBlazorApp.Features.Enterprise.Stations.Domain; 

namespace AWBlazorApp.Features.Enterprise.OrgUnits.Dtos;

public sealed record OrgUnitDto(
    int Id, int OrganizationId, int? ParentOrgUnitId, OrgUnitKind Kind,
    string Code, string Name, string Path, byte Depth,
    int? CostCenterId, int? ManagerBusinessEntityId, bool IsActive, DateTime ModifiedDate);

public sealed record CreateOrgUnitRequest
{
    public int OrganizationId { get; set; }
    public int? ParentOrgUnitId { get; set; }
    public OrgUnitKind Kind { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public int? CostCenterId { get; set; }
    public int? ManagerBusinessEntityId { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed record UpdateOrgUnitRequest
{
    public int? ParentOrgUnitId { get; set; }
    public OrgUnitKind? Kind { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public int? CostCenterId { get; set; }
    public int? ManagerBusinessEntityId { get; set; }
    public bool? IsActive { get; set; }
}

public static class OrgUnitMappings
{
    public static OrgUnitDto ToDto(this OrgUnit e) => new(
        e.Id, e.OrganizationId, e.ParentOrgUnitId, e.Kind, e.Code, e.Name, e.Path, e.Depth,
        e.CostCenterId, e.ManagerBusinessEntityId, e.IsActive, e.ModifiedDate);

    public static OrgUnit ToEntity(this CreateOrgUnitRequest r) => new()
    {
        OrganizationId = r.OrganizationId,
        ParentOrgUnitId = r.ParentOrgUnitId,
        Kind = r.Kind,
        Code = (r.Code ?? string.Empty).Trim().ToUpperInvariant(),
        Name = (r.Name ?? string.Empty).Trim(),
        Path = string.Empty, // populated by endpoint after resolving parent
        Depth = 0,
        CostCenterId = r.CostCenterId,
        ManagerBusinessEntityId = r.ManagerBusinessEntityId,
        IsActive = r.IsActive,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateOrgUnitRequest r, OrgUnit e)
    {
        if (r.ParentOrgUnitId is not null) e.ParentOrgUnitId = r.ParentOrgUnitId;
        if (r.Kind.HasValue) e.Kind = r.Kind.Value;
        if (r.Code is not null) e.Code = r.Code.Trim().ToUpperInvariant();
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.CostCenterId is not null) e.CostCenterId = r.CostCenterId;
        if (r.ManagerBusinessEntityId is not null) e.ManagerBusinessEntityId = r.ManagerBusinessEntityId;
        if (r.IsActive.HasValue) e.IsActive = r.IsActive.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
