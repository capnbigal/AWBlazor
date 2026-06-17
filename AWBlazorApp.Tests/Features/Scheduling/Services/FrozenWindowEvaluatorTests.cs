using AWBlazorApp.Features.Scheduling.LineConfigurations.Domain;
using AWBlazorApp.Features.Scheduling.Services;
using AWBlazorApp.Features.Scheduling.WeeklyPlans.Domain;
using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Tests.Infrastructure.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Features.Scheduling.Services;

public class FrozenWindowEvaluatorTests : IntegrationTestFixtureBase
{
    private const short PilotLocation = 60;

    [SetUp]
    public async Task SeedLine()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (!await db.LineConfigurations.AnyAsync(l => l.LocationId == PilotLocation))
        {
            db.LineConfigurations.Add(new LineConfiguration
            {
                LocationId = PilotLocation, TaktSeconds = 600, ShiftsPerDay = 2,
                MinutesPerShift = 480, FrozenLookaheadHours = 72, IsActive = true,
                ModifiedDate = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
    }

    [TearDown]
    public async Task Cleanup()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.WeeklyPlans.Where(p => p.LocationId == PilotLocation).ExecuteDeleteAsync();
    }

    [Test]
    public async Task InFrozenWindow_False_When_No_Plan_Published()
    {
        using var scope = Factory.Services.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<IFrozenWindowEvaluator>();
        // Pick a DueDate in a week that has no WeeklyPlan (far future)
        var soh = new SalesOrderHeader { DueDate = DateTime.UtcNow.AddDays(90) };
        var result = await sut.EvaluateAsync(soh, DateTime.UtcNow, PilotLocation);
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task InFrozenWindow_False_When_Plan_Exists_But_Due_Beyond_Lookahead()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var dueDate = DateTime.UtcNow.AddHours(200); // beyond 72h lookahead
        var weekId = IsoWeekHelper.FromDate(dueDate);
        if (!await db.WeeklyPlans.AnyAsync(p => p.WeekId == weekId && p.LocationId == PilotLocation))
        {
            db.WeeklyPlans.Add(new WeeklyPlan
            {
                WeekId = weekId, LocationId = PilotLocation, Version = 1,
                PublishedAt = DateTime.UtcNow, PublishedBy = "test"
            });
            await db.SaveChangesAsync();
        }

        var sut = scope.ServiceProvider.GetRequiredService<IFrozenWindowEvaluator>();
        var soh = new SalesOrderHeader { DueDate = dueDate };
        var result = await sut.EvaluateAsync(soh, DateTime.UtcNow, PilotLocation);
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task InFrozenWindow_True_When_Plan_Exists_AND_Due_Within_Lookahead()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var dueDate = DateTime.UtcNow.AddHours(24); // inside 72h lookahead
        var weekId = IsoWeekHelper.FromDate(dueDate);
        if (!await db.WeeklyPlans.AnyAsync(p => p.WeekId == weekId && p.LocationId == PilotLocation))
        {
            db.WeeklyPlans.Add(new WeeklyPlan
            {
                WeekId = weekId, LocationId = PilotLocation, Version = 1,
                PublishedAt = DateTime.UtcNow, PublishedBy = "test"
            });
            await db.SaveChangesAsync();
        }

        var sut = scope.ServiceProvider.GetRequiredService<IFrozenWindowEvaluator>();
        var soh = new SalesOrderHeader { DueDate = dueDate };
        var result = await sut.EvaluateAsync(soh, DateTime.UtcNow, PilotLocation);
        Assert.That(result, Is.True);
    }
}
