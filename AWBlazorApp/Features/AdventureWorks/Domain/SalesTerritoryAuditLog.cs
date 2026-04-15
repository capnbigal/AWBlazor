using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.AdventureWorks.Domain;

/// <summary>Audit log for <see cref="SalesTerritory"/>. EF-managed table <c>dbo.SalesTerritoryAuditLogs</c>.</summary>
public class SalesTerritoryAuditLog : AdventureWorksAuditLogBase
{
    public int SalesTerritoryId { get; set; }

    [MaxLength(50)] public string? Name { get; set; }
    [MaxLength(3)]  public string? CountryRegionCode { get; set; }
    [MaxLength(50)] public string? GroupName { get; set; }
    public decimal SalesYtd { get; set; }
    public decimal SalesLastYear { get; set; }
    public decimal CostYtd { get; set; }
    public decimal CostLastYear { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
