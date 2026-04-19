using AWBlazorApp.Features.Forecasting.Domain;

namespace AWBlazorApp.Features.Forecasting.Services;

/// <summary>
/// Logarithmic regression: fits y = a + b · ln(x + 1). Useful for series that grow
/// quickly at first and then plateau (saturation curves).
/// </summary>
public sealed class LogarithmicRegressionAlgorithm : IForecastAlgorithm
{
    public ForecastMethod Method => ForecastMethod.LogarithmicRegression;

    public List<TimeSeriesPoint> Compute(
        List<TimeSeriesPoint> historicalData,
        int horizonPeriods,
        ForecastGranularity granularity,
        IDictionary<string, object>? parameters)
    {
        var n = historicalData.Count;
        double sumLnX = 0, sumY = 0, sumLnXY = 0, sumLnX2 = 0;

        for (var i = 0; i < n; i++)
        {
            var lnx = Math.Log(i + 1);
            double y = (double)historicalData[i].Value;
            sumLnX += lnx;
            sumY += y;
            sumLnXY += lnx * y;
            sumLnX2 += lnx * lnx;
        }

        var denom = n * sumLnX2 - sumLnX * sumLnX;
        var b = denom != 0 ? (n * sumLnXY - sumLnX * sumY) / denom : 0;
        var a = (sumY - b * sumLnX) / n;

        var lastDate = historicalData[^1].PeriodDate;
        var result = new List<TimeSeriesPoint>(horizonPeriods);

        for (var i = 0; i < horizonPeriods; i++)
        {
            var x = n + i;
            var predicted = a + b * Math.Log(x + 1);
            var nextDate = ForecastPeriod.Advance(lastDate, granularity, i + 1);
            result.Add(new TimeSeriesPoint(nextDate, Math.Round((decimal)Math.Max(0, predicted), 4)));
        }

        return result;
    }
}
