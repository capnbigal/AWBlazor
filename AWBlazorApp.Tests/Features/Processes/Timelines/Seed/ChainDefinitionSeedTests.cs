using AWBlazorApp.Features.Processes.Timelines.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Tests.Infrastructure.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Text.Json;

namespace AWBlazorApp.Tests.Features.Processes.Timelines.Seed;

public class ChainDefinitionSeedTests : IntegrationTestFixtureBase
{
    [Test]
    public async Task SalesToShip_And_PurchaseToReceive_Are_Seeded_With_Parseable_Steps()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var rows = await db.ProcessChainDefinitions.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Code)
            .ToListAsync();

        Assert.That(rows.Any(c => c.Code == "sales-to-ship" && c.Name == "Sales to Ship"), Is.True,
            "sales-to-ship chain missing");
        Assert.That(rows.Any(c => c.Code == "purchase-to-receive" && c.Name == "Purchase to Receive"), Is.True,
            "purchase-to-receive chain missing");

        var sales = rows.Single(c => c.Code == "sales-to-ship");
        var steps = JsonSerializer.Deserialize<ChainStep[]>(sales.StepsJson,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        Assert.That(steps, Is.Not.Null);
        Assert.That(steps!.Length, Is.EqualTo(3));
        Assert.That(steps[0].Entity, Is.EqualTo("SalesOrderHeader"));
        Assert.That(steps[0].Role, Is.EqualTo("Root"));
        Assert.That(steps[1].Entity, Is.EqualTo("Shipment"));
        Assert.That(steps[1].ForeignKey, Is.EqualTo("SalesOrderId"));
        Assert.That(steps[2].Entity, Is.EqualTo("ShipmentLine"));
        Assert.That(steps[2].ForeignKey, Is.EqualTo("ShipmentId"));
    }
}
