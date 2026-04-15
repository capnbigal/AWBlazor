using AWBlazorApp.Features.Forecasting.Domain;

namespace AWBlazorApp.Features.Forecasting.Services;

/// <summary>
/// Theta method (θ = 2 variant), the classic M3-competition winner. Combines a long-run
/// linear regression with simple exponential smoothing on the "theta-2" line, then averages
/// the two extrapolations. Empirically robust for monthly business series.
/// </summary>
public sealed class ThetaAlgorithm : IForecastAlgorithm
{
    public ForecastMethod Method => ForecastMethod.Theta;

    public List<TimeSeriesPoint> Compute(
        List<TimeSeriesPoint> historicalData,
        int horizonPeriods,
        ForecastGranularity granularity,
        IDictionary<string, object>? parameters)
    {
        var alpha = Math.Clamp(
            parameters?.TryGetValue("alpha", out var a) == true ? Convert.ToDecimal(a) : 0.3m,
            0.01m, 0.99m);

        var n = historicalData.Count;

        // 1. Linear regression — slope b and intercept i.
        decimal sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
        for (var t = 0; t < n; t++)
        {
            decimal x = t;
            var y = historicalData[t].Value;
            sumX += x; sumY += y; sumXY += x * y; sumX2 += x * x;
        }
        var denom = n * sumX2 - sumX * sumX;
        var slope = denom != 0 ? (n * sumXY - sumX * sumY) / denom : 0m;
        var intercept = (sumY - slope * sumX) / n;

        // 2. Theta-2 line: 2·Y[t] - linearFit[t]. Run SES on it.
        var thetaLine = Enumerable.Range(0, n)
            .Select(t => 2 * historicalData[t].Value - (intercept + slope * t))
            .ToList();
        var sesLevel = thetaLine[0];
        for (var t = 1; t < n; t++)
            sesLevel = alpha * thetaLine[t] + (1 - alpha) * sesLevel;

        var lastDate = historicalData[^1].PeriodDate;
        var result = new List<TimeSeriesPoint>(horizonPeriods);

        for (var i = 0; i < horizonPeriods; i++)
        {
            var x = n + i;
            var linearForecast = intercept + slope * x;
            var predicted = (linearForecast + sesLevel) / 2m;
            var nextDate = ForecastPeriod.Advance(lastDate, granularity, i + 1);
            result.Add(new TimeSeriesPoint(nextDate, Math.Round(Math.Max(0, predicted), 4)));
        }

        return result;
    }
}
