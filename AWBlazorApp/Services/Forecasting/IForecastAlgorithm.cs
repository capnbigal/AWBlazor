using AWBlazorApp.Data.Entities.Forecasting;

namespace AWBlazorApp.Services.Forecasting;

public interface IForecastAlgorithm
{
    ForecastMethod Method { get; }

    List<TimeSeriesPoint> Compute(
        List<TimeSeriesPoint> historicalData,
        int horizonPeriods,
        ForecastGranularity granularity,
        IDictionary<string, object>? parameters);
}
