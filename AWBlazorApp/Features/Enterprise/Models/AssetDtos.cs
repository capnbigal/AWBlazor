using AWBlazorApp.Features.Enterprise.Domain;

namespace AWBlazorApp.Features.Enterprise.Models;

public sealed record AssetDto(
    int Id, int OrganizationId, int? OrgUnitId, string AssetTag, string Name,
    string? Manufacturer, string? Model, string? SerialNumber,
    AssetType AssetType, DateTime? CommissionedAt, DateTime? DecommissionedAt,
    AssetStatus Status, int? ParentAssetId, DateTime ModifiedDate);

public sealed record CreateAssetRequest
{
    public int OrganizationId { get; set; }
    public int? OrgUnitId { get; set; }
    public string? AssetTag { get; set; }
    public string? Name { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public AssetType AssetType { get; set; } = AssetType.Machine;
    public DateTime? CommissionedAt { get; set; }
    public DateTime? DecommissionedAt { get; set; }
    public AssetStatus Status { get; set; } = AssetStatus.Active;
    public int? ParentAssetId { get; set; }
}

public sealed record UpdateAssetRequest
{
    public int? OrgUnitId { get; set; }
    public string? AssetTag { get; set; }
    public string? Name { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public AssetType? AssetType { get; set; }
    public DateTime? CommissionedAt { get; set; }
    public DateTime? DecommissionedAt { get; set; }
    public AssetStatus? Status { get; set; }
    public int? ParentAssetId { get; set; }
}

public sealed record AssetAuditLogDto(
    int Id, int AssetId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, int OrganizationId, int? OrgUnitId, string? AssetTag, string? Name,
    string? Manufacturer, string? Model, string? SerialNumber,
    AssetType AssetType, DateTime? CommissionedAt, DateTime? DecommissionedAt,
    AssetStatus Status, int? ParentAssetId, DateTime SourceModifiedDate);

public static class AssetMappings
{
    public static AssetDto ToDto(this Asset e) => new(
        e.Id, e.OrganizationId, e.OrgUnitId, e.AssetTag, e.Name,
        e.Manufacturer, e.Model, e.SerialNumber,
        e.AssetType, e.CommissionedAt, e.DecommissionedAt,
        e.Status, e.ParentAssetId, e.ModifiedDate);

    public static Asset ToEntity(this CreateAssetRequest r) => new()
    {
        OrganizationId = r.OrganizationId,
        OrgUnitId = r.OrgUnitId,
        AssetTag = (r.AssetTag ?? string.Empty).Trim().ToUpperInvariant(),
        Name = (r.Name ?? string.Empty).Trim(),
        Manufacturer = r.Manufacturer?.Trim(),
        Model = r.Model?.Trim(),
        SerialNumber = r.SerialNumber?.Trim(),
        AssetType = r.AssetType,
        CommissionedAt = r.CommissionedAt,
        DecommissionedAt = r.DecommissionedAt,
        Status = r.Status,
        ParentAssetId = r.ParentAssetId,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateAssetRequest r, Asset e)
    {
        if (r.OrgUnitId is not null) e.OrgUnitId = r.OrgUnitId;
        if (r.AssetTag is not null) e.AssetTag = r.AssetTag.Trim().ToUpperInvariant();
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.Manufacturer is not null) e.Manufacturer = r.Manufacturer.Trim();
        if (r.Model is not null) e.Model = r.Model.Trim();
        if (r.SerialNumber is not null) e.SerialNumber = r.SerialNumber.Trim();
        if (r.AssetType.HasValue) e.AssetType = r.AssetType.Value;
        if (r.CommissionedAt is not null) e.CommissionedAt = r.CommissionedAt;
        if (r.DecommissionedAt is not null) e.DecommissionedAt = r.DecommissionedAt;
        if (r.Status.HasValue) e.Status = r.Status.Value;
        if (r.ParentAssetId is not null) e.ParentAssetId = r.ParentAssetId;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static AssetAuditLogDto ToDto(this AssetAuditLog a) => new(
        a.Id, a.AssetId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.OrganizationId, a.OrgUnitId, a.AssetTag, a.Name,
        a.Manufacturer, a.Model, a.SerialNumber,
        a.AssetType, a.CommissionedAt, a.DecommissionedAt,
        a.Status, a.ParentAssetId, a.SourceModifiedDate);
}
