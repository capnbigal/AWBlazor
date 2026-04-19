using AWBlazorApp.Features.AdventureWorks.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Enterprise.Domain;

public class StationAuditLog : AdventureWorksAuditLogBase
{
    public int StationId { get; set; }

    public int OrgUnitId { get; set; }
    [MaxLength(32)]  public string? Code { get; set; }
    [MaxLength(200)] public string? Name { get; set; }
    public StationKind StationKind { get; set; }
    public int? OperatorBusinessEntityId { get; set; }
    public int? AssetId { get; set; }
    public decimal? IdealCycleSeconds { get; set; }
    public bool IsActive { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
