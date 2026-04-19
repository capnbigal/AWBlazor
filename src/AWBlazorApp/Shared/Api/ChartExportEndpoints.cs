using System.Text;
using AWBlazorApp.Shared.Services;
using Microsoft.Extensions.Caching.Memory;

namespace AWBlazorApp.Shared.Api;

public static class ChartExportEndpoints
{
    /// <summary>
    /// Registers a chart data export endpoint. Components call <see cref="AnalyticsCacheService"/>
    /// to store chart export data under a known key, then link to this endpoint to download it.
    /// No JS required — the download button is a plain <c>&lt;a href&gt;</c> link.
    /// </summary>
    public static void MapChartExportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/chart-export/{key}", (string key, IMemoryCache cache) =>
        {
            if (!cache.TryGetValue($"chart-export:{key}", out ChartExportData? data) || data is null)
                return Results.NotFound("Chart data not found or expired. Reload the dashboard and try again.");

            var sb = new StringBuilder();
            sb.Append("Period");
            foreach (var s in data.Series)
                sb.Append($",{s.Name}");
            sb.AppendLine();

            for (var i = 0; i < data.Labels.Length; i++)
            {
                sb.Append(data.Labels[i]);
                foreach (var s in data.Series)
                    sb.Append($",{(i < s.Data.Length ? s.Data[i] : 0)}");
                sb.AppendLine();
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return Results.File(bytes, "text/csv", $"{data.Title}.csv");
        })
        .RequireAuthorization();
    }

    /// <summary>
    /// Stores chart data for export and returns a cache key that can be used as a download URL.
    /// Called by the TimeSeriesChart component when rendering.
    /// </summary>
    public static string StoreForExport(IMemoryCache cache, string title, string period, string[] labels, List<ChartExportSeries> series)
    {
        var raw = $"{title.Replace(' ', '_')}_{period}_{Guid.NewGuid():N}";
        var key = raw[..Math.Min(raw.Length, 64)];
        var data = new ChartExportData(title, labels, series);
        cache.Set($"chart-export:{key}", data, TimeSpan.FromMinutes(10));
        return key;
    }

    public sealed record ChartExportData(string Title, string[] Labels, List<ChartExportSeries> Series);
    public sealed record ChartExportSeries(string Name, double[] Data);
}
