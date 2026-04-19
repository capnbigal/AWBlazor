namespace AWBlazorApp.Features.Forecasting.Domain;

public enum ForecastMethod
{
    SimpleMovingAverage = 0,
    WeightedMovingAverage = 1,
    ExponentialSmoothing = 2,
    LinearRegression = 3,
    Naive = 4,
    SeasonalNaive = 5,
    Drift = 6,
    HistoricalAverage = 7,
    MedianSmoothing = 8,
    DoubleExponentialSmoothing = 9,
    HoltWinters = 10,
    QuadraticRegression = 11,
    LogarithmicRegression = 12,
    ExponentialRegression = 13,
    Croston = 14,
    Theta = 15,
}
