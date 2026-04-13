using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Data.Entities.Forecasting;

public class ForecastHistoricalSnapshot
{
    public int Id { get; set; }

    public int ForecastDefinitionId { get; set; }

    [ForeignKey(nameof(ForecastDefinitionId))]
    public ForecastDefinition ForecastDefinition { get; set; } = null!;

    /// <summary>First day of the historical period that was used as input.</summary>
    public DateTime PeriodDate { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal Value { get; set; }
}
