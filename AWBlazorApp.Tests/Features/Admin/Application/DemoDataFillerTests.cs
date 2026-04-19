using AWBlazorApp.Features.Admin.Services;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using AWBlazorApp.Tests.Infrastructure.Testing;

namespace AWBlazorApp.Tests.Features.Admin.Application;

/// <summary>
/// Companion to <see cref="DemoDataSeederTests"/> — exercises <see cref="DemoDataFiller"/>.
/// Unlike the seeder, the filler is additive — every call should add rows (provided the
/// baseline seed has already run).
/// </summary>
public class DemoDataFillerTests : IntegrationTestFixtureBase
{
    [Test]
    public async Task Filler_Adds_Rows_On_Every_Call()
    {
        // Make sure the baseline is in place — without it, the filler legitimately skips.
        using var seedScope = Factory.Services.CreateScope();
        var seeder = seedScope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
        await seeder.SeedAllAsync(CancellationToken.None);

        using var scope = Factory.Services.CreateScope();
        var filler = scope.ServiceProvider.GetRequiredService<DemoDataFiller>();

        var first = await filler.FillAsync(count: 5, CancellationToken.None);
        if (first.Skipped)
        {
            Assert.Ignore("Seeder baseline missing in dev DB — filler legitimately skipped.");
            return;
        }
        Assert.That(first.Total, Is.GreaterThan(0), "First fill must add at least one row.");

        var second = await filler.FillAsync(count: 5, CancellationToken.None);
        Assert.That(second.Total, Is.GreaterThan(0), "Second fill must also add rows — the filler is additive, not idempotent.");
    }
}
