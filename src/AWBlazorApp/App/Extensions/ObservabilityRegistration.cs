using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AWBlazorApp.App.Extensions;

/// <summary>
/// OpenTelemetry traces + metrics, exported via OTLP to the central Aspire Dashboard.
/// Logs are exported separately through the Serilog OTLP sink (see Program.cs), since the
/// app uses Serilog as its logging provider. Export is opt-in: it only activates when
/// OTEL_EXPORTER_OTLP_ENDPOINT is set, so local runs without a collector stay quiet.
/// </summary>
public static class ObservabilityRegistration
{
    public static IServiceCollection AddApplicationObservability(this IServiceCollection services, IConfiguration config)
    {
        var otel = services.AddOpenTelemetry();

        otel.ConfigureResource(r => r.AddService(
            serviceName: config["OTEL_SERVICE_NAME"] ?? "awblazor"));

        otel.WithTracing(t => t
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation());

        otel.WithMetrics(m => m
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation());

        if (!string.IsNullOrWhiteSpace(config["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            otel.UseOtlpExporter();
        }

        return services;
    }
}
