using AWBlazorApp.Features.Forecasting.Domain;

namespace AWBlazorApp.Features.Forecasting.Services;

/// <summary>
/// Quadratic regression: fits y = a + b·x + c·x² via the closed-form normal equations
/// (Cramer's rule on a 3×3 system). Captures gentle curvature that LinearRegression misses.
/// </summary>
public sealed class QuadraticRegressionAlgorithm : IForecastAlgorithm
{
    public ForecastMethod Method => ForecastMethod.QuadraticRegression;

    public List<TimeSeriesPoint> Compute(
        List<TimeSeriesPoint> historicalData,
        int horizonPeriods,
        ForecastGranularity granularity,
        IDictionary<string, object>? parameters)
    {
        var n = historicalData.Count;
        double sumX = 0, sumX2 = 0, sumX3 = 0, sumX4 = 0;
        double sumY = 0, sumXY = 0, sumX2Y = 0;

        for (var i = 0; i < n; i++)
        {
            double x = i;
            double y = (double)historicalData[i].Value;
            var x2 = x * x;
            sumX += x; sumX2 += x2; sumX3 += x2 * x; sumX4 += x2 * x2;
            sumY += y; sumXY += x * y; sumX2Y += x2 * y;
        }

        // Normal equations:
        //   [ n      ΣX     ΣX² ] [a]   [ΣY  ]
        //   [ ΣX     ΣX²    ΣX³ ] [b] = [ΣXY ]
        //   [ ΣX²    ΣX³    ΣX⁴ ] [c]   [ΣX²Y]
        var det = Det3(n, sumX, sumX2, sumX, sumX2, sumX3, sumX2, sumX3, sumX4);
        double a, b, c;
        if (Math.Abs(det) < 1e-12)
        {
            // Degenerate (e.g. n < 3) — fall back to linear regression on the same data.
            return new LinearRegressionAlgorithm().Compute(historicalData, horizonPeriods, granularity, parameters);
        }

        a = Det3(sumY, sumX, sumX2, sumXY, sumX2, sumX3, sumX2Y, sumX3, sumX4) / det;
        b = Det3(n, sumY, sumX2, sumX, sumXY, sumX3, sumX2, sumX2Y, sumX4) / det;
        c = Det3(n, sumX, sumY, sumX, sumX2, sumXY, sumX2, sumX3, sumX2Y) / det;

        var lastDate = historicalData[^1].PeriodDate;
        var result = new List<TimeSeriesPoint>(horizonPeriods);

        for (var i = 0; i < horizonPeriods; i++)
        {
            double x = n + i;
            var predicted = a + b * x + c * x * x;
            var nextDate = ForecastPeriod.Advance(lastDate, granularity, i + 1);
            result.Add(new TimeSeriesPoint(nextDate, Math.Round((decimal)Math.Max(0, predicted), 4)));
        }

        return result;
    }

    private static double Det3(
        double a, double b, double c,
        double d, double e, double f,
        double g, double h, double i)
        => a * (e * i - f * h) - b * (d * i - f * g) + c * (d * h - e * g);
}
