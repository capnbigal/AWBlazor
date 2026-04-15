using System.ComponentModel.DataAnnotations;
using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Forecasting.Domain;

public class ForecastDefinition : AuditableEntity
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public ForecastDataSource DataSource { get; set; }

    public ForecastMethod Method { get; set; }

    public ForecastGranularity Granularity { get; set; }

    public ForecastStatus Status { get; set; }

    /// <summary>How many months of historical data to use as input.</summary>
    public int LookbackMonths { get; set; }

    /// <summary>How many periods forward to project.</summary>
    public int HorizonPeriods { get; set; }

    /// <summary>
    /// Method-specific parameters as JSON. Allows extensibility without schema changes.
    /// Examples: {"windowSize":3} for SMA, {"alpha":0.3} for ExponentialSmoothing.
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? MethodParametersJson { get; set; }

    /// <summary>When the forecast computation was last executed.</summary>
    public DateTime? LastComputedDate { get; set; }

    public ICollection<ForecastDataPoint> DataPoints { get; set; } = new List<ForecastDataPoint>();
    public ICollection<ForecastHistoricalSnapshot> HistoricalSnapshots { get; set; } = new List<ForecastHistoricalSnapshot>();
}
