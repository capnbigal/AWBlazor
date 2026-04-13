using AWBlazorApp.Data.Entities.AdventureWorks;

namespace AWBlazorApp.Models.AdventureWorks;

public sealed record SalesPersonDto(
    int Id, int? TerritoryId, decimal? SalesQuota, decimal Bonus, decimal CommissionPct,
    decimal SalesYtd, decimal SalesLastYear, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateSalesPersonRequest
{
    /// <summary>PK / FK to Person.BusinessEntity. NOT identity — caller must supply.</summary>
    public int Id { get; set; }
    public int? TerritoryId { get; set; }
    public decimal? SalesQuota { get; set; }
    public decimal Bonus { get; set; }
    public decimal CommissionPct { get; set; }
}

public sealed record UpdateSalesPersonRequest
{
    public int? TerritoryId { get; set; }
    public decimal? SalesQuota { get; set; }
    public decimal? Bonus { get; set; }
    public decimal? CommissionPct { get; set; }
    public decimal? SalesYtd { get; set; }
    public decimal? SalesLastYear { get; set; }
}

public sealed record SalesPersonAuditLogDto(
    int Id, int SalesPersonId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, int? TerritoryId, decimal? SalesQuota, decimal Bonus, decimal CommissionPct,
    decimal SalesYtd, decimal SalesLastYear, Guid RowGuid, DateTime SourceModifiedDate);

public static class SalesPersonMappings
{
    public static SalesPersonDto ToDto(this SalesPerson e) => new(
        e.Id, e.TerritoryId, e.SalesQuota, e.Bonus, e.CommissionPct,
        e.SalesYtd, e.SalesLastYear, e.RowGuid, e.ModifiedDate);

    public static SalesPerson ToEntity(this CreateSalesPersonRequest r) => new()
    {
        Id = r.Id,
        TerritoryId = r.TerritoryId,
        SalesQuota = r.SalesQuota,
        Bonus = r.Bonus,
        CommissionPct = r.CommissionPct,
        SalesYtd = 0m,
        SalesLastYear = 0m,
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateSalesPersonRequest r, SalesPerson e)
    {
        if (r.TerritoryId.HasValue) e.TerritoryId = r.TerritoryId.Value;
        if (r.SalesQuota.HasValue) e.SalesQuota = r.SalesQuota.Value;
        if (r.Bonus.HasValue) e.Bonus = r.Bonus.Value;
        if (r.CommissionPct.HasValue) e.CommissionPct = r.CommissionPct.Value;
        if (r.SalesYtd.HasValue) e.SalesYtd = r.SalesYtd.Value;
        if (r.SalesLastYear.HasValue) e.SalesLastYear = r.SalesLastYear.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static SalesPersonAuditLogDto ToDto(this SalesPersonAuditLog a) => new(
        a.Id, a.SalesPersonId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.TerritoryId, a.SalesQuota, a.Bonus, a.CommissionPct,
        a.SalesYtd, a.SalesLastYear, a.RowGuid, a.SourceModifiedDate);
}
