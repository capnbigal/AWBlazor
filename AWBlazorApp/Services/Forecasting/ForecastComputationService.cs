using System.Text.Json;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Data.Entities.Forecasting;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Services.Forecasting;

public sealed class ForecastComputationService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    IForecastDataSourceProvider dataSourceProvider,
    IEnumerable<IForecastAlgorithm> algorithms) : IForecastComputationService
{
    public async Task<List<ForecastDataPoint>> ComputeAndSaveAsync(int forecastDefinitionId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var definition = await db.ForecastDefinitions.FirstOrDefaultAsync(d => d.Id == forecastDefinitionId, ct)
            ?? throw new InvalidOperationException($"Forecast definition {forecastDefinitionId} not found.");

        var historical = await dataSourceProvider.GetHistoricalDataAsync(
            definition.DataSource, definition.Granularity, definition.LookbackMonths, ct);

        if (historical.Count < 2)
            throw new InvalidOperationException("Not enough historical data to generate a forecast. Need at least 2 data points.");

        var algorithm = algorithms.FirstOrDefault(a => a.Method == definition.Method)
            ?? throw new InvalidOperationException($"No algorithm found for method {definition.Method}.");

        var parameters = ParseParameters(definition.MethodParametersJson);
        var projected = algorithm.Compute(historical, definition.HorizonPeriods, definition.Granularity, parameters);

        // Clear previous computation
        var oldPoints = await db.ForecastDataPoints.Where(p => p.ForecastDefinitionId == forecastDefinitionId).ToListAsync(ct);
        var oldSnapshots = await db.ForecastHistoricalSnapshots.Where(s => s.ForecastDefinitionId == forecastDefinitionId).ToListAsync(ct);
        db.ForecastDataPoints.RemoveRange(oldPoints);
        db.ForecastHistoricalSnapshots.RemoveRange(oldSnapshots);

        // Save historical snapshot
        var snapshots = historical.Select(h => new ForecastHistoricalSnapshot
        {
            ForecastDefinitionId = forecastDefinitionId,
            PeriodDate = h.PeriodDate,
            Value = h.Value,
        }).ToList();
        db.ForecastHistoricalSnapshots.AddRange(snapshots);

        // Save projected data points
        var dataPoints = projected.Select(p => new ForecastDataPoint
        {
            ForecastDefinitionId = forecastDefinitionId,
            PeriodDate = p.PeriodDate,
            ForecastedValue = p.Value,
        }).ToList();
        db.ForecastDataPoints.AddRange(dataPoints);

        definition.LastComputedDate = DateTime.UtcNow;
        definition.Status = ForecastStatus.Active;

        await db.SaveChangesAsync(ct);
        return dataPoints;
    }

    public async Task<ForecastPreview> PreviewAsync(
        ForecastDataSource dataSource,
        ForecastMethod method,
        ForecastGranularity granularity,
        int lookbackMonths,
        int horizonPeriods,
        string? methodParametersJson,
        CancellationToken ct = default)
    {
        var historical = await dataSourceProvider.GetHistoricalDataAsync(dataSource, granularity, lookbackMonths, ct);

        if (historical.Count < 2)
            return new ForecastPreview(historical, []);

        var algorithm = algorithms.FirstOrDefault(a => a.Method == method)
            ?? throw new InvalidOperationException($"No algorithm found for method {method}.");

        var parameters = ParseParameters(methodParametersJson);
        var projected = algorithm.Compute(historical, horizonPeriods, granularity, parameters);

        return new ForecastPreview(historical, projected);
    }

    private static Dictionary<string, object>? ParseParameters(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            return dict?.ToDictionary(
                kvp => kvp.Key,
                kvp => (object)(kvp.Value.ValueKind switch
                {
                    JsonValueKind.Number => kvp.Value.GetDecimal(),
                    JsonValueKind.String => kvp.Value.GetString()!,
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    _ => kvp.Value.ToString(),
                }));
        }
        catch { return null; }
    }
}
