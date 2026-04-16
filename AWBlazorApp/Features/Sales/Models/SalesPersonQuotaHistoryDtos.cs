using AWBlazorApp.Features.Sales.Domain;

namespace AWBlazorApp.Features.Sales.Models;

public sealed record SalesPersonQuotaHistoryDto(
    int BusinessEntityId, DateTime QuotaDate, decimal SalesQuota, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateSalesPersonQuotaHistoryRequest
{
    public int BusinessEntityId { get; set; }
    public DateTime QuotaDate { get; set; }
    public decimal SalesQuota { get; set; }
}

public sealed record UpdateSalesPersonQuotaHistoryRequest
{
    public decimal? SalesQuota { get; set; }
}

public sealed record SalesPersonQuotaHistoryAuditLogDto(
    int Id, int BusinessEntityId, DateTime QuotaDate, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, decimal SalesQuota, Guid RowGuid, DateTime SourceModifiedDate);

public static class SalesPersonQuotaHistoryMappings
{
    public static SalesPersonQuotaHistoryDto ToDto(this SalesPersonQuotaHistory e) => new(
        e.BusinessEntityId, e.QuotaDate, e.SalesQuota, e.RowGuid, e.ModifiedDate);

    public static SalesPersonQuotaHistory ToEntity(this CreateSalesPersonQuotaHistoryRequest r) => new()
    {
        BusinessEntityId = r.BusinessEntityId,
        QuotaDate = r.QuotaDate,
        SalesQuota = r.SalesQuota,
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateSalesPersonQuotaHistoryRequest r, SalesPersonQuotaHistory e)
    {
        if (r.SalesQuota.HasValue) e.SalesQuota = r.SalesQuota.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static SalesPersonQuotaHistoryAuditLogDto ToDto(this SalesPersonQuotaHistoryAuditLog a) => new(
        a.Id, a.BusinessEntityId, a.QuotaDate, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.SalesQuota, a.RowGuid, a.SourceModifiedDate);
}
