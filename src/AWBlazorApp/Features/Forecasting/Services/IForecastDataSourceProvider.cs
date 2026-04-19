using AWBlazorApp.Features.Forecasting.Domain;

namespace AWBlazorApp.Features.Forecasting.Services;

public interface IForecastDataSourceProvider
{
    Task<List<TimeSeriesPoint>> GetHistoricalDataAsync(
        ForecastDataSource dataSource,
        ForecastGranularity granularity,
        int lookbackMonths,
        CancellationToken ct = default);

    Task<decimal?> GetActualValueAsync(
        ForecastDataSource dataSource,
        ForecastGranularity granularity,
        DateTime periodDate,
        CancellationToken ct = default);
}
