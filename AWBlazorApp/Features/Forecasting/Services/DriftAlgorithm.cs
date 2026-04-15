using AWBlazorApp.Features.Forecasting.Domain;

namespace AWBlazorApp.Features.Forecasting.Services;

/// <summary>
/// Drift method: extrapolates the average rate of change between the first and last
/// historical observations. Forecast(t+h) = Y[n] + h * (Y[n] - Y[1]) / (n - 1).
/// </summary>
public sealed class DriftAlgorithm : IForecastAlgorithm
{
    public ForecastMethod Method => ForecastMethod.Drift;

    public List<TimeSeriesPoint> Compute(
        List<TimeSeriesPoint> historicalData,
        int horizonPeriods,
        ForecastGranularity granularity,
        IDictionary<string, object>? parameters)
    {
        var n = historicalData.Count;
        var first = historicalData[0].Value;
        var last = historicalData[^1].Value;
        var slope = n > 1 ? (last - first) / (n - 1) : 0m;

        var lastDate = historicalData[^1].PeriodDate;
        var result = new List<TimeSeriesPoint>(horizonPeriods);

        for (var i = 0; i < horizonPeriods; i++)
        {
            var predicted = last + (i + 1) * slope;
            var nextDate = ForecastPeriod.Advance(lastDate, granularity, i + 1);
            result.Add(new TimeSeriesPoint(nextDate, Math.Round(Math.Max(0, predicted), 4)));
        }

        return result;
    }
}
