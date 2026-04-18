using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Performance.Domain;

/// <summary>
/// Named dashboard composed of ordered KPIs. The <see cref="ScorecardKpi"/> join rows carry
/// the display order and visual hint (KpiCard / LineChart / BarChart).
/// </summary>
[Table("ScorecardDefinition", Schema = "perf")]
public class ScorecardDefinition
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(32)] public string Code { get; set; } = string.Empty;
    [MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(2000)] public string? Description { get; set; }

    [MaxLength(450)] public string? OwnerUserId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime ModifiedDate { get; set; }
}
