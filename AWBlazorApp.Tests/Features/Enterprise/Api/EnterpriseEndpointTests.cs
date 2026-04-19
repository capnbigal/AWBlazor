using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Features.Enterprise.Assets.Domain; using AWBlazorApp.Features.Enterprise.CostCenters.Domain; using AWBlazorApp.Features.Enterprise.OrgUnits.Domain; using AWBlazorApp.Features.Enterprise.Organizations.Domain; using AWBlazorApp.Features.Enterprise.ProductLines.Domain; using AWBlazorApp.Features.Enterprise.Stations.Domain; 
using AWBlazorApp.Features.Enterprise.Assets.Dtos; using AWBlazorApp.Features.Enterprise.CostCenters.Dtos; using AWBlazorApp.Features.Enterprise.OrgUnits.Dtos; using AWBlazorApp.Features.Enterprise.Organizations.Dtos; using AWBlazorApp.Features.Enterprise.ProductLines.Dtos; using AWBlazorApp.Features.Enterprise.Stations.Dtos;
using AWBlazorApp.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using AWBlazorApp.Tests.Infrastructure.Testing;

namespace AWBlazorApp.Tests.Features.Enterprise.Api;

// IdResponse.Id is typed as `object` in the production code, so when System.Text.Json
// deserializes it on the test client side the value arrives as a JsonElement. This helper
// unwraps it back into an int so the tests can treat it as a number.
file static class IdResponseExtensions
{
    public static int AsInt(this IdResponse r) => r.Id switch
    {
        int i => i,
        long l => (int)l,
        JsonElement je => je.GetInt32(),
        IConvertible c => Convert.ToInt32(c),
        _ => throw new InvalidOperationException($"Unexpected IdResponse.Id type: {r.Id?.GetType()}"),
    };
}

/// <summary>
/// Integration tests for the Phase A Enterprise endpoints (<c>/api/organizations</c>,
/// <c>/api/org-units</c>, <c>/api/stations</c>, <c>/api/assets</c>, <c>/api/cost-centers</c>,
/// <c>/api/product-lines</c>). Runs against the real SQL Server dev database via
/// <see cref="IntegrationTestFixtureBase"/>.
/// </summary>
public class EnterpriseEndpointTests : IntegrationTestFixtureBase
{
    private static readonly string[] EnterpriseGroups =
    [
        "/api/organizations",
        "/api/org-units",
        "/api/stations",
        "/api/assets",
        "/api/cost-centers",
        "/api/product-lines",
    ];

    private static IEnumerable<string> Groups => EnterpriseGroups;

