using AWBlazorApp.Features.Sales.Domain;

namespace AWBlazorApp.Features.Sales.Dtos;

public sealed record CountryRegionCurrencyDto(
    string CountryRegionCode, string CurrencyCode, DateTime ModifiedDate);

public sealed record CreateCountryRegionCurrencyRequest
{
    public string? CountryRegionCode { get; set; }
    public string? CurrencyCode { get; set; }
}

/// <summary>
/// Pure junction table — there are no non-key columns to update beyond ModifiedDate, so
/// PATCH effectively just touches the row's modified-date stamp.
/// </summary>
public sealed record UpdateCountryRegionCurrencyRequest
{
    // Reserved for future expansion. Touching this row updates ModifiedDate.
}

public sealed record CountryRegionCurrencyAuditLogDto(
    int Id, string CountryRegionCode, string CurrencyCode, string Action,
    string? ChangedBy, DateTime ChangedDate, string? ChangeSummary, DateTime SourceModifiedDate);

public static class CountryRegionCurrencyMappings
{
    public static CountryRegionCurrencyDto ToDto(this CountryRegionCurrency e) => new(
        e.CountryRegionCode, e.CurrencyCode, e.ModifiedDate);

    public static CountryRegionCurrency ToEntity(this CreateCountryRegionCurrencyRequest r) => new()
    {
        CountryRegionCode = (r.CountryRegionCode ?? string.Empty).Trim(),
        CurrencyCode = (r.CurrencyCode ?? string.Empty).Trim(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateCountryRegionCurrencyRequest _, CountryRegionCurrency e)
    {
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static CountryRegionCurrencyAuditLogDto ToDto(this CountryRegionCurrencyAuditLog a) => new(
        a.Id, a.CountryRegionCode, a.CurrencyCode, a.Action, a.ChangedBy, a.ChangedDate,
        a.ChangeSummary, a.SourceModifiedDate);
}
