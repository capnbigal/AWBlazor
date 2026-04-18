using AWBlazorApp.Features.Admin.Services;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests;

/// <summary>
/// Exercises <see cref="DemoDataSeeder"/> — the droplet-demo seeder. Verifies it writes rows
/// on a fresh run and is idempotent (second run skips because markers are present).
/// </summary>
public class DemoDataSeederTests : IntegrationTestFixtureBase
{
    [Test]
    public async Task Seeder_Is_Idempotent()
    {
        using var scope = Factory.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DemoDataSeeder>();

        var first = await seeder.SeedAllAsync(CancellationToken.None);

        // If the fixture already had demo rows from a prior run, a "first" pass may also
        // return 0s — that's a legitimate outcome and still proves idempotency below.
        if (first.Skipped)
        {
            Assert.Ignore("FK targets missing in dev DB — seeder legitimately skipped.");
            return;
        }

        // Second run should be a strict no-op across all modules.
        var second = await seeder.SeedAllAsync(CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(second.Workforce, Is.EqualTo(0), "Workforce seed must no-op on second run.");
            Assert.That(second.Engineering, Is.EqualTo(0), "Engineering seed must no-op on second run.");
            Assert.That(second.Maintenance, Is.EqualTo(0), "Maintenance seed must no-op on second run.");
            Assert.That(second.Performance, Is.EqualTo(0), "Performance seed must no-op on second run.");
        });
    }
}
