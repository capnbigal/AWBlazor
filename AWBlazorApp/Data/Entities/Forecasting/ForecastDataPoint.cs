using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Data.Entities.Forecasting;

public class ForecastDataPoint
{
    public int Id { get; set; }

    public int ForecastDefinitionId { get; set; }

    [ForeignKey(nameof(ForecastDefinitionId))]
    public ForecastDefinition ForecastDefinition { get; set; } = null!;

    /// <summary>First day of the forecasted period (e.g. 2026-05-01 for May 2026).</summary>
    public DateTime PeriodDate { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal ForecastedValue { get; set; }

    /// <summary>Null until the period has elapsed and the evaluation job fills it in.</summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal? ActualValue { get; set; }

    /// <summary>ActualValue - ForecastedValue. Null until ActualValue is set.</summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal? Variance { get; set; }

    /// <summary>(ActualValue - ForecastedValue) / ForecastedValue * 100. Null until ActualValue is set.</summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal? VariancePercent { get; set; }

    /// <summary>When ActualValue was populated by the evaluation job.</summary>
    public DateTime? EvaluatedDate { get; set; }
}
