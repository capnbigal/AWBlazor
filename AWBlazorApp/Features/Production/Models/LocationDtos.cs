using AWBlazorApp.Features.Production.Domain;

namespace AWBlazorApp.Features.Production.Models;

public sealed record LocationDto(short Id, string Name, decimal CostRate, decimal Availability, DateTime ModifiedDate);

public sealed record CreateLocationRequest
{
    public string? Name { get; set; }
    public decimal CostRate { get; set; }
    public decimal Availability { get; set; }
}

public sealed record UpdateLocationRequest
{
    public string? Name { get; set; }
    public decimal? CostRate { get; set; }
    public decimal? Availability { get; set; }
}

public sealed record LocationAuditLogDto(
    int Id, short LocationId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? Name, decimal CostRate, decimal Availability, DateTime SourceModifiedDate);

public static class LocationMappings
{
    public static LocationDto ToDto(this Location e) => new(e.Id, e.Name, e.CostRate, e.Availability, e.ModifiedDate);

    public static Location ToEntity(this CreateLocationRequest r) => new()
    {
        Name = (r.Name ?? string.Empty).Trim(),
        CostRate = r.CostRate,
        Availability = r.Availability,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateLocationRequest r, Location e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.CostRate.HasValue) e.CostRate = r.CostRate.Value;
        if (r.Availability.HasValue) e.Availability = r.Availability.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static LocationAuditLogDto ToDto(this LocationAuditLog a) => new(
        a.Id, a.LocationId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Name, a.CostRate, a.Availability, a.SourceModifiedDate);
}
