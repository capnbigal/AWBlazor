using AWBlazorApp.Features.AdventureWorks.Domain;

namespace AWBlazorApp.Features.AdventureWorks.Models;

public sealed record CurrencyDto(string CurrencyCode, string Name, DateTime ModifiedDate);

public sealed record CreateCurrencyRequest
{
    public string? CurrencyCode { get; set; }
    public string? Name { get; set; }
}

public sealed record UpdateCurrencyRequest
{
    public string? Name { get; set; }
}

public sealed record CurrencyAuditLogDto(
    int Id, string CurrencyCode, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? Name, DateTime SourceModifiedDate);

public static class CurrencyMappings
{
    public static CurrencyDto ToDto(this Currency e) => new(e.CurrencyCode, e.Name, e.ModifiedDate);

    public static Currency ToEntity(this CreateCurrencyRequest r) => new()
    {
        CurrencyCode = (r.CurrencyCode ?? string.Empty).Trim(),
        Name = (r.Name ?? string.Empty).Trim(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateCurrencyRequest r, Currency e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static CurrencyAuditLogDto ToDto(this CurrencyAuditLog a) => new(
        a.Id, a.CurrencyCode, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Name, a.SourceModifiedDate);
}
