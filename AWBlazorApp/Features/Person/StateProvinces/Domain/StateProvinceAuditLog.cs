using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Person.StateProvinces.Domain;

/// <summary>Audit log for <see cref="StateProvince"/>. EF-managed table <c>dbo.StateProvinceAuditLogs</c>.</summary>
public class StateProvinceAuditLog : AdventureWorksAuditLogBase
{
    public int StateProvinceId { get; set; }

    [MaxLength(3)]  public string? StateProvinceCode { get; set; }
    [MaxLength(3)]  public string? CountryRegionCode { get; set; }
    public bool IsOnlyStateProvinceFlag { get; set; }
    [MaxLength(50)] public string? Name { get; set; }
    public int TerritoryId { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
