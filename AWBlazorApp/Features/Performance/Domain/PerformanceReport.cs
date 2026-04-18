using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Performance.Domain;

/// <summary>
/// A saved performance-report query. Definition is a JSON blob that the report runner
/// interprets — kept generic so new report shapes can ship without schema changes.
/// </summary>
[Table("PerformanceReport", Schema = "perf")]
public class PerformanceReport
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(32)] public string Code { get; set; } = string.Empty;
    [MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(2000)] public string? Description { get; set; }

    public PerformanceReportKind Kind { get; set; } = PerformanceReportKind.OeeSummary;

    /// <summary>JSON blob — filters, date ranges, station lists, etc.</summary>
    public string DefinitionJson { get; set; } = "{}";

    public DateTime? LastRunAt { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime ModifiedDate { get; set; }
}

public enum PerformanceReportKind : byte
{
    OeeSummary = 1,
    ProductionTrend = 2,
    MaintenanceScorecard = 3,
    KpiRollup = 4,
    Custom = 99,
}
