using AWBlazorApp.Features.AdventureWorks.Domain;

namespace AWBlazorApp.Features.AdventureWorks.Audit;

public static class CountryRegionCurrencyAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static CountryRegionCurrencyAuditLog RecordCreate(CountryRegionCurrency e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static CountryRegionCurrencyAuditLog RecordUpdate(CountryRegionCurrency e, string? by)
        => BuildLog(e, ActionUpdated, by, "Touched");

    public static CountryRegionCurrencyAuditLog RecordDelete(CountryRegionCurrency e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static CountryRegionCurrencyAuditLog BuildLog(
        CountryRegionCurrency e, string action, string? by, string? summary)
        => new()
        {
            CountryRegionCode = e.CountryRegionCode,
            CurrencyCode = e.CurrencyCode,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            SourceModifiedDate = e.ModifiedDate,
        };
}
