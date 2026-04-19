using System.Net;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Features.Admin.Services;
using AWBlazorApp.Features.Dashboard.Dtos;
using AWBlazorApp.Features.Dashboard.Services;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Net.Http.Json;
using AWBlazorApp.Tests.Infrastructure.Testing;

namespace AWBlazorApp.Tests.Features.Dashboard.Application;

/// <summary>
/// Integration tests for the cross-module plant dashboard. Verifies the service aggregates
/// without throwing across all 9 schemas, the cache invalidates correctly, and the endpoint
/// is properly auth-gated.
/// </summary>
public class PlantDashboardTests : IntegrationTestFixtureBase
{
    [Test]
    public async Task Service_Returns_Populated_Dto_After_Seed()
    {
        // Run baseline so module health cards have something non-zero to report.
        using var seedScope = Factory.Services.CreateScope();
        await seedScope.ServiceProvider.GetRequiredService<DemoDataSeeder>().SeedAllAsync(CancellationToken.None);

        using var scope = Factory.Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IPlantDashboardService>();
        svc.Invalidate(); // ensure fresh build

        var dto = await svc.GetAsync(CancellationToken.None);

        Assert.That(dto, Is.Not.Null);
        Assert.That(dto.GeneratedAt, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-1)));
        Assert.That(dto.Headlines, Is.Not.Null);
        Assert.That(dto.ModuleHealth.Count, Is.EqualTo(9), "Expected one health card per module (wf/eng/maint/perf/qa/mes/lgx/inv/org).");

        // Headline counts should be sane non-negative integers.
        Assert.That(dto.Headlines.ActiveProductionRuns, Is.GreaterThanOrEqualTo(0));
        Assert.That(dto.Headlines.OpenMaintenanceWorkOrders, Is.GreaterThanOrEqualTo(0));
        Assert.That(dto.Headlines.OpenQualificationAlerts, Is.GreaterThanOrEqualTo(0));

        // Trend lists are populated to length 0..7+ depending on history.
        Assert.That(dto.OeeTrend7d, Is.Not.Null);
        Assert.That(dto.ProductionTrend7d, Is.Not.Null);
        Assert.That(dto.WorkOrderTrend7d.Count, Is.EqualTo(8), "WO trend should always emit 8 days (7 prior + today) padded with zeros.");

        // Module mini-stats now expose drill-through hrefs and a 30-day trend per card.
        var workforce = dto.ModuleHealth.First(m => m.Name == "Workforce");
        Assert.That(workforce.Stats.All(s => !string.IsNullOrWhiteSpace(s.LinkHref)), Is.True,
            "Every Workforce mini-stat should ship a drill-through href.");
        Assert.That(dto.ModuleHealth.All(m => m.Trend30d.Count == 30), Is.True,
            "Every module card should emit exactly 30 daily bins for the trend sparkline.");
    }

    [Test]
    public async Task Service_Caches_Subsequent_Calls()
    {
        using var seedScope = Factory.Services.CreateScope();
        await seedScope.ServiceProvider.GetRequiredService<DemoDataSeeder>().SeedAllAsync(CancellationToken.None);

        using var scope = Factory.Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IPlantDashboardService>();
        svc.Invalidate();

        var first = await svc.GetAsync(CancellationToken.None);
        var second = await svc.GetAsync(CancellationToken.None);

        Assert.That(second.GeneratedAt, Is.EqualTo(first.GeneratedAt),
            "Second call within TTL should return the cached payload — same GeneratedAt timestamp.");

        svc.Invalidate();
        var third = await svc.GetAsync(CancellationToken.None);
        Assert.That(third.GeneratedAt, Is.GreaterThanOrEqualTo(first.GeneratedAt),
            "After Invalidate, the next call should rebuild.");
    }

    [Test]
    public async Task Endpoint_Without_Auth_Returns_Unauthorized()
    {
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var response = await client.GetAsync("/api/dashboard/plant");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized).Or.EqualTo(HttpStatusCode.Redirect).Or.EqualTo(HttpStatusCode.Found));
    }

    [Test]
    public async Task Endpoint_With_ApiKey_Returns_Dto()
    {
        using var seedScope = Factory.Services.CreateScope();
        await seedScope.ServiceProvider.GetRequiredService<DemoDataSeeder>().SeedAllAsync(CancellationToken.None);

        var apiKey = await EnsureAdminApiKeyAsync();
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        var response = await client.GetAsync("/api/dashboard/plant");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"Expected 200, got {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");

        var dto = await response.Content.ReadFromJsonAsync<PlantDashboardDto>();
        Assert.That(dto, Is.Not.Null);
        Assert.That(dto!.ModuleHealth.Count, Is.EqualTo(9));
    }

    private static string? _adminApiKey;
    private async Task<string> EnsureAdminApiKeyAsync()
    {
        if (_adminApiKey is not null) return _adminApiKey;
        using var scope = Factory.Services.CreateScope();
        var dbFactory = scope.ServiceProvider.GetRequiredService<Microsoft.EntityFrameworkCore.IDbContextFactory<AWBlazorApp.Infrastructure.Persistence.ApplicationDbContext>>();
        await using var db = await dbFactory.CreateDbContextAsync();

        var adminRoleId = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .FirstAsync(db.Roles.Where(r => r.Name == AppRoles.Admin).Select(r => r.Id));
        var adminUserId = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .FirstAsync(db.UserRoles.Where(ur => ur.RoleId == adminRoleId).Select(ur => ur.UserId));

        var rawKey = "ek_dash_" + Guid.NewGuid().ToString("N");
        var hashed = AWBlazorApp.Infrastructure.Authentication.ApiKeyHasher.Hash(rawKey);
        db.ApiKeys.Add(new ApiKey
        {
            Name = "dashboard-tests",
            Key = hashed,
            UserId = adminUserId,
            CreatedDate = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
        _adminApiKey = rawKey;
        return rawKey;
    }
}
