using AWBlazorApp.Features.Forecasting.Domain;

namespace AWBlazorApp.Features.Forecasting.Services;

internal static class ForecastPeriod
{
    public static DateTime Advance(DateTime baseDate, ForecastGranularity granularity, int periods)
        => granularity == ForecastGranularity.Quarterly
            ? baseDate.AddMonths(periods * 3)
            : baseDate.AddMonths(periods);

    public static int DefaultSeasonLength(ForecastGranularity granularity)
        => granularity == ForecastGranularity.Quarterly ? 4 : 12;
}
