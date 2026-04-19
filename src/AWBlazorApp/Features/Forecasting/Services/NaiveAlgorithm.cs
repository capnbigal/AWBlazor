using AWBlazorApp.Features.Forecasting.Domain;

namespace AWBlazorApp.Features.Forecasting.Services;

/// <summary>
/// Naive forecast: every future period equals the most recent observed value.
/// Fast baseline; useful when the series has no clear trend or seasonality.
/// </summary>
public sealed class NaiveAlgorithm : IForecastAlgorithm
{
    public ForecastMethod Method => ForecastMethod.Naive;

    public List<TimeSeriesPoint> Compute(
        List<TimeSeriesPoint> historicalData,
        int horizonPeriods,
        ForecastGranularity granularity,
        IDictionary<string, object>? parameters)
    {
        var lastValue = historicalData[^1].Value;
        var lastDate = historicalData[^1].PeriodDate;
        var result = new List<TimeSeriesPoint>(horizonPeriods);

        for (var i = 0; i < horizonPeriods; i++)
        {
            var nextDate = ForecastPeriod.Advance(lastDate, granularity, i + 1);
            result.Add(new TimeSeriesPoint(nextDate, Math.Round(lastValue, 4)));
        }

        return result;
    }
}
