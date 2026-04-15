using AWBlazorApp.Features.Forecasting.Domain;

namespace AWBlazorApp.Features.Forecasting.Services;

public sealed class WeightedMovingAverageAlgorithm : IForecastAlgorithm
{
    public ForecastMethod Method => ForecastMethod.WeightedMovingAverage;

    public List<TimeSeriesPoint> Compute(
        List<TimeSeriesPoint> historicalData,
        int horizonPeriods,
        ForecastGranularity granularity,
        IDictionary<string, object>? parameters)
    {
        var windowSize = Math.Min(
            parameters?.TryGetValue("windowSize", out var ws) == true ? Convert.ToInt32(ws) : 3,
            historicalData.Count);

        var window = historicalData.TakeLast(windowSize).Select(p => p.Value).ToList();
        var lastDate = historicalData[^1].PeriodDate;
        var result = new List<TimeSeriesPoint>();

        // Weights: linearly increasing (1, 2, 3, ..., windowSize)
        var totalWeight = (decimal)(windowSize * (windowSize + 1)) / 2;

        for (var i = 0; i < horizonPeriods; i++)
        {
            var weightedSum = 0m;
            for (var w = 0; w < window.Count; w++)
                weightedSum += window[w] * (w + 1);

            var wma = weightedSum / totalWeight;
            var nextDate = granularity == ForecastGranularity.Quarterly
                ? lastDate.AddMonths((i + 1) * 3)
                : lastDate.AddMonths(i + 1);
            result.Add(new TimeSeriesPoint(nextDate, Math.Round(wma, 4)));
            window.RemoveAt(0);
            window.Add(wma);
        }

        return result;
    }
}
