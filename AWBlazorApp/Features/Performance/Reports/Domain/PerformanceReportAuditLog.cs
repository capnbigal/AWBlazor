using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Performance.Reports.Domain;

public class PerformanceReportAuditLog : AdventureWorksAuditLogBase
{
    public int PerformanceReportId { get; set; }

    [MaxLength(32)] public string? Code { get; set; }
    [MaxLength(200)] public string? Name { get; set; }
    [MaxLength(2000)] public string? Description { get; set; }
    public PerformanceReportKind Kind { get; set; }
    public ReportRangePreset RangePreset { get; set; }
    public int? StationId { get; set; }
    public int? AssetId { get; set; }
    public string? DefinitionJson { get; set; }
    public DateTime? LastRunAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
