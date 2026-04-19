using AWBlazorApp.Features.Forecasting.Domain;

namespace AWBlazorApp.Features.Forecasting.Services;

/// <summary>
/// Holt-Winters additive triple exponential smoothing — captures level, trend, and
/// additive seasonality. Falls back to <see cref="DoubleExponentialSmoothingAlgorithm"/>
/// if there isn't enough data to estimate a full season.
/// </summary>
public sealed class HoltWintersAlgorithm : IForecastAlgorithm
{
    public ForecastMethod Method => ForecastMethod.HoltWinters;

    public List<TimeSeriesPoint> Compute(
        List<TimeSeriesPoint> historicalData,
        int horizonPeriods,
        ForecastGranularity granularity,
        IDictionary<string, object>? parameters)
    {
        var alpha = Clamp(parameters, "alpha", 0.3m);
        var beta = Clamp(parameters, "beta", 0.1m);
        var gamma = Clamp(parameters, "gamma", 0.1m);
        var requestedSeason = parameters?.TryGetValue("seasonLength", out var sl) == true
            ? Convert.ToInt32(sl)
            : ForecastPeriod.DefaultSeasonLength(granularity);
        var seasonLength = Math.Max(2, requestedSeason);

        if (historicalData.Count < seasonLength * 2)
        {
            // Not enough data for two full cycles — degrade to Holt's method.
            return new DoubleExponentialSmoothingAlgorithm()
                .Compute(historicalData, horizonPeriods, granularity, parameters);
        }

        // Initial level: mean of the first season.
        // Initial trend: avg per-step change between season 1 and season 2.
        // Initial seasonals: deviations of season 1 from initial level.
        var level = historicalData.Take(seasonLength).Average(p => p.Value);
        var trend = Enumerable.Range(0, seasonLength)
            .Select(i => (historicalData[i + seasonLength].Value - historicalData[i].Value) / seasonLength)
            .Average();
        var seasonals = Enumerable.Range(0, seasonLength)
            .Select(i => historicalData[i].Value - level)
            .ToArray();

        for (var t = 0; t < historicalData.Count; t++)
        {
            var value = historicalData[t].Value;
            var seasonalIdx = t % seasonLength;
            var prevLevel = level;
            level = alpha * (value - seasonals[seasonalIdx]) + (1 - alpha) * (level + trend);
            trend = beta * (level - prevLevel) + (1 - beta) * trend;
            seasonals[seasonalIdx] = gamma * (value - level) + (1 - gamma) * seasonals[seasonalIdx];
        }

        var lastDate = historicalData[^1].PeriodDate;
        var result = new List<TimeSeriesPoint>(horizonPeriods);

        for (var i = 0; i < horizonPeriods; i++)
        {
            var seasonalIdx = (historicalData.Count + i) % seasonLength;
            var predicted = level + (i + 1) * trend + seasonals[seasonalIdx];
            var nextDate = ForecastPeriod.Advance(lastDate, granularity, i + 1);
            result.Add(new TimeSeriesPoint(nextDate, Math.Round(Math.Max(0, predicted), 4)));
        }

        return result;
    }

    private static decimal Clamp(IDictionary<string, object>? parameters, string key, decimal defaultValue)
    {
        var raw = parameters?.TryGetValue(key, out var v) == true ? Convert.ToDecimal(v) : defaultValue;
        return Math.Clamp(raw, 0.01m, 0.99m);
    }
}
