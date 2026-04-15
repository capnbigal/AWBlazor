using AWBlazorApp.Features.Forecasting.Domain;

namespace AWBlazorApp.Features.Forecasting.Services;

/// <summary>
/// Exponential regression: fits y = a · e^(b·x) by linearising into ln(y) = ln(a) + b·x and
/// running ordinary least squares. Requires strictly positive Y values; falls back to linear
/// regression on series that contain zero or negative observations.
/// </summary>
public sealed class ExponentialRegressionAlgorithm : IForecastAlgorithm
{
    public ForecastMethod Method => ForecastMethod.ExponentialRegression;

    public List<TimeSeriesPoint> Compute(
        List<TimeSeriesPoint> historicalData,
        int horizonPeriods,
        ForecastGranularity granularity,
        IDictionary<string, object>? parameters)
    {
        if (historicalData.Any(p => p.Value <= 0))
            return new LinearRegressionAlgorithm().Compute(historicalData, horizonPeriods, granularity, parameters);

        var n = historicalData.Count;
        double sumX = 0, sumLnY = 0, sumXLnY = 0, sumX2 = 0;

        for (var i = 0; i < n; i++)
        {
            double x = i;
            var lny = Math.Log((double)historicalData[i].Value);
            sumX += x;
            sumLnY += lny;
            sumXLnY += x * lny;
            sumX2 += x * x;
        }

        var denom = n * sumX2 - sumX * sumX;
        var b = denom != 0 ? (n * sumXLnY - sumX * sumLnY) / denom : 0;
        var lnA = (sumLnY - b * sumX) / n;
        var a = Math.Exp(lnA);

        var lastDate = historicalData[^1].PeriodDate;
        var result = new List<TimeSeriesPoint>(horizonPeriods);

        for (var i = 0; i < horizonPeriods; i++)
        {
            var x = n + i;
            var predicted = a * Math.Exp(b * x);
            var nextDate = ForecastPeriod.Advance(lastDate, granularity, i + 1);
            result.Add(new TimeSeriesPoint(nextDate, Math.Round((decimal)Math.Max(0, predicted), 4)));
        }

        return result;
    }
}
