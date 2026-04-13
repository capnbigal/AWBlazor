namespace ElementaryApp.Data.Entities.Forecasting;

public enum ForecastMethod
{
    SimpleMovingAverage = 0,
    WeightedMovingAverage = 1,
    ExponentialSmoothing = 2,
    LinearRegression = 3,
}
