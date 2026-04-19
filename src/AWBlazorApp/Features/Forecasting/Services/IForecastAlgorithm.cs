using AWBlazorApp.Features.Forecasting.Domain;

namespace AWBlazorApp.Features.Forecasting.Services;

public interface IForecastAlgorithm
{
    ForecastMethod Method { get; }

    List<TimeSeriesPoint> Compute(
        List<TimeSeriesPoint> historicalData,
        int horizonPeriods,
        ForecastGranularity granularity,
        IDictionary<string, object>? parameters);
}
