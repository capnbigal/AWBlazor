using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AWBlazorApp.Infrastructure.Observability;

/// <summary>
/// Optional OpenTelemetry tracing + metrics, gated behind the <c>Features:OpenTelemetry</c> flag.
/// <para>
/// OFF by default: when the flag is false or unset, <see cref="AddApplicationObservability"/>
/// registers nothing and returns immediately, so there is zero runtime overhead and local/test runs
/// are completely unaffected. Logging stays on Serilog; this adds distributed <b>traces</b>
/// (ASP.NET Core requests, outbound <c>HttpClient</c> calls, SQL commands) and <b>metrics</b>
/// (request, HTTP-client, and .NET runtime counters), exported over OTLP to a collector.
/// </para>
/// <para>
/// Turn it on only where an OTLP collector exists: set <c>Features:OpenTelemetry=true</c> and point
/// <c>Observability:OtlpEndpoint</c> (or the standard <c>OTEL_EXPORTER_OTLP_ENDPOINT</c> env var) at
/// the collector. With the flag on but no reachable collector the app still runs — the exporter just
/// logs background export failures.
/// </para>
/// </summary>
public static class ObservabilityRegistration
{
    public static IServiceCollection AddApplicationObservability(this IServiceCollection services, IConfiguration configuration)
    {
        if (!configuration.GetValue("Features:OpenTelemetry", defaultValue: false))
            return services;

        var serviceName = configuration.GetValue("Observability:ServiceName", "AWBlazorApp")!;
        var otlpEndpoint = configuration.GetValue<string?>("Observability:OtlpEndpoint");
        var serviceVersion = typeof(ObservabilityRegistration).Assembly.GetName().Version?.ToString() ?? "0.0.0";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName: serviceName, serviceVersion: serviceVersion))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options => options.RecordException = true)
                .AddHttpClientInstrumentation()
                .AddSqlClientInstrumentation()
                .AddOtlpExporter(options => ApplyEndpoint(options, otlpEndpoint)))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(options => ApplyEndpoint(options, otlpEndpoint)));

        return services;
    }

    // The OTLP exporter also honors the standard OTEL_EXPORTER_OTLP_ENDPOINT env var; the config key
    // is an explicit override. When neither is set the exporter keeps its default (http://localhost:4317).
    private static void ApplyEndpoint(OtlpExporterOptions options, string? endpoint)
    {
        if (!string.IsNullOrWhiteSpace(endpoint))
            options.Endpoint = new Uri(endpoint);
    }
}
