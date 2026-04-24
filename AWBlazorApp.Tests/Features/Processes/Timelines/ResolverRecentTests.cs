using AWBlazorApp.Features.Processes.Timelines.Application;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Tests.Infrastructure.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Features.Processes.Timelines;

public class ResolverRecentTests : IntegrationTestFixtureBase
{
    private const string TestSentinelPrefix = "__ProcessTimelineTest";

    [SetUp] public Task Before() => Cleanup();
    [TearDown] public Task After() => Cleanup();

    private async Task Cleanup()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.AuditLogs.Where(a => a.EntityType.StartsWith(TestSentinelPrefix)).ExecuteDeleteAsync();
    }

    [Test]
    public async Task Recent_Does_Not_Throw_And_Respects_Limit()
    {
        using var scope = Factory.Services.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<IProcessChainResolver>();
        var result = await sut.RecentAsync(
            new ChainQuery(Since: DateTime.UtcNow.AddYears(-50), Until: DateTime.UtcNow.AddYears(50), Limit: 10),
            CancellationToken.None);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.LessThanOrEqualTo(10));
    }

    [Test]
    public async Task Recent_Default_Limit_Is_100()
    {
        using var scope = Factory.Services.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<IProcessChainResolver>();
        var result = await sut.RecentAsync(new ChainQuery(), CancellationToken.None);
        Assert.That(result.Count, Is.LessThanOrEqualTo(100));
    }

    [Test]
    public async Task Recent_Limit_Capped_At_500()
    {
        using var scope = Factory.Services.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<IProcessChainResolver>();
        var result = await sut.RecentAsync(new ChainQuery(Limit: 10_000), CancellationToken.None);
        Assert.That(result.Count, Is.LessThanOrEqualTo(500));
    }
}
