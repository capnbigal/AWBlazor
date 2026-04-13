using AWBlazorApp.Data.Entities.AdventureWorks;

namespace AWBlazorApp.Models.AdventureWorks;

public sealed record CultureDto(string CultureId, string Name, DateTime ModifiedDate);

public sealed record CreateCultureRequest
{
    public string? CultureId { get; set; }
    public string? Name { get; set; }
}

public sealed record UpdateCultureRequest
{
    public string? Name { get; set; }
}

public sealed record CultureAuditLogDto(
    int Id, string CultureId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? Name, DateTime SourceModifiedDate);

public static class CultureMappings
{
    public static CultureDto ToDto(this Culture e) => new(e.CultureId, e.Name, e.ModifiedDate);

    public static Culture ToEntity(this CreateCultureRequest r) => new()
    {
        CultureId = (r.CultureId ?? string.Empty).Trim(),
        Name = (r.Name ?? string.Empty).Trim(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateCultureRequest r, Culture e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static CultureAuditLogDto ToDto(this CultureAuditLog a) => new(
        a.Id, a.CultureId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Name, a.SourceModifiedDate);
}
