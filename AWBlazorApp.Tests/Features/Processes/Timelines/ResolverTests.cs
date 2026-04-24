using AWBlazorApp.Features.Processes.Timelines.Application;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Tests.Infrastructure.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Features.Processes.Timelines;

public class ResolverTests : IntegrationTestFixtureBase
{
    [Test]
    public async Task Resolve_Known_Root_Returns_Instance_With_Root_Set_Populated()
    {
        using var scope = Factory.Services.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<IProcessChainResolver>();
        var result = await sut.ResolveAsync("sales-to-ship", "43659", CancellationToken.None);
        Assert.That(result.Definition.Code, Is.EqualTo("sales-to-ship"));
        Assert.That(result.RootEntityId, Is.EqualTo("43659"));
        Assert.That(result.CollectedIds["SalesOrderHeader"], Contains.Item("43659"));
    }

    [Test]
    public void Resolve_Unknown_Chain_Throws_ChainDefinitionNotFoundException()
    {
        using var scope = Factory.Services.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<IProcessChainResolver>();
        Assert.ThrowsAsync<ChainDefinitionNotFoundException>(async () =>
            await sut.ResolveAsync("nonexistent-chain", "1", CancellationToken.None));
    }

    [Test]
    public async Task Resolve_Root_With_No_Downstream_Returns_Empty_Child_Sets()
    {
        using var scope = Factory.Services.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<IProcessChainResolver>();
        var result = await sut.ResolveAsync("sales-to-ship", "43659", CancellationToken.None);
        Assert.That(result.CollectedIds.ContainsKey("SalesOrderHeader"), Is.True);
        Assert.That(result.CollectedIds["SalesOrderHeader"], Has.Count.EqualTo(1));
        // Shipment + ShipmentLine keys present but possibly empty — no Shipments point at AW SO 43659 in sample data.
        var shipments = result.CollectedIds.TryGetValue("Shipment", out var s) ? s : Array.Empty<string>();
        Assert.That(shipments, Is.Empty);
    }
}
