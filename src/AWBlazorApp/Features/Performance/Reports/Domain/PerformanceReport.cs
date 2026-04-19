using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Performance.Reports.Domain;

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

    /// <summary>
    /// Date range the runner will use. Maps to a concrete (start, end) pair at run time —
    /// keeping the preset rather than storing absolute dates so a saved report keeps
    /// "rolling" instead of becoming stale.
    /// </summary>
    public ReportRangePreset RangePreset { get; set; } = ReportRangePreset.Last7Days;

    /// <summary>Optional station filter — applies to OeeSummary / ProductionTrend kinds.</summary>
    public int? StationId { get; set; }

    /// <summary>Optional asset filter — applies to MaintenanceScorecard kind.</summary>
    public int? AssetId { get; set; }

    /// <summary>
    /// Free-form extension blob preserved for the Custom kind and forward-compat. The runner
    /// does NOT read this for the four typed kinds — those are driven by the structured
    /// columns above. Kept as a column rather than removed so existing rows / audit logs
    /// stay intact and the Custom kind has somewhere to store its own params.
    /// </summary>
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

/// <summary>
/// Rolling date-range presets understood by the report runner. Resolves to a concrete
/// (startUtc, endUtc) pair each time the report runs — never stored as absolute dates so
/// that "last 30 days" stays meaningful regardless of when the report was created.
/// </summary>
public enum ReportRangePreset : byte
{
    Last7Days = 1,
    Last30Days = 2,
    Last90Days = 3,
    ThisWeek = 4,
    ThisMonth = 5,
    LastMonth = 6,
    YearToDate = 7,
}
