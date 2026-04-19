using AWBlazorApp.Features.Forecasting.Domain;

namespace AWBlazorApp.Features.Forecasting.Services;

/// <summary>
/// Croston's method for intermittent demand. Smooths the demand size and the inter-arrival
/// interval separately, then forecasts as size / interval. Produces a flat forecast.
/// </summary>
public sealed class CrostonAlgorithm : IForecastAlgorithm
{
    public ForecastMethod Method => ForecastMethod.Croston;

    public List<TimeSeriesPoint> Compute(
        List<TimeSeriesPoint> historicalData,
        int horizonPeriods,
        ForecastGranularity granularity,
        IDictionary<string, object>? parameters)
    {
        var alpha = Math.Clamp(
            parameters?.TryGetValue("alpha", out var a) == true ? Convert.ToDecimal(a) : 0.1m,
            0.01m, 0.99m);

        // Initialise on the first non-zero observation.
        var firstNonZero = historicalData.FindIndex(p => p.Value > 0);
        decimal demandLevel;
        decimal intervalLevel;
        if (firstNonZero < 0)
        {
            demandLevel = 0m;
            intervalLevel = 1m;
        }
        else
        {
            demandLevel = historicalData[firstNonZero].Value;
            intervalLevel = firstNonZero + 1;
        }

        var periodsSinceLast = 1;
        for (var t = firstNonZero + 1; t < historicalData.Count; t++)
        {
            if (historicalData[t].Value > 0)
            {
                demandLevel = alpha * historicalData[t].Value + (1 - alpha) * demandLevel;
                intervalLevel = alpha * periodsSinceLast + (1 - alpha) * intervalLevel;
                periodsSinceLast = 1;
            }
            else
            {
                periodsSinceLast++;
            }
        }

        var forecast = intervalLevel > 0 ? demandLevel / intervalLevel : 0m;
        var lastDate = historicalData[^1].PeriodDate;
        var result = new List<TimeSeriesPoint>(horizonPeriods);

        for (var i = 0; i < horizonPeriods; i++)
        {
            var nextDate = ForecastPeriod.Advance(lastDate, granularity, i + 1);
            result.Add(new TimeSeriesPoint(nextDate, Math.Round(Math.Max(0, forecast), 4)));
        }

        return result;
    }
}
