using AWBlazorApp.Features.Processes.Timelines.Application;
using AWBlazorApp.Features.Processes.Timelines.Application.HopQueries;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Tests.Infrastructure.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Features.Processes.Timelines;

public class HopQueryTests : IntegrationTestFixtureBase
{
    private ApplicationDbContext Db(IServiceScope scope)
        => scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    [Test]
    public void ShipmentFromSalesOrderHeader_Properties_Match_Spec()
    {
        var sut = new ShipmentFromSalesOrderHeader();
        Assert.That(sut.ParentEntity, Is.EqualTo("SalesOrderHeader"));
        Assert.That(sut.ChildEntity, Is.EqualTo("Shipment"));
        Assert.That(sut.ForeignKey, Is.EqualTo("SalesOrderId"));
    }

    [Test]
    public async Task ShipmentFromSalesOrderHeader_Empty_Parents_Returns_Empty()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = Db(scope);
        var sut = new ShipmentFromSalesOrderHeader();
        var result = await sut.GetChildIdsAsync(db, Array.Empty<string>(), CancellationToken.None);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task ShipmentLineFromShipment_Empty_Parents_Returns_Empty()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = Db(scope);
        var sut = new ShipmentLineFromShipment();
        var result = await sut.GetChildIdsAsync(db, Array.Empty<string>(), CancellationToken.None);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GoodsReceiptFromPurchaseOrderHeader_Empty_Parents_Returns_Empty()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = Db(scope);
        var sut = new GoodsReceiptFromPurchaseOrderHeader();
        var result = await sut.GetChildIdsAsync(db, Array.Empty<string>(), CancellationToken.None);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GoodsReceiptLineFromGoodsReceipt_Empty_Parents_Returns_Empty()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = Db(scope);
        var sut = new GoodsReceiptLineFromGoodsReceipt();
        var result = await sut.GetChildIdsAsync(db, Array.Empty<string>(), CancellationToken.None);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task ShipmentFromSalesOrderHeader_Known_Parent_Executes_Cleanly()
    {
        // AW SO 43659 is the first-ever SO (stable since 2011).
        // The lgx.Shipment rows referencing it may be zero — this test just asserts the query
        // executes end-to-end and returns a non-null list.
        using var scope = Factory.Services.CreateScope();
        await using var db = Db(scope);
        var sut = new ShipmentFromSalesOrderHeader();
        var result = await sut.GetChildIdsAsync(db, new[] { "43659" }, CancellationToken.None);
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task ShipmentFromSalesOrderHeader_GetParent_Unknown_Child_Returns_Null()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = Db(scope);
        var sut = new ShipmentFromSalesOrderHeader();
        var result = await sut.GetParentIdAsync(db, "999999999", CancellationToken.None);
        Assert.That(result, Is.Null);
    }
}
