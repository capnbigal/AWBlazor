using AWBlazorApp.Features.Processes.Timelines.Application;
using AWBlazorApp.Features.Processes.Timelines.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Tests.Infrastructure.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Features.Processes.Timelines;

public class EndToEndSmokeTests : IntegrationTestFixtureBase
{
    private const string SentinelRoot = "__ProcessTimelineTestRoot";

    [SetUp] public Task Before() => Cleanup();
    [TearDown] public Task After() => Cleanup();

    private async Task Cleanup()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.AuditLogs.Where(a => a.EntityType.StartsWith("__ProcessTimelineTest")).ExecuteDeleteAsync();
    }

    [Test]
    public async Task Composer_Builds_Ordered_Timeline_For_Sentinel_Root()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var baseTime = DateTime.UtcNow.AddDays(-1);
        db.AuditLogs.AddRange(
            new AuditLog { EntityType = SentinelRoot, EntityId = "7000", Action = "Created",
                ChangedDate = baseTime.AddMinutes(1), ChangedBy = "alice@" },
            new AuditLog { EntityType = SentinelRoot, EntityId = "7000", Action = "Updated",
                ChangedDate = baseTime.AddMinutes(5), ChangedBy = "bob@" },
            new AuditLog { EntityType = SentinelRoot, EntityId = "7000", Action = "Deleted",
                ChangedDate = baseTime.AddMinutes(10), ChangedBy = "alice@" });
        await db.SaveChangesAsync();

        var composer = scope.ServiceProvider.GetRequiredService<IProcessTimelineComposer>();
        var def = new ProcessChainDefinition
        {
            Id = 9999, Code = "sentinel", Name = "Sentinel", IsActive = true, StepsJson = "[]"
        };
        var instance = new ChainInstance(def, "7000",
            new Dictionary<string, IReadOnlyList<string>> { [SentinelRoot] = new[] { "7000" } });

        var timeline = await composer.ComposeAsync(instance, CancellationToken.None);
        Assert.That(timeline.Events, Has.Count.EqualTo(3));
        Assert.That(timeline.Events[0].Action, Is.EqualTo("Created"));
        Assert.That(timeline.Events[1].Action, Is.EqualTo("Updated"));
        Assert.That(timeline.Events[2].Action, Is.EqualTo("Deleted"));
        Assert.That(timeline.Events.Select(e => e.ChangedBy), Is.EqualTo(new[] { "alice@", "bob@", "alice@" }));
        Assert.That(timeline.Truncated, Is.False);
    }
}
