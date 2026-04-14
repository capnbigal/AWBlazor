using System.Net;
using AWBlazorApp.Data;
using AWBlazorApp.Data.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests;

/// <summary>
/// Parameterized smoke tests for every AdventureWorks REST endpoint group. These tests verify
/// two things on each /api/aw/* group:
///   1. Anonymous GET returns 401 (auth is correctly required)
///   2. Authenticated GET via API key returns 200 and a non-empty body
///
/// Mutating endpoints (POST/PATCH/DELETE) are not exercised here — they're covered by the
/// per-entity dialog tests via FormPostHelper.
///
/// This class shares the WebApplicationFactory startup with IntegrationTest via the
/// IntegrationTestFixtureBase pattern below; we don't recreate the factory per class because
/// host startup against AdventureWorks2022_dev takes 5-10 seconds.
/// </summary>
public class ApiSmokeTests : IntegrationTestFixtureBase
{
    /// <summary>All registered /api/aw/* group prefixes. Add new entries when new endpoint
    /// groups are mapped in MapApiEndpoints / Endpoints/AdventureWorks/EndpointMappingExtensions.cs.</summary>
    private static readonly string[] AwEndpointGroups =
    [
        "/api/aw/address-types",
        "/api/aw/addresses",
        "/api/aw/bill-of-materials",
        "/api/aw/business-entities",
        "/api/aw/business-entity-addresses",
        "/api/aw/business-entity-contacts",
        "/api/aw/contact-types",
        "/api/aw/country-region-currencies",
        "/api/aw/country-regions",
        "/api/aw/credit-cards",
        "/api/aw/cultures",
        "/api/aw/currencies",
        "/api/aw/currency-rates",
        "/api/aw/customers",
        "/api/aw/departments",
        "/api/aw/documents",
        "/api/aw/email-addresses",
        "/api/aw/employee-department-histories",
        "/api/aw/employee-pay-histories",
        "/api/aw/employees",
        "/api/aw/illustrations",
        "/api/aw/job-candidates",
        "/api/aw/locations",
        "/api/aw/person-credit-cards",
        "/api/aw/person-phones",
        "/api/aw/persons",
        "/api/aw/phone-number-types",
        "/api/aw/product-categories",
        "/api/aw/product-cost-histories",
        "/api/aw/product-descriptions",
        "/api/aw/product-documents",
        "/api/aw/product-inventories",
        "/api/aw/product-list-price-histories",
        "/api/aw/product-model-illustrations",
        "/api/aw/product-model-product-description-cultures",
        "/api/aw/product-models",
        "/api/aw/product-photos",
        "/api/aw/product-product-photos",
        "/api/aw/product-reviews",
        "/api/aw/product-subcategories",
        "/api/aw/product-vendors",
        "/api/aw/products",
        "/api/aw/purchase-order-details",
        "/api/aw/purchase-order-headers",
        "/api/aw/sales-order-details",
        "/api/aw/sales-order-header-sales-reasons",
        "/api/aw/sales-order-headers",
        "/api/aw/sales-person-quota-histories",
        "/api/aw/sales-persons",
        "/api/aw/sales-reasons",
        "/api/aw/sales-tax-rates",
        "/api/aw/sales-territories",
        "/api/aw/sales-territory-histories",
        "/api/aw/scrap-reasons",
        "/api/aw/shifts",
        "/api/aw/ship-methods",
        "/api/aw/shopping-cart-items",
        "/api/aw/special-offer-products",
        "/api/aw/special-offers",
        "/api/aw/state-provinces",
        "/api/aw/stores",
        "/api/aw/transaction-histories",
        "/api/aw/transaction-history-archives",
        "/api/aw/unit-measures",
        "/api/aw/vendors",
        "/api/aw/work-order-routings",
        "/api/aw/work-orders",
    ];

    private static IEnumerable<string> EndpointGroupsSource => AwEndpointGroups;

    [TestCaseSource(nameof(EndpointGroupsSource))]
    public async Task AwEndpoint_Without_Auth_Returns_Unauthorized(string endpoint)
    {
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var response = await client.GetAsync(endpoint);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized).Or.EqualTo(HttpStatusCode.Redirect).Or.EqualTo(HttpStatusCode.Found),
            $"Expected 401 or redirect for {endpoint}, got {(int)response.StatusCode}");
    }

    [TestCaseSource(nameof(EndpointGroupsSource))]
    public async Task AwEndpoint_With_ApiKey_Returns_Success(string endpoint)
    {
        var apiKey = await EnsureSmokeTestApiKeyAsync();
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        var response = await client.GetAsync(endpoint + "?take=1");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"Expected 200 for {endpoint}, got {(int)response.StatusCode} {await response.Content.ReadAsStringAsync()}");

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Is.Not.Null.And.Not.Empty,
            $"Expected non-empty JSON response from {endpoint}");
        Assert.That(body, Does.StartWith("{"), $"Expected JSON object from {endpoint}, got: {body[..Math.Min(200, body.Length)]}");
    }

    private static string? _smokeTestApiKey;

    /// <summary>Creates (or retrieves) a hashed Admin API key for smoke tests. Caches across calls.</summary>
    private async Task<string> EnsureSmokeTestApiKeyAsync()
    {
        if (_smokeTestApiKey is not null) return _smokeTestApiKey;

        await using var db = await GetDbContextAsync();

        // Find an Admin user (the seed data should provide one).
        var adminRoleId = await db.Roles.Where(r => r.Name == AppRoles.Admin).Select(r => r.Id).FirstAsync();
        var adminUserId = await db.UserRoles.Where(ur => ur.RoleId == adminRoleId).Select(ur => ur.UserId).FirstAsync();

        // Generate a unique key, store its hash so the auth handler can verify it.
        var rawKey = "ek_smoke_" + Guid.NewGuid().ToString("N");
        var hashed = AWBlazorApp.Authentication.ApiKeyHasher.Hash(rawKey);

        db.ApiKeys.Add(new ApiKey
        {
            Name = "smoke-tests",
            Key = hashed,
            UserId = adminUserId,
            CreatedDate = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        _smokeTestApiKey = rawKey;
        return rawKey;
    }
}

/// <summary>
/// Shared fixture base — provides Factory and GetDbContextAsync without each test class
/// rebuilding the WebApplicationFactory (host startup is slow against the real SQL Server).
/// Test classes inherit this; NUnit reuses the OneTimeSetUp across [TestCaseSource] enumerations.
/// </summary>
public abstract class IntegrationTestFixtureBase
{
    private static Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>? _sharedFactory;
    private static readonly object _factoryLock = new();

    protected Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> Factory
    {
        get
        {
            if (_sharedFactory is not null) return _sharedFactory;
            lock (_factoryLock)
            {
                _sharedFactory ??= new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>()
                    .WithWebHostBuilder(builder =>
                    {
                        builder.UseEnvironment("Development");
                        builder.ConfigureAppConfiguration((_, config) =>
                        {
                            config.AddInMemoryCollection(new Dictionary<string, string?>
                            {
                                ["Features:Hangfire"] = "false",
                                ["RequestLogs:Enabled"] = "false",
                                ["Features:RateLimiting"] = "false",
                            });
                        });
                    });
                return _sharedFactory;
            }
        }
    }

    protected async Task<ApplicationDbContext> GetDbContextAsync()
    {
        var scope = Factory.Services.CreateScope();
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        return await dbFactory.CreateDbContextAsync();
    }
}
