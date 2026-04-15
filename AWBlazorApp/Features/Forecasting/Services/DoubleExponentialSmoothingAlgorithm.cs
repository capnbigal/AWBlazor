using AWBlazorApp.Features.Forecasting.Domain;

namespace AWBlazorApp.Features.Forecasting.Services;

/// <summary>
/// Holt's linear method (double exponential smoothing). Captures trend in addition to level:
///   L[t] = α * Y[t] + (1-α) * (L[t-1] + T[t-1])
///   T[t] = β * (L[t] - L[t-1]) + (1-β) * T[t-1]
///   Forecast(t+h) = L[t] + h * T[t]
/// </summary>
public sealed class DoubleExponentialSmoothingAlgorithm : IForecastAlgorithm
{
    public ForecastMethod Method => ForecastMethod.DoubleExponentialSmoothing;

    public List<TimeSeriesPoint> Compute(
        List<TimeSeriesPoint> historicalData,
        int horizonPeriods,
        ForecastGranularity granularity,
        IDictionary<string, object>? parameters)
    {
        var alpha = Math.Clamp(
            parameters?.TryGetValue("alpha", out var a) == true ? Convert.ToDecimal(a) : 0.3m,
            0.01m, 0.99m);
        var beta = Math.Clamp(
            parameters?.TryGetValue("beta", out var b) == true ? Convert.ToDecimal(b) : 0.1m,
            0.01m, 0.99m);

        var level = historicalData[0].Value;
        var trend = historicalData.Count > 1 ? historicalData[1].Value - historicalData[0].Value : 0m;

        for (var t = 1; t < historicalData.Count; t++)
        {
            var prevLevel = level;
            level = alpha * historicalData[t].Value + (1 - alpha) * (level + trend);
            trend = beta * (level - prevLevel) + (1 - beta) * trend;
        }

        var lastDate = historicalData[^1].PeriodDate;
        var result = new List<TimeSeriesPoint>(horizonPeriods);

        for (var i = 0; i < horizonPeriods; i++)
        {
            var predicted = level + (i + 1) * trend;
            var nextDate = ForecastPeriod.Advance(lastDate, granularity, i + 1);
            result.Add(new TimeSeriesPoint(nextDate, Math.Round(Math.Max(0, predicted), 4)));
        }

        return result;
    }
}
