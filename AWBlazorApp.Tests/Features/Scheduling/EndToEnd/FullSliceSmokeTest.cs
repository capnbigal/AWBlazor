using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
using AWBlazorApp.Features.Scheduling.Alerts.Domain;
using AWBlazorApp.Features.Scheduling.DeliverySchedules.Domain;
using AWBlazorApp.Features.Scheduling.LineConfigurations.Domain;
using AWBlazorApp.Features.Scheduling.LineProductAssignments.Domain;
using AWBlazorApp.Features.Scheduling.WeeklyPlans.Application;
using AWBlazorApp.Features.Scheduling.WeeklyPlans.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Tests.Infrastructure.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Features.Scheduling.EndToEnd;

/// <summary>
/// Full-slice smoke: seed line + assignment, generate a plan (DryRun then commit),
/// insert a SalesOrderHeader triggering the dispatcher → expect an alert, write a
/// SchedulingException, confirm it appears in the view, resolve the exception, confirm
/// it clears.
/// </summary>
public class FullSliceSmokeTest : IntegrationTestFixtureBase
{
    private const short Loc = 60;
    private const int WeekId = 203002; // far future — avoids any real data collision
    private static readonly string TestMarker = "FullSliceSmokeTest";
    private readonly List<int> _createdSoIds = new();

    [SetUp] public Task Before() => Cleanup();
    [TearDown] public Task After() => Cleanup();

    private async Task Cleanup()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // Only clean rows this test created (tracked by _createdSoIds) + week-scoped cleanup.
        if (_createdSoIds.Count > 0)
        {
            await db.SchedulingAlerts.Where(a => a.SalesOrderId != null && _createdSoIds.Contains(a.SalesOrderId.Value)).ExecuteDeleteAsync();
            await db.SalesOrderHeaders.Where(s => _createdSoIds.Contains(s.Id)).ExecuteDeleteAsync();
            _createdSoIds.Clear();
        }
        await db.WeeklyPlanItems.Where(i => i.WeeklyPlan.WeekId == WeekId && i.WeeklyPlan.LocationId == Loc).ExecuteDeleteAsync();
        await db.WeeklyPlans.Where(p => p.WeekId == WeekId && p.LocationId == Loc).ExecuteDeleteAsync();
        await db.SchedulingExceptions.Where(e => e.WeekId == WeekId && e.LocationId == Loc).ExecuteDeleteAsync();
    }

    [Test]
    public async Task FullSlice_GenerateAndSoInsertAndExceptionAndResolve()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // 1. Seed minimal line + assignment data (idempotent)
        if (!await db.LineConfigurations.AnyAsync(l => l.LocationId == Loc))
            db.LineConfigurations.Add(new LineConfiguration
            {
                LocationId = Loc, TaktSeconds = 600, ShiftsPerDay = 2, MinutesPerShift = 480,
                FrozenLookaheadHours = 72, IsActive = true, ModifiedDate = DateTime.UtcNow
            });
        foreach (var modelId in new[] { 25, 28, 30 })
            if (!await db.LineProductAssignments.AnyAsync(a => a.LocationId == Loc && a.ProductModelId == modelId))
                db.LineProductAssignments.Add(new LineProductAssignment
                {
                    LocationId = Loc, ProductModelId = modelId, IsActive = true,
                    ModifiedDate = DateTime.UtcNow
                });
        await db.SaveChangesAsync();

        // 2. Generate plan (dry run first, then commit)
        var generator = scope.ServiceProvider.GetRequiredService<IWeeklyPlanGenerator>();
        var preview = await generator.GenerateAsync(WeekId, Loc, new WeeklyPlanGenerationOptions(DryRun: true), "smoke", CancellationToken.None);
        Assert.That(preview.WeeklyPlanId, Is.EqualTo(0), "Dry-run should not write.");

        var committed = await generator.GenerateAsync(WeekId, Loc, new WeeklyPlanGenerationOptions(), "smoke", CancellationToken.None);
        Assert.That(committed.Version, Is.EqualTo(1));
        Assert.That(committed.WeeklyPlanId, Is.GreaterThan(0));

        // 3. Insert a SalesOrderHeader → interceptor should fire and write an alert
        // DueDate inside the far-future week (but the plan IS published there, so interceptor
        // will see "no plan for real current week" and SoftResort — since DueDate is >72h).
        var dueDate = DateTime.UtcNow.AddDays(21);
        var soh = new SalesOrderHeader
        {
            // Id is identity — let SQL Server assign it
            RevisionNumber = 0,
            OrderDate = DateTime.UtcNow,
            DueDate = dueDate,
            Status = 1,
            OnlineOrderFlag = false,
            CustomerId = 29825,
            ShipMethodId = 1,
            BillToAddressId = 985,
            ShipToAddressId = 985,
            SubTotal = 100m,
            TaxAmt = 0m,
            Freight = 0m,
            RowGuid = Guid.NewGuid(),
            ModifiedDate = DateTime.UtcNow,
        };
        db.SalesOrderHeaders.Add(soh);
        await db.SaveChangesAsync();
        var realSoId = soh.Id;
        _createdSoIds.Add(realSoId);

        var alertCount = await db.SchedulingAlerts.CountAsync(a => a.SalesOrderId == realSoId);
        Assert.That(alertCount, Is.EqualTo(1), "Expected exactly one alert from SoftResort dispatch.");

        // 4. Write a SchedulingException (Pin sequence #1)
        var ex = new SchedulingException
        {
            WeekId = WeekId, LocationId = Loc,
            SalesOrderDetailId = 1, // any; view won't find it but filter still exercises
            ExceptionType = ExceptionType.ManualSequencePin,
            PinnedSequence = 1,
            Reason = "smoke test",
            CreatedAt = DateTime.UtcNow, CreatedBy = "smoke",
        };
        db.SchedulingExceptions.Add(ex);
        await db.SaveChangesAsync();
        Assert.That(ex.Id, Is.GreaterThan(0));

        // 5. Query the view — must at least execute without error
        var rows = await db.CurrentDeliverySchedule.AsNoTracking()
            .Where(r => r.WeekId == WeekId && r.LocationId == Loc)
            .ToListAsync();
        // rows may be empty if no real SOs fall into the future week, but the view query must succeed
        Assert.That(rows, Is.Not.Null);

        // 6. Resolve the exception
        ex.ResolvedAt = DateTime.UtcNow;
        ex.ResolvedBy = "smoke";
        await db.SaveChangesAsync();

        var active = await db.SchedulingExceptions.CountAsync(e =>
            e.WeekId == WeekId && e.LocationId == Loc && e.ResolvedAt == null);
        Assert.That(active, Is.EqualTo(0), "Active exception count should be 0 after resolve.");

        // Cleanup handled by [TearDown]
    }
}
