using AWBlazorApp.Features.Forecasting.Domain;

namespace AWBlazorApp.Features.Forecasting.Services;

/// <summary>
/// Seasonal naive: forecast for period t+h equals the value observed one season ago
/// (Y[t+h] = Y[t+h-m]). Default season length is 12 for monthly data, 4 for quarterly.
/// </summary>
public sealed class SeasonalNaiveAlgorithm : IForecastAlgorithm
{
    public ForecastMethod Method => ForecastMethod.SeasonalNaive;

    public List<TimeSeriesPoint> Compute(
        List<TimeSeriesPoint> historicalData,
        int horizonPeriods,
        ForecastGranularity granularity,
        IDictionary<string, object>? parameters)
    {
        var requested = parameters?.TryGetValue("seasonLength", out var sl) == true
            ? Convert.ToInt32(sl)
            : ForecastPeriod.DefaultSeasonLength(granularity);
        var seasonLength = Math.Max(1, Math.Min(requested, historicalData.Count));

        var lastDate = historicalData[^1].PeriodDate;
        var result = new List<TimeSeriesPoint>(horizonPeriods);

        for (var i = 0; i < horizonPeriods; i++)
        {
            var sourceIndex = historicalData.Count - seasonLength + (i % seasonLength);
            var value = historicalData[Math.Max(0, sourceIndex)].Value;
            var nextDate = ForecastPeriod.Advance(lastDate, granularity, i + 1);
            result.Add(new TimeSeriesPoint(nextDate, Math.Round(value, 4)));
        }

        return result;
    }
}
