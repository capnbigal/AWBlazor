using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
using AWBlazorApp.Features.Scheduling.Alerts.Domain;
using AWBlazorApp.Features.Scheduling.LineConfigurations.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Tests.Infrastructure.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Features.Scheduling.Dispatcher;

/// <summary>
/// Exercises the full interceptor chain: SalesOrderHeader insert via
/// ApplicationDbContext.SaveChangesAsync fires SchedulingDispatchInterceptor,
/// which invokes SchedulingDispatcher, which writes an alert. The nested
/// SaveChanges for that alert re-enters the interceptor, which self-skips
/// via the AsyncLocal guard — verifying no duplicate-alert / infinite loop.
/// </summary>
public class InterceptorIntegrationTests : IntegrationTestFixtureBase
{
    private const short Loc = 60;
    private readonly List<int> _soIdsToClean = new();

    [SetUp]
    public async Task SeedLine()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (!await db.LineConfigurations.AnyAsync(l => l.LocationId == Loc))
        {
            db.LineConfigurations.Add(new LineConfiguration
            {
                LocationId = Loc, TaktSeconds = 600, ShiftsPerDay = 2, MinutesPerShift = 480,
                FrozenLookaheadHours = 72, IsActive = true, ModifiedDate = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
    }

    [TearDown]
    public async Task Cleanup()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (_soIdsToClean.Count > 0)
        {
            await db.SchedulingAlerts.Where(a => a.SalesOrderId != null && _soIdsToClean.Contains(a.SalesOrderId.Value)).ExecuteDeleteAsync();
            await db.SalesOrderHeaders.Where(s => _soIdsToClean.Contains(s.Id)).ExecuteDeleteAsync();
            _soIdsToClean.Clear();
        }
    }

    [Test]
    public async Task InsertingSO_OutsideFrozenWindow_WritesExactlyOneInfoAlert()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var soh = CreateMinimalSoh(DateTime.UtcNow.AddDays(30)); // well outside 72h lookahead
        db.SalesOrderHeaders.Add(soh);
        await db.SaveChangesAsync();
        _soIdsToClean.Add(soh.Id);

        var alerts = await db.SchedulingAlerts.AsNoTracking()
            .Where(a => a.SalesOrderId == soh.Id).ToListAsync();
        Assert.That(alerts, Has.Count.EqualTo(1), "expected exactly one alert — re-entry guard failed if > 1");
        Assert.That(alerts[0].Severity, Is.EqualTo(AlertSeverity.Info));
    }

    private static SalesOrderHeader CreateMinimalSoh(DateTime dueDate) => new()
    {
        RevisionNumber = 0,
        OrderDate = DateTime.UtcNow,
        DueDate = dueDate,
        Status = 1,                    // InProcess
        OnlineOrderFlag = false,
        CustomerId = 29825,            // valid AW demo customer
        ShipMethodId = 1,
        BillToAddressId = 985,
        ShipToAddressId = 985,
        SubTotal = 100m,
        TaxAmt = 0m,
        Freight = 0m,
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow
    };
}
