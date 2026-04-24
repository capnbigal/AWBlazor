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
        // Use a SO ID well beyond anything AW has (max real SO is ~75124 in the full sample).
        // The root set still gets populated with the supplied rootId (walker doesn't validate
        // root existence), but the downstream hop queries return empty.
        using var scope = Factory.Services.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<IProcessChainResolver>();
        var result = await sut.ResolveAsync("sales-to-ship", "999999999", CancellationToken.None);
        Assert.That(result.CollectedIds["SalesOrderHeader"], Is.EqualTo(new[] { "999999999" }));
        Assert.That(result.CollectedIds["Shipment"], Is.Empty);
        Assert.That(result.CollectedIds["ShipmentLine"], Is.Empty);
    }
}
