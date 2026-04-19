using AWBlazorApp.Features.Production.Domain;

namespace AWBlazorApp.Features.Production.Dtos;

public sealed record UnitMeasureDto(string UnitMeasureCode, string Name, DateTime ModifiedDate);

public sealed record CreateUnitMeasureRequest
{
    public string? UnitMeasureCode { get; set; }
    public string? Name { get; set; }
}

public sealed record UpdateUnitMeasureRequest
{
    public string? Name { get; set; }
}

public sealed record UnitMeasureAuditLogDto(
    int Id, string UnitMeasureCode, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? Name, DateTime SourceModifiedDate);

public static class UnitMeasureMappings
{
    public static UnitMeasureDto ToDto(this UnitMeasure e) => new(e.UnitMeasureCode, e.Name, e.ModifiedDate);

    public static UnitMeasure ToEntity(this CreateUnitMeasureRequest r) => new()
    {
        UnitMeasureCode = (r.UnitMeasureCode ?? string.Empty).Trim(),
        Name = (r.Name ?? string.Empty).Trim(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateUnitMeasureRequest r, UnitMeasure e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static UnitMeasureAuditLogDto ToDto(this UnitMeasureAuditLog a) => new(
        a.Id, a.UnitMeasureCode, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Name, a.SourceModifiedDate);
}
