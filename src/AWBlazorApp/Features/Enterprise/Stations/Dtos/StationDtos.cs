using AWBlazorApp.Features.Enterprise.Assets.Domain; using AWBlazorApp.Features.Enterprise.CostCenters.Domain; using AWBlazorApp.Features.Enterprise.OrgUnits.Domain; using AWBlazorApp.Features.Enterprise.Organizations.Domain; using AWBlazorApp.Features.Enterprise.ProductLines.Domain; using AWBlazorApp.Features.Enterprise.Stations.Domain; 

namespace AWBlazorApp.Features.Enterprise.Stations.Dtos;

public sealed record StationDto(
    int Id, int OrgUnitId, string Code, string Name, StationKind StationKind,
    int? OperatorBusinessEntityId, int? AssetId, decimal? IdealCycleSeconds,
    bool IsActive, DateTime ModifiedDate);

public sealed record CreateStationRequest
{
    public int OrgUnitId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public StationKind StationKind { get; set; } = StationKind.Workstation;
    public int? OperatorBusinessEntityId { get; set; }
    public int? AssetId { get; set; }
    public decimal? IdealCycleSeconds { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed record UpdateStationRequest
{
    public int? OrgUnitId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public StationKind? StationKind { get; set; }
    public int? OperatorBusinessEntityId { get; set; }
    public int? AssetId { get; set; }
    public decimal? IdealCycleSeconds { get; set; }
    public bool? IsActive { get; set; }
}

public static class StationMappings
{
    public static StationDto ToDto(this Station e) => new(
        e.Id, e.OrgUnitId, e.Code, e.Name, e.StationKind,
        e.OperatorBusinessEntityId, e.AssetId, e.IdealCycleSeconds,
        e.IsActive, e.ModifiedDate);

    public static Station ToEntity(this CreateStationRequest r) => new()
    {
        OrgUnitId = r.OrgUnitId,
        Code = (r.Code ?? string.Empty).Trim().ToUpperInvariant(),
        Name = (r.Name ?? string.Empty).Trim(),
        StationKind = r.StationKind,
        OperatorBusinessEntityId = r.OperatorBusinessEntityId,
        AssetId = r.AssetId,
        IdealCycleSeconds = r.IdealCycleSeconds,
        IsActive = r.IsActive,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateStationRequest r, Station e)
    {
        if (r.OrgUnitId.HasValue) e.OrgUnitId = r.OrgUnitId.Value;
        if (r.Code is not null) e.Code = r.Code.Trim().ToUpperInvariant();
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.StationKind.HasValue) e.StationKind = r.StationKind.Value;
        if (r.OperatorBusinessEntityId is not null) e.OperatorBusinessEntityId = r.OperatorBusinessEntityId;
        if (r.AssetId is not null) e.AssetId = r.AssetId;
        if (r.IdealCycleSeconds is not null) e.IdealCycleSeconds = r.IdealCycleSeconds;
        if (r.IsActive.HasValue) e.IsActive = r.IsActive.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
