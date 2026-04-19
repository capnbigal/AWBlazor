using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Performance.Scorecards.Domain;

/// <summary>KPI included on a scorecard, with ordering and visual hint.</summary>
[Table("ScorecardKpi", Schema = "perf")]
public class ScorecardKpi
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int ScorecardDefinitionId { get; set; }

    public int KpiDefinitionId { get; set; }

    public int DisplayOrder { get; set; }

    public ScorecardKpiVisual Visual { get; set; } = ScorecardKpiVisual.KpiCard;

    public DateTime ModifiedDate { get; set; }
}

public enum ScorecardKpiVisual : byte
{
    KpiCard = 1,
    LineChart = 2,
    BarChart = 3,
    Gauge = 4,
}