    [TestCaseSource(nameof(Groups))]
    public async Task EnterpriseEndpoint_Without_Auth_Returns_Unauthorized(string endpoint)
    {
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var response = await client.GetAsync(endpoint);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized).Or.EqualTo(HttpStatusCode.Redirect).Or.EqualTo(HttpStatusCode.Found),
            $"Expected 401/redirect for {endpoint}, got {(int)response.StatusCode}");
    }

    [TestCaseSource(nameof(Groups))]
    public async Task EnterpriseEndpoint_With_ApiKey_Returns_Success(string endpoint)
    {
        var apiKey = await EnsureAdminApiKeyAsync();
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        var response = await client.GetAsync(endpoint + "?take=1");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"Expected 200 for {endpoint}, got {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    }

    [Test]
    public async Task PrimaryOrganization_Is_Seeded()
    {
        await using var db = await GetDbContextAsync();
        var primary = await db.Organizations.AsNoTracking().SingleOrDefaultAsync(o => o.IsPrimary);
        Assert.That(primary, Is.Not.Null, "DatabaseInitializer should seed a primary organization on first boot.");
        Assert.That(primary!.Code, Is.Not.Empty);
    }

    [Test]
    public async Task Organization_List_Returns_PagedResult_With_Primary()
    {
        var apiKey = await EnsureAdminApiKeyAsync();
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        var page = await client.GetFromJsonAsync<PagedResult<OrganizationDto>>("/api/organizations?take=50");
        Assert.That(page, Is.Not.Null);
        Assert.That(page!.Results.Count, Is.GreaterThan(0));
        Assert.That(page.Results.Any(o => o.IsPrimary), Is.True, "Primary organization should be in the list.");
    }

    [Test]
    public async Task OrgUnit_Create_Resolves_Path_And_Depth_From_Parent()
    {
        var apiKey = await EnsureAdminApiKeyAsync();
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        await using var db = await GetDbContextAsync();
        var primary = await db.Organizations.AsNoTracking().SingleOrDefaultAsync(o => o.IsPrimary);
        Assert.That(primary, Is.Not.Null);

        var plantCode = $"PLANT-{Guid.NewGuid():N}".Substring(0, 12);
        var createPlant = await client.PostAsJsonAsync("/api/org-units", new CreateOrgUnitRequest
        {
            OrganizationId = primary!.Id,
            Kind = OrgUnitKind.Plant,
            Code = plantCode,
            Name = "Test plant",
        });
        Assert.That(createPlant.StatusCode, Is.EqualTo(HttpStatusCode.Created), await createPlant.Content.ReadAsStringAsync());
        var plantId = (await createPlant.Content.ReadFromJsonAsync<IdResponse>())!.AsInt();

        var areaCode = "AREA-01";
        var createArea = await client.PostAsJsonAsync("/api/org-units", new CreateOrgUnitRequest
        {
            OrganizationId = primary.Id,
            ParentOrgUnitId = Convert.ToInt32(plantId),
            Kind = OrgUnitKind.Area,
            Code = areaCode,
            Name = "Test area",
        });
        Assert.That(createArea.StatusCode, Is.EqualTo(HttpStatusCode.Created), await createArea.Content.ReadAsStringAsync());
        var areaId = (await createArea.Content.ReadFromJsonAsync<IdResponse>())!.AsInt();

        try
        {
            await using var reread = await GetDbContextAsync();
            var plant = await reread.OrgUnits.AsNoTracking().FirstAsync(u => u.Id == plantId);
            var area  = await reread.OrgUnits.AsNoTracking().FirstAsync(u => u.Id == areaId);

            Assert.That(plant.Path, Is.EqualTo(plantCode.ToUpperInvariant()));
            Assert.That(plant.Depth, Is.EqualTo(0));
            Assert.That(area.Path, Is.EqualTo($"{plantCode.ToUpperInvariant()}/{areaCode}"));
            Assert.That(area.Depth, Is.EqualTo(1));
            Assert.That(area.OrganizationId, Is.EqualTo(primary.Id), "Child should inherit the parent's organization.");
        }
        finally
        {
            // Cleanup — delete area first (has no children), then plant.
            await client.DeleteAsync($"/api/org-units/{areaId}");
            await client.DeleteAsync($"/api/org-units/{plantId}");
        }
    }

    [Test]
    public async Task OrgUnit_Delete_With_Children_Returns_ValidationProblem()
    {
        var apiKey = await EnsureAdminApiKeyAsync();
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        await using var db = await GetDbContextAsync();
        var primary = await db.Organizations.AsNoTracking().SingleAsync(o => o.IsPrimary);

        var parentCode = $"PARENT-{Guid.NewGuid():N}".Substring(0, 12);
        var parentId = ((await (await client.PostAsJsonAsync("/api/org-units", new CreateOrgUnitRequest
        {
            OrganizationId = primary.Id, Kind = OrgUnitKind.Plant, Code = parentCode, Name = "Parent",
        })).Content.ReadFromJsonAsync<IdResponse>())!).AsInt();

        var childId = ((await (await client.PostAsJsonAsync("/api/org-units", new CreateOrgUnitRequest
        {
            OrganizationId = primary.Id, ParentOrgUnitId = parentId, Kind = OrgUnitKind.Area, Code = "CHILD", Name = "Child",
        })).Content.ReadFromJsonAsync<IdResponse>())!).AsInt();

        try
        {
            var delete = await client.DeleteAsync($"/api/org-units/{parentId}");
            Assert.That(delete.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
                "Deleting an OrgUnit with children should 400 — not cascade or succeed.");
        }
        finally
        {
            await client.DeleteAsync($"/api/org-units/{childId}");
            await client.DeleteAsync($"/api/org-units/{parentId}");
        }
    }

    private static string? _adminApiKey;

    /// <summary>Mirror of ApiSmokeTests' helper — creates (and caches) a hashed Admin API key.</summary>
    private async Task<string> EnsureAdminApiKeyAsync()
    {
        if (_adminApiKey is not null) return _adminApiKey;

        await using var db = await GetDbContextAsync();
        var adminRoleId = await db.Roles.Where(r => r.Name == AppRoles.Admin).Select(r => r.Id).FirstAsync();
        var adminUserId = await db.UserRoles.Where(ur => ur.RoleId == adminRoleId).Select(ur => ur.UserId).FirstAsync();

        var rawKey = "ek_enterprise_" + Guid.NewGuid().ToString("N");
        var hashed = AWBlazorApp.Infrastructure.Authentication.ApiKeyHasher.Hash(rawKey);
        db.ApiKeys.Add(new ApiKey
        {
            Name = "enterprise-tests",
            Key = hashed,
            UserId = adminUserId,
            CreatedDate = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
        _adminApiKey = rawKey;
        return rawKey;
    }
}
