using AWBlazorApp.Features.Forecasting.Domain;

namespace AWBlazorApp.Features.Forecasting.Services;

/// <summary>
/// Median of the trailing window. Robust to outliers in a way the mean-based
/// SimpleMovingAverage isn't — useful for series with sporadic spikes.
/// </summary>
public sealed class MedianSmoothingAlgorithm : IForecastAlgorithm
{
    public ForecastMethod Method => ForecastMethod.MedianSmoothing;

    public List<TimeSeriesPoint> Compute(
        List<TimeSeriesPoint> historicalData,
        int horizonPeriods,
        ForecastGranularity granularity,
        IDictionary<string, object>? parameters)
    {
        var requested = parameters?.TryGetValue("windowSize", out var ws) == true
            ? Convert.ToInt32(ws)
            : 3;
        var windowSize = Math.Max(1, Math.Min(requested, historicalData.Count));

        var window = historicalData.TakeLast(windowSize).Select(p => p.Value).ToList();
        var lastDate = historicalData[^1].PeriodDate;
        var result = new List<TimeSeriesPoint>(horizonPeriods);

        for (var i = 0; i < horizonPeriods; i++)
        {
            var median = Median(window);
            var nextDate = ForecastPeriod.Advance(lastDate, granularity, i + 1);
            result.Add(new TimeSeriesPoint(nextDate, Math.Round(median, 4)));
            window.RemoveAt(0);
            window.Add(median);
        }

        return result;
    }

    private static decimal Median(List<decimal> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        var mid = sorted.Count / 2;
        return sorted.Count % 2 == 0
            ? (sorted[mid - 1] + sorted[mid]) / 2m
            : sorted[mid];
    }
}
