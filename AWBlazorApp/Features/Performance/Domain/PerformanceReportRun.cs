using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Performance.Domain;

/// <summary>Run-history cache for a <see cref="PerformanceReport"/>.</summary>
[Table("PerformanceReportRun", Schema = "perf")]
public class PerformanceReportRun
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public int PerformanceReportId { get; set; }

    public DateTime RunAt { get; set; }

    [MaxLength(450)] public string? RunByUserId { get; set; }

    public int RowCount { get; set; }

    public int DurationMs { get; set; }

    public string? ResultJson { get; set; }

    [MaxLength(2000)] public string? ErrorMessage { get; set; }

    public DateTime ModifiedDate { get; set; }
}
