using AWBlazorApp.Features.Person.Domain;

namespace AWBlazorApp.Features.Person.Dtos;

public sealed record CountryRegionDto(string CountryRegionCode, string Name, DateTime ModifiedDate);

public sealed record CreateCountryRegionRequest
{
    public string? CountryRegionCode { get; set; }
    public string? Name { get; set; }
}

public sealed record UpdateCountryRegionRequest
{
    public string? Name { get; set; }
}

public sealed record CountryRegionAuditLogDto(
    int Id, string CountryRegionCode, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? Name, DateTime SourceModifiedDate);

public static class CountryRegionMappings
{
    public static CountryRegionDto ToDto(this CountryRegion e)
        => new(e.CountryRegionCode, e.Name, e.ModifiedDate);

    public static CountryRegion ToEntity(this CreateCountryRegionRequest r) => new()
    {
        CountryRegionCode = (r.CountryRegionCode ?? string.Empty).Trim(),
        Name = (r.Name ?? string.Empty).Trim(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateCountryRegionRequest r, CountryRegion e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static CountryRegionAuditLogDto ToDto(this CountryRegionAuditLog a) => new(
        a.Id, a.CountryRegionCode, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Name, a.SourceModifiedDate);
}
