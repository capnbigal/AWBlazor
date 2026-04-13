using ElementaryApp.Data.Entities.Forecasting;

namespace ElementaryApp.Services.Forecasting;

public sealed class SimpleMovingAverageAlgorithm : IForecastAlgorithm
{
    public ForecastMethod Method => ForecastMethod.SimpleMovingAverage;

    public List<TimeSeriesPoint> Compute(
        List<TimeSeriesPoint> historicalData,
        int horizonPeriods,
        ForecastGranularity granularity,
        IDictionary<string, object>? parameters)
    {
        var windowSize = GetWindowSize(parameters, historicalData.Count);
        var window = historicalData.TakeLast(windowSize).Select(p => p.Value).ToList();
        var lastDate = historicalData[^1].PeriodDate;
        var result = new List<TimeSeriesPoint>();

        for (var i = 0; i < horizonPeriods; i++)
        {
            var avg = window.Average();
            var nextDate = AdvancePeriod(lastDate, granularity, i + 1);
            result.Add(new TimeSeriesPoint(nextDate, Math.Round(avg, 4)));
            window.RemoveAt(0);
            window.Add(avg);
        }

        return result;
    }

    private static int GetWindowSize(IDictionary<string, object>? parameters, int dataCount)
    {
        if (parameters?.TryGetValue("windowSize", out var ws) == true)
        {
            var size = Convert.ToInt32(ws);
            return Math.Min(size, dataCount);
        }
        return Math.Min(3, dataCount);
    }

    private static DateTime AdvancePeriod(DateTime baseDate, ForecastGranularity granularity, int periods) =>
        granularity == ForecastGranularity.Quarterly
            ? baseDate.AddMonths(periods * 3)
            : baseDate.AddMonths(periods);
}
