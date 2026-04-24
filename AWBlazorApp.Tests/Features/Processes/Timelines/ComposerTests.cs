using AWBlazorApp.Features.Processes.Timelines.Application;
using AWBlazorApp.Features.Processes.Timelines.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Tests.Infrastructure.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Features.Processes.Timelines;

public class ComposerTests : IntegrationTestFixtureBase
{
    private const string SentinelRoot  = "__ProcessTimelineTestRoot";
    private const string SentinelChild = "__ProcessTimelineTestChild";

    [SetUp] public Task Before() => Cleanup();
    [TearDown] public Task After() => Cleanup();

    private async Task Cleanup()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.AuditLogs
            .Where(a => a.EntityType == SentinelRoot || a.EntityType == SentinelChild)
            .ExecuteDeleteAsync();
    }

    private ChainInstance MakeInstance(string rootId, IEnumerable<string> childIds)
    {
        var def = new ProcessChainDefinition
        {
            Id = 999, Code = "sentinel-test", Name = "Sentinel", IsActive = true,
            StepsJson = "[]",
            ModifiedDate = DateTime.UtcNow,
        };
        var collected = new Dictionary<string, IReadOnlyList<string>>
        {
            [SentinelRoot]  = new[] { rootId },
            [SentinelChild] = childIds.ToArray(),
        };
        return new ChainInstance(def, rootId, collected);
    }

    [Test]
    public async Task Compose_Orders_Events_Ascending_By_ChangedDate()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var baseTime = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        db.AuditLogs.AddRange(
            new AuditLog { EntityType = SentinelRoot, EntityId = "1", Action = "Created", ChangedDate = baseTime.AddMinutes(10), ChangedBy = "a@" },
            new AuditLog { EntityType = SentinelChild, EntityId = "100", Action = "Created", ChangedDate = baseTime.AddMinutes(5), ChangedBy = "b@" },
            new AuditLog { EntityType = SentinelRoot, EntityId = "1", Action = "Updated", ChangedDate = baseTime.AddMinutes(20), ChangedBy = "a@" });
        await db.SaveChangesAsync();

        var sut = scope.ServiceProvider.GetRequiredService<IProcessTimelineComposer>();
        var instance = MakeInstance("1", new[] { "100" });
        var result = await sut.ComposeAsync(instance, CancellationToken.None);

        Assert.That(result.Events, Has.Count.EqualTo(3));
        Assert.That(result.Events[0].EntityType, Is.EqualTo(SentinelChild));
        Assert.That(result.Events[1].EntityType, Is.EqualTo(SentinelRoot));
        Assert.That(result.Events[2].Action, Is.EqualTo("Updated"));
        Assert.That(result.Truncated, Is.False);
    }

    [Test]
    public async Task Compose_Sets_Truncated_When_More_Than_500_Events()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var baseTime = DateTime.UtcNow.AddDays(-1);
        var rows = Enumerable.Range(0, 510)
            .Select(i => new AuditLog
            {
                EntityType = SentinelRoot, EntityId = "1", Action = "Updated",
                ChangedDate = baseTime.AddSeconds(i), ChangedBy = "bulk@"
            });
        db.AuditLogs.AddRange(rows);
        await db.SaveChangesAsync();

        var sut = scope.ServiceProvider.GetRequiredService<IProcessTimelineComposer>();
        var instance = MakeInstance("1", Array.Empty<string>());
        var result = await sut.ComposeAsync(instance, CancellationToken.None);

        Assert.That(result.Events, Has.Count.EqualTo(500));
        Assert.That(result.Truncated, Is.True);
    }

    [Test]
    public async Task Compose_Empty_Instance_Returns_Empty_Events()
    {
        using var scope = Factory.Services.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<IProcessTimelineComposer>();
        var empty = new ChainInstance(
            new ProcessChainDefinition { Code = "x", Name = "X", StepsJson = "[]", IsActive = true },
            "0",
            new Dictionary<string, IReadOnlyList<string>>());
        var result = await sut.ComposeAsync(empty, CancellationToken.None);
        Assert.That(result.Events, Is.Empty);
        Assert.That(result.Truncated, Is.False);
    }
}
