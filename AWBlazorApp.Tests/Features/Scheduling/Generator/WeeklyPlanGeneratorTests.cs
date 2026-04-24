using AWBlazorApp.Features.Scheduling.LineConfigurations.Domain;
using AWBlazorApp.Features.Scheduling.LineProductAssignments.Domain;
using AWBlazorApp.Features.Scheduling.WeeklyPlans.Application;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Tests.Infrastructure.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Features.Scheduling.Generator;

public class WeeklyPlanGeneratorTests : IntegrationTestFixtureBase
{
    private const short Loc = 60;
    private const int TestWeekId = 203001; // far future — avoids colliding with real SOs

    [SetUp]
    public async Task Setup()
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
        }
        foreach (var modelId in new[] { 25, 28, 30 }) // valid AW ProductModelIDs for bikes
        {
            if (!await db.LineProductAssignments.AnyAsync(a => a.LocationId == Loc && a.ProductModelId == modelId))
            {
                db.LineProductAssignments.Add(new LineProductAssignment
                {
                    LocationId = Loc, ProductModelId = modelId, IsActive = true,
                    ModifiedDate = DateTime.UtcNow
                });
            }
        }
        await db.SaveChangesAsync();
        // clean leftover plans for target week
        await db.WeeklyPlanItems.Where(i => i.WeeklyPlan.WeekId == TestWeekId && i.WeeklyPlan.LocationId == Loc).ExecuteDeleteAsync();
        await db.WeeklyPlans.Where(p => p.WeekId == TestWeekId && p.LocationId == Loc).ExecuteDeleteAsync();
    }

    [TearDown]
    public async Task Cleanup()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.WeeklyPlanItems.Where(i => i.WeeklyPlan.WeekId == TestWeekId && i.WeeklyPlan.LocationId == Loc).ExecuteDeleteAsync();
        await db.WeeklyPlans.Where(p => p.WeekId == TestWeekId && p.LocationId == Loc).ExecuteDeleteAsync();
    }

    [Test]
    public async Task DryRun_ReturnsResult_And_Writes_Nothing()
    {
        using var scope = Factory.Services.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<IWeeklyPlanGenerator>();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var before = await db.WeeklyPlans.CountAsync(p => p.WeekId == TestWeekId && p.LocationId == Loc);
        var result = await sut.GenerateAsync(TestWeekId, Loc,
            new WeeklyPlanGenerationOptions(DryRun: true), "tester", CancellationToken.None);
        var after = await db.WeeklyPlans.CountAsync(p => p.WeekId == TestWeekId && p.LocationId == Loc);

        Assert.That(after, Is.EqualTo(before));
        Assert.That(result.WeeklyPlanId, Is.EqualTo(0));
    }

    [Test]
    public async Task Generate_V1_WhenNoPriorPlan()
    {
        using var scope = Factory.Services.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<IWeeklyPlanGenerator>();

        var result = await sut.GenerateAsync(TestWeekId, Loc,
            new WeeklyPlanGenerationOptions(), "tester", CancellationToken.None);

        Assert.That(result.Version, Is.EqualTo(1));
        Assert.That(result.WeeklyPlanId, Is.GreaterThan(0));
    }

    [Test]
    public void Precondition_Fails_When_No_LineConfiguration()
    {
        using var scope = Factory.Services.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<IWeeklyPlanGenerator>();
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await sut.GenerateAsync(TestWeekId, locationId: 999 /* unconfigured */,
                new WeeklyPlanGenerationOptions(), "tester", CancellationToken.None));
    }
}
