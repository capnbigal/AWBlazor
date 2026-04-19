using AWBlazorApp.Features.Forecasting.Domain;

namespace AWBlazorApp.Features.Forecasting.Services;

/// <summary>
/// Mean forecast: every future period equals the arithmetic mean of all observed values.
/// </summary>
public sealed class HistoricalAverageAlgorithm : IForecastAlgorithm
{
    public ForecastMethod Method => ForecastMethod.HistoricalAverage;

    public List<TimeSeriesPoint> Compute(
        List<TimeSeriesPoint> historicalData,
        int horizonPeriods,
        ForecastGranularity granularity,
        IDictionary<string, object>? parameters)
    {
        var avg = historicalData.Average(p => p.Value);
        var lastDate = historicalData[^1].PeriodDate;
        var result = new List<TimeSeriesPoint>(horizonPeriods);

        for (var i = 0; i < horizonPeriods; i++)
        {
            var nextDate = ForecastPeriod.Advance(lastDate, granularity, i + 1);
            result.Add(new TimeSeriesPoint(nextDate, Math.Round(avg, 4)));
        }

        return result;
    }
}
