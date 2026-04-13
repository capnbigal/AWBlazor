using ElementaryApp.Data.Entities.Forecasting;

namespace ElementaryApp.Services.Forecasting;

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
