using AWBlazorApp.Data.Entities.Forecasting;

namespace AWBlazorApp.Services.Forecasting;

public interface IForecastComputationService
{
    Task<List<ForecastDataPoint>> ComputeAndSaveAsync(int forecastDefinitionId, CancellationToken ct = default);

    Task<ForecastPreview> PreviewAsync(
        ForecastDataSource dataSource,
        ForecastMethod method,
        ForecastGranularity granularity,
        int lookbackMonths,
        int horizonPeriods,
        string? methodParametersJson,
        CancellationToken ct = default);
}

public record ForecastPreview(List<TimeSeriesPoint> HistoricalData, List<TimeSeriesPoint> ProjectedData);
