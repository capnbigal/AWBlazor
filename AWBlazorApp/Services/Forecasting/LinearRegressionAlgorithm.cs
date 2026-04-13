using AWBlazorApp.Data.Entities.Forecasting;

namespace AWBlazorApp.Services.Forecasting;

public sealed class LinearRegressionAlgorithm : IForecastAlgorithm
{
    public ForecastMethod Method => ForecastMethod.LinearRegression;

    public List<TimeSeriesPoint> Compute(
        List<TimeSeriesPoint> historicalData,
        int horizonPeriods,
        ForecastGranularity granularity,
        IDictionary<string, object>? parameters)
    {
        var n = historicalData.Count;

        // x = 0, 1, 2, ..., n-1 (period index)
        // y = values
        var sumX = 0m;
        var sumY = 0m;
        var sumXY = 0m;
        var sumX2 = 0m;

        for (var i = 0; i < n; i++)
        {
            var x = (decimal)i;
            var y = historicalData[i].Value;
            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumX2 += x * x;
        }

        var denom = n * sumX2 - sumX * sumX;
        var slope = denom != 0 ? (n * sumXY - sumX * sumY) / denom : 0;
        var intercept = (sumY - slope * sumX) / n;

        var lastDate = historicalData[^1].PeriodDate;
        var result = new List<TimeSeriesPoint>();

        for (var i = 0; i < horizonPeriods; i++)
        {
            var x = n + i; // extrapolate beyond historical range
            var predicted = intercept + slope * x;
            var nextDate = granularity == ForecastGranularity.Quarterly
                ? lastDate.AddMonths((i + 1) * 3)
                : lastDate.AddMonths(i + 1);
            result.Add(new TimeSeriesPoint(nextDate, Math.Round(Math.Max(0, predicted), 4)));
        }

        return result;
    }
}
