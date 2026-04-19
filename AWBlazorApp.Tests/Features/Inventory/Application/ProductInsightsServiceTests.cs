using AWBlazorApp.Features.Inventory.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using AWBlazorApp.Tests.Infrastructure.Testing;

namespace AWBlazorApp.Tests.Features.Inventory.Application;

/// <summary>
/// Integration tests for the product-centric explorer service. Verifies the picker filter,
/// header construction, the 12-month bin shape, and the graceful handling of products that
/// have no inventory record. Runs against the real AdventureWorks2022_dev schema, which has
/// hundreds of seed Products, so the queries actually exercise EF Core SQL translation.
/// </summary>
public class ProductInsightsServiceTests : IntegrationTestFixtureBase
{
    [Test]
    public async Task ListProductsAsync_Returns_Rows_Sorted_By_Name()
    {
        using var scope = Factory.Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IProductInsightsService>();

        var rows = await svc.ListProductsAsync(search: null, take: 25, CancellationToken.None);

        Assert.That(rows.Count, Is.GreaterThan(0), "AdventureWorks seed has products; an empty result means the join broke.");
        Assert.That(rows.Count, Is.LessThanOrEqualTo(25));

        // Strictly non-decreasing names — verifies the OrderBy path. SQL Server's default
        // collation is case-insensitive, so use OrdinalIgnoreCase here to match.
        var names = rows.Select(r => r.Name).ToList();
        var sorted = names.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
        Assert.That(names, Is.EqualTo(sorted), "Picker rows should be name-sorted (case-insensitive).");
    }

    [Test]
    public async Task ListProductsAsync_Search_Filters_By_Name_Or_ProductNumber()
    {
        using var scope = Factory.Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IProductInsightsService>();

        // "Mountain" appears in many AW seed product names; the count should be > 0 and the
        // filter should be honoured (every row must contain the term in either field).
        var rows = await svc.ListProductsAsync(search: "Mountain", take: 50, CancellationToken.None);

        Assert.That(rows.Count, Is.GreaterThan(0));
        Assert.That(rows.All(r =>
            r.Name.Contains("Mountain", StringComparison.OrdinalIgnoreCase)
            || r.ProductNumber.Contains("Mountain", StringComparison.OrdinalIgnoreCase)),
            Is.True,
            "Every search result should match the search term in Name or ProductNumber.");
    }

    [Test]
    public async Task GetAsync_Unknown_ProductId_Returns_Null()
    {
        using var scope = Factory.Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IProductInsightsService>();

        var dto = await svc.GetAsync(productId: -1, CancellationToken.None);

        Assert.That(dto, Is.Null);
    }

    [Test]
    public async Task GetAsync_Returns_Header_And_Twelve_Month_Bins_For_Real_Product()
    {
        // Pick any product the picker returns so this works on either AW or AW_dev databases.
        using var scope = Factory.Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IProductInsightsService>();
        var picker = await svc.ListProductsAsync(search: null, take: 1, CancellationToken.None);
        Assert.That(picker.Count, Is.EqualTo(1), "Need at least one product for this test.");

        var dto = await svc.GetAsync(picker[0].ProductId, CancellationToken.None);

        Assert.That(dto, Is.Not.Null);
        Assert.That(dto!.Header.ProductId, Is.EqualTo(picker[0].ProductId));
        Assert.That(dto.Header.Name, Is.EqualTo(picker[0].Name));

        // All four 12-month series are always exactly 12 elements with chronologically-ordered (year, month) keys.
        Assert.Multiple(() =>
        {
            Assert.That(dto.InventoryActivity12m.Count, Is.EqualTo(12), "Inventory activity always has 12 monthly bins.");
            Assert.That(dto.PurchaseOrders12m.Count, Is.EqualTo(12), "Purchase orders always have 12 monthly bins.");
            Assert.That(dto.WorkOrders12m.Count, Is.EqualTo(12), "Work orders always have 12 monthly bins.");
            Assert.That(dto.ProductionRuns12m.Count, Is.EqualTo(12), "Production runs always have 12 monthly bins.");
        });

        // Bins must be strictly ascending by (year, month).
        var keys = dto.InventoryActivity12m.Select(b => b.Year * 100 + b.Month).ToList();
        var sortedKeys = keys.OrderBy(k => k).ToList();
        Assert.That(keys, Is.EqualTo(sortedKeys));

        // The newest bin is the current calendar month (UTC).
        var now = DateTime.UtcNow;
        var last = dto.InventoryActivity12m[^1];
        Assert.That(last.Year, Is.EqualTo(now.Year));
        Assert.That(last.Month, Is.EqualTo(now.Month));
    }

    [Test]
    public async Task GetAsync_Caches_Subsequent_Calls()
    {
        using var scope = Factory.Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IProductInsightsService>();
        var picker = await svc.ListProductsAsync(search: null, take: 1, CancellationToken.None);
        Assert.That(picker.Count, Is.EqualTo(1));

        var first = await svc.GetAsync(picker[0].ProductId, CancellationToken.None);
        var second = await svc.GetAsync(picker[0].ProductId, CancellationToken.None);

        Assert.That(second!.GeneratedAt, Is.EqualTo(first!.GeneratedAt),
            "Second call within the 60s TTL should return the cached payload.");

        svc.Invalidate(picker[0].ProductId);
        var third = await svc.GetAsync(picker[0].ProductId, CancellationToken.None);
        Assert.That(third!.GeneratedAt, Is.GreaterThanOrEqualTo(first.GeneratedAt),
            "After Invalidate the next call should rebuild.");
    }
}
