using AWBlazorApp.Features.Forecasting.Services;

namespace AWBlazorApp.Features.Forecasting;

public static class ForecastingServiceRegistration
{
    public static IServiceCollection AddForecastingServices(this IServiceCollection services)
    {
        services.AddScoped<IForecastDataSourceProvider, ForecastDataSourceProvider>();
        services.AddScoped<IForecastComputationService, ForecastComputationService>();

        services.AddScoped<IForecastAlgorithm, SimpleMovingAverageAlgorithm>();
        services.AddScoped<IForecastAlgorithm, WeightedMovingAverageAlgorithm>();
        services.AddScoped<IForecastAlgorithm, ExponentialSmoothingAlgorithm>();
        services.AddScoped<IForecastAlgorithm, LinearRegressionAlgorithm>();
        services.AddScoped<IForecastAlgorithm, NaiveAlgorithm>();
        services.AddScoped<IForecastAlgorithm, SeasonalNaiveAlgorithm>();
        services.AddScoped<IForecastAlgorithm, DriftAlgorithm>();
        services.AddScoped<IForecastAlgorithm, HistoricalAverageAlgorithm>();
        services.AddScoped<IForecastAlgorithm, MedianSmoothingAlgorithm>();
        services.AddScoped<IForecastAlgorithm, DoubleExponentialSmoothingAlgorithm>();
        services.AddScoped<IForecastAlgorithm, HoltWintersAlgorithm>();
        services.AddScoped<IForecastAlgorithm, QuadraticRegressionAlgorithm>();
        services.AddScoped<IForecastAlgorithm, LogarithmicRegressionAlgorithm>();
        services.AddScoped<IForecastAlgorithm, ExponentialRegressionAlgorithm>();
        services.AddScoped<IForecastAlgorithm, CrostonAlgorithm>();
        services.AddScoped<IForecastAlgorithm, ThetaAlgorithm>();

        services.AddTransient<ForecastEvaluationJob>();
        return services;
    }
}
