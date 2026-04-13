using ElementaryApp.Data.Entities.Forecasting;

namespace ElementaryApp.Services.Forecasting;

public interface IForecastAlgorithm
{
    ForecastMethod Method { get; }

    List<TimeSeriesPoint> Compute(
        List<TimeSeriesPoint> historicalData,
        int horizonPeriods,
        ForecastGranularity granularity,
        IDictionary<string, object>? parameters);
}
