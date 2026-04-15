using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Features.Forecasting.Domain;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Forecasting.Services;

public sealed class ForecastEvaluationJob(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    IForecastDataSourceProvider dataSourceProvider,
    ILogger<ForecastEvaluationJob> logger)
{
    public async Task ExecuteAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var today = DateTime.UtcNow.Date;

        var pendingPoints = await db.ForecastDataPoints
            .Include(dp => dp.ForecastDefinition)
            .Where(dp => dp.ActualValue == null
                      && dp.ForecastDefinition.Status == ForecastStatus.Active
                      && dp.ForecastDefinition.DeletedDate == null)
            .Where(dp =>
                (dp.ForecastDefinition.Granularity == ForecastGranularity.Monthly
                    && dp.PeriodDate.AddMonths(1) <= today)
                || (dp.ForecastDefinition.Granularity == ForecastGranularity.Quarterly
                    && dp.PeriodDate.AddMonths(3) <= today))
            .ToListAsync();

        if (pendingPoints.Count == 0)
        {
            logger.LogInformation("ForecastEvaluationJob: no pending data points to evaluate.");
            return;
        }

        var evaluated = 0;
        foreach (var point in pendingPoints)
        {
            try
            {
                var actual = await dataSourceProvider.GetActualValueAsync(
                    point.ForecastDefinition.DataSource,
                    point.ForecastDefinition.Granularity,
                    point.PeriodDate);

                if (actual is null) continue;

                point.ActualValue = actual.Value;
                point.Variance = actual.Value - point.ForecastedValue;
                point.VariancePercent = point.ForecastedValue != 0
                    ? Math.Round((actual.Value - point.ForecastedValue) / Math.Abs(point.ForecastedValue) * 100, 2)
                    : null;
                point.EvaluatedDate = DateTime.UtcNow;
                evaluated++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to evaluate forecast point {PointId} for period {Period}",
                    point.Id, point.PeriodDate);
            }
        }

        await db.SaveChangesAsync();
        logger.LogInformation("ForecastEvaluationJob: evaluated {Count} of {Total} pending data points.",
            evaluated, pendingPoints.Count);
    }
}
