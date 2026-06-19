using AWBlazorApp.Infrastructure.Observability;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace AWBlazorApp.Tests.Infrastructure;

/// <summary>
/// Unit tests for the <c>Features:OpenTelemetry</c> gating in <see cref="ObservabilityRegistration"/>.
/// Pure DI/config — no host, no SQL — so they run in the fast <c>[Category("Unit")]</c> tier and
/// validate both the flag-ON (providers registered) and flag-OFF (nothing registered) paths. The
/// registration reads configuration at service-registration time, which is fully populated in
/// production; a host-level test cannot exercise it because WebApplicationFactory applies config
/// overrides only at Build time (after registration).
/// </summary>
[TestFixture]
[Category("Unit")]
public class ObservabilityTests
{
    [Test]
    public void Flag_On_Registers_Tracer_And_Meter_Providers()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Features:OpenTelemetry"] = "true",
                ["Observability:OtlpEndpoint"] = "http://127.0.0.1:4317",
            })
            .Build();

        var services = new ServiceCollection().AddLogging();
        services.AddApplicationObservability(configuration);

        using var provider = services.BuildServiceProvider();
        Assert.That(provider.GetService<TracerProvider>(), Is.Not.Null, "TracerProvider should be registered when the flag is on.");
        Assert.That(provider.GetService<MeterProvider>(), Is.Not.Null, "MeterProvider should be registered when the flag is on.");
    }

    [Test]
    public void Flag_Off_Registers_Nothing()
    {
        // No Features:OpenTelemetry key => defaults to false.
        var configuration = new ConfigurationBuilder().Build();

        var services = new ServiceCollection().AddLogging();
        services.AddApplicationObservability(configuration);

        using var provider = services.BuildServiceProvider();
        Assert.That(provider.GetService<TracerProvider>(), Is.Null, "Nothing should be registered when the flag is off.");
        Assert.That(provider.GetService<MeterProvider>(), Is.Null, "Nothing should be registered when the flag is off.");
    }
}
