using ElementaryApp.Data.Entities.Forecasting;

namespace ElementaryApp.Services.Forecasting;

public sealed class ExponentialSmoothingAlgorithm : IForecastAlgorithm
{
    public ForecastMethod Method => ForecastMethod.ExponentialSmoothing;

    public List<TimeSeriesPoint> Compute(
        List<TimeSeriesPoint> historicalData,
        int horizonPeriods,
        ForecastGranularity granularity,
        IDictionary<string, object>? parameters)
    {
        var alpha = parameters?.TryGetValue("alpha", out var a) == true
            ? Convert.ToDecimal(a)
            : 0.3m;

        alpha = Math.Clamp(alpha, 0.01m, 0.99m);

        // Compute smoothed level through the historical data
        var level = historicalData[0].Value;
        foreach (var point in historicalData.Skip(1))
            level = alpha * point.Value + (1 - alpha) * level;

        var lastDate = historicalData[^1].PeriodDate;
        var result = new List<TimeSeriesPoint>();

        // For simple exponential smoothing, the forecast is flat (last level)
        for (var i = 0; i < horizonPeriods; i++)
        {
            var nextDate = granularity == ForecastGranularity.Quarterly
                ? lastDate.AddMonths((i + 1) * 3)
                : lastDate.AddMonths(i + 1);
            result.Add(new TimeSeriesPoint(nextDate, Math.Round(level, 4)));
        }

        return result;
    }
}
