using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Models.AdventureWorks;

public sealed record StateProvinceDto(
    int Id, string StateProvinceCode, string CountryRegionCode, bool IsOnlyStateProvinceFlag,
    string Name, int TerritoryId, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateStateProvinceRequest
{
    public string? StateProvinceCode { get; set; }
    public string? CountryRegionCode { get; set; }
    public bool IsOnlyStateProvinceFlag { get; set; }
    public string? Name { get; set; }
    public int TerritoryId { get; set; }
}

public sealed record UpdateStateProvinceRequest
{
    public string? StateProvinceCode { get; set; }
    public string? CountryRegionCode { get; set; }
    public bool? IsOnlyStateProvinceFlag { get; set; }
    public string? Name { get; set; }
    public int? TerritoryId { get; set; }
}

public sealed record StateProvinceAuditLogDto(
    int Id, int StateProvinceId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? StateProvinceCode, string? CountryRegionCode, bool IsOnlyStateProvinceFlag,
    string? Name, int TerritoryId, Guid RowGuid, DateTime SourceModifiedDate);

public static class StateProvinceMappings
{
    public static StateProvinceDto ToDto(this StateProvince e) => new(
        e.Id, e.StateProvinceCode, e.CountryRegionCode, e.IsOnlyStateProvinceFlag,
        e.Name, e.TerritoryId, e.RowGuid, e.ModifiedDate);

    public static StateProvince ToEntity(this CreateStateProvinceRequest r) => new()
    {
        StateProvinceCode = (r.StateProvinceCode ?? string.Empty).Trim(),
        CountryRegionCode = (r.CountryRegionCode ?? string.Empty).Trim(),
        IsOnlyStateProvinceFlag = r.IsOnlyStateProvinceFlag,
        Name = (r.Name ?? string.Empty).Trim(),
        TerritoryId = r.TerritoryId,
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateStateProvinceRequest r, StateProvince e)
    {
        if (r.StateProvinceCode is not null) e.StateProvinceCode = r.StateProvinceCode.Trim();
        if (r.CountryRegionCode is not null) e.CountryRegionCode = r.CountryRegionCode.Trim();
        if (r.IsOnlyStateProvinceFlag.HasValue) e.IsOnlyStateProvinceFlag = r.IsOnlyStateProvinceFlag.Value;
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.TerritoryId.HasValue) e.TerritoryId = r.TerritoryId.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static StateProvinceAuditLogDto ToDto(this StateProvinceAuditLog a) => new(
        a.Id, a.StateProvinceId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.StateProvinceCode, a.CountryRegionCode, a.IsOnlyStateProvinceFlag,
        a.Name, a.TerritoryId, a.RowGuid, a.SourceModifiedDate);
}
