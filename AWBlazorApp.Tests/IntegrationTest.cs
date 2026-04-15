using System.Net;
using AWBlazorApp.Features.ToolSlots.Models;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Features.Identity.Domain;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Web;
using AWBlazorApp.Data;
using AWBlazorApp.Shared.Domain;
using AWBlazorApp.Shared.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests;

/// <summary>
/// Smoke-tests the Blazor host. The Blazor host runs in its registered Development configuration,
/// which means the EF Core <see cref="IDbContextFactory{TContext}"/> points at SQL Server
/// <c>ELITE / AdventureWorks2022_dev</c> (per <c>appsettings.Development.json</c>). Tests only
/// override the Hangfire + Serilog SQL sink feature flags via in-memory config so a test run
/// doesn't spin up background workers or spam the dev <c>RequestLogs</c> table — everything else,
/// including EF, uses the production wiring against the real SQL Server instance.
/// </summary>
public class IntegrationTest
{
    private string tempContentRoot = null!;
    private WebApplicationFactory<Program> factory = null!;
    private static readonly TestEmailSender testEmailSender = new();

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        tempContentRoot = Path.Combine(Path.GetTempPath(), "AWBlazorAppTests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempContentRoot);

        factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.UseContentRoot(tempContentRoot);

                // Keep the real SQL Server DbContextFactory (it reads from the Development
                // connection string = AdventureWorks2022_dev on ELITE). Only disable the
                // Hangfire background server and the Serilog MSSqlServer sink: tests shouldn't
                // spin up worker threads or spam the dev RequestLogs table.
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Features:Hangfire"] = "false",
                        ["RequestLogs:Enabled"] = "false",
                        // Disable rate limiting in the shared test factory — the auth limiter
                        // (5 req/min) would otherwise leak between unrelated tests. The rate
                        // limiting test creates its own factory with this flag flipped on.
                        ["Features:RateLimiting"] = "false",
                    });
                });

                builder.ConfigureTestServices(services =>
                {
                    // Replace the real email sender with a test double that captures sent emails
                    // so tests can inspect confirmation links, password reset codes, etc.
                    services.AddSingleton<IEmailSender<ApplicationUser>>(testEmailSender);
                });
            });

        // Force host construction so DatabaseInitializer.InitializeAsync runs (migrations,
        // seed, reconcile). Creating a client is the most reliable way to ensure exceptions
        // during host build surface here rather than getting swallowed when individual tests
        // later try to use the factory.
        try
        {
            using var _ = factory.CreateClient();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "WebApplicationFactory failed to start the host against AdventureWorks2022_dev on " +
                "ELITE. Confirm the database is reachable and the current user has access. " +
                "See InnerException.", ex);
        }

        // dbo.ToolSlotConfigurations is .ExcludeFromMigrations() because in production the DBA
        // owns that table in AdventureWorks2022. In AdventureWorks2022_dev it may or may not
        // exist yet — create a minimal version if it's missing so tests that exercise the
        // /api/tool-slots CRUD endpoints have a real table to write to. Column names match the
        // [Column(...)] attributes on ToolSlotConfiguration.
        await using var db = await GetDbContextAsync();
        await db.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[ToolSlotConfigurations]', 'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[ToolSlotConfigurations] (
                    [CID]         INT            IDENTITY(1,1) NOT NULL CONSTRAINT [PK_ToolSlotConfigurations] PRIMARY KEY,
                    [FAMILY]      NVARCHAR(255)  NULL,
                    [MT_CODE]     NVARCHAR(255)  NULL,
                    [DESTINATION] NVARCHAR(255)  NULL,
                    [FCL1]        NVARCHAR(255)  NULL,
                    [FCL2]        NVARCHAR(255)  NULL,
                    [FCR1]        NVARCHAR(255)  NULL,
                    [FFL1]        NVARCHAR(255)  NULL,
                    [FFL2]        NVARCHAR(255)  NULL,
                    [FFR1]        NVARCHAR(255)  NULL,
                    [FFR2]        NVARCHAR(255)  NULL,
                    [FFR3]        NVARCHAR(255)  NULL,
                    [FFR4]        NVARCHAR(255)  NULL,
                    [RCL1]        NVARCHAR(255)  NULL,
                    [RCR1]        NVARCHAR(255)  NULL,
                    [RCR2]        NVARCHAR(255)  NULL,
                    [RFL1]        NVARCHAR(255)  NULL,
                    [RFR1]        NVARCHAR(255)  NULL,
                    [RFR2]        NVARCHAR(255)  NULL,
                    [IsActive]    BIT            NOT NULL CONSTRAINT [DF_ToolSlotConfigurations_IsActive] DEFAULT (0)
                );
            END
            """);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        factory.Dispose();
        try
        {
            if (Directory.Exists(tempContentRoot))
                Directory.Delete(tempContentRoot, recursive: true);
        }
        catch
        {
            // Best-effort cleanup; ignore file-locking races on Windows.
        }
    }

    [Test]
    public async Task Login_Page_Returns_Success()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/Account/Login");
        Assert.That(response.IsSuccessStatusCode, Is.True);
    }

    [Test]
    public async Task Hello_Endpoint_Returns_Greeting()
    {
        var client = factory.CreateClient();
        var response = await client.GetFromJsonAsync<HelloResponse>("/api/hello/World");

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Result, Is.EqualTo("Hello, World!"));
    }

    [Test]
    public async Task Forecasts_Endpoint_Without_Auth_Returns_Unauthorized()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var response = await client.GetAsync("/api/forecasts");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Swagger_Json_Is_Served_In_Development()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.That(response.IsSuccessStatusCode, Is.True);

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain("/api/forecasts"));
        Assert.That(body, Does.Contain("/api/tool-slots"));
    }

    [Test]
    public async Task Swagger_UI_Html_Is_Served()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/swagger/index.html");
        Assert.That(response.IsSuccessStatusCode, Is.True,
            $"Expected /swagger/index.html to serve the Swagger UI, got {(int)response.StatusCode}.");

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain("swagger-ui").IgnoreCase,
            "Expected the Swagger UI bundle markers in the response body.");
    }

    // Authorized Blazor pages should redirect anonymous users to /Account/Login.
    [TestCase("/forecasts")]
    [TestCase("/tool-slots")]
    [TestCase("/tool-slots/history")]
    [TestCase("/tool-slots/history/1")]
    [TestCase("/admin")]
    [TestCase("/admin/users")]
    // AdventureWorks reference-data pages
    [TestCase("/aw/address-types")]
    [TestCase("/aw/address-types/history")]
    [TestCase("/aw/address-types/history/1")]
    [TestCase("/aw/contact-types")]
    [TestCase("/aw/contact-types/history")]
    [TestCase("/aw/country-regions")]
    [TestCase("/aw/country-regions/history")]
    [TestCase("/aw/country-regions/history/US")]
    [TestCase("/aw/phone-number-types")]
    [TestCase("/aw/phone-number-types/history")]
    [TestCase("/aw/cultures")]
    [TestCase("/aw/cultures/history")]
    [TestCase("/aw/cultures/history/en")]
    [TestCase("/aw/product-categories")]
    [TestCase("/aw/product-categories/history")]
    [TestCase("/aw/scrap-reasons")]
    [TestCase("/aw/scrap-reasons/history")]
    [TestCase("/aw/unit-measures")]
    [TestCase("/aw/unit-measures/history")]
    [TestCase("/aw/unit-measures/history/EA")]
    [TestCase("/aw/currencies")]
    [TestCase("/aw/currencies/history")]
    [TestCase("/aw/currencies/history/USD")]
    [TestCase("/aw/sales-reasons")]
    [TestCase("/aw/sales-reasons/history")]
    [TestCase("/aw/departments")]
    [TestCase("/aw/departments/history")]
    [TestCase("/aw/shifts")]
    [TestCase("/aw/shifts/history")]
    // Batch 2 AdventureWorks reference-data pages
    [TestCase("/aw/locations")]
    [TestCase("/aw/locations/history")]
    [TestCase("/aw/locations/history/1")]
    [TestCase("/aw/ship-methods")]
    [TestCase("/aw/ship-methods/history")]
    [TestCase("/aw/ship-methods/history/1")]
    [TestCase("/aw/product-subcategories")]
    [TestCase("/aw/product-subcategories/history")]
    [TestCase("/aw/product-subcategories/history/1")]
    [TestCase("/aw/product-descriptions")]
    [TestCase("/aw/product-descriptions/history")]
    [TestCase("/aw/special-offers")]
    [TestCase("/aw/special-offers/history")]
    [TestCase("/aw/special-offers/history/1")]
    [TestCase("/aw/state-provinces")]
    [TestCase("/aw/state-provinces/history")]
    [TestCase("/aw/state-provinces/history/1")]
    [TestCase("/aw/sales-territories")]
    [TestCase("/aw/sales-territories/history")]
    [TestCase("/aw/sales-territories/history/1")]
    [TestCase("/aw/sales-tax-rates")]
    [TestCase("/aw/sales-tax-rates/history")]
    [TestCase("/aw/sales-tax-rates/history/1")]
    [TestCase("/aw/shopping-cart-items")]
    [TestCase("/aw/shopping-cart-items/history")]
    [TestCase("/aw/shopping-cart-items/history/1")]
    // Batch 3 — business entities + composite-key tables
    [TestCase("/aw/customers")]
    [TestCase("/aw/customers/history")]
    [TestCase("/aw/customers/history/1")]
    [TestCase("/aw/sales-persons")]
    [TestCase("/aw/sales-persons/history")]
    [TestCase("/aw/sales-persons/history/1")]
    [TestCase("/aw/work-orders")]
    [TestCase("/aw/work-orders/history")]
    [TestCase("/aw/work-orders/history/1")]
    [TestCase("/aw/bill-of-materials")]
    [TestCase("/aw/bill-of-materials/history")]
    [TestCase("/aw/bill-of-materials/history/1")]
    [TestCase("/aw/currency-rates")]
    [TestCase("/aw/currency-rates/history")]
    [TestCase("/aw/currency-rates/history/1")]
    [TestCase("/aw/sales-person-quota-histories")]
    [TestCase("/aw/sales-person-quota-histories/history")]
    [TestCase("/aw/sales-person-quota-histories/history?businessEntityId=1")]
    [TestCase("/aw/sales-order-header-sales-reasons")]
    [TestCase("/aw/sales-order-header-sales-reasons/history")]
    [TestCase("/aw/sales-order-header-sales-reasons/history?salesOrderId=1&salesReasonId=1")]
    [TestCase("/aw/product-cost-histories")]
    [TestCase("/aw/product-cost-histories/history")]
    [TestCase("/aw/product-cost-histories/history?productId=1")]
    [TestCase("/aw/country-region-currencies")]
    [TestCase("/aw/country-region-currencies/history")]
    [TestCase("/aw/country-region-currencies/history?countryRegionCode=US&currencyCode=USD")]
    [TestCase("/aw/employee-department-histories")]
    [TestCase("/aw/employee-department-histories/history")]
    [TestCase("/aw/employee-department-histories/history?businessEntityId=1&departmentId=1&shiftId=1&startDate=2020-01-01")]
    // Batch 4 — Person hierarchy
    [TestCase("/aw/addresses")]
    [TestCase("/aw/addresses/history")]
    [TestCase("/aw/addresses/history/1")]
    [TestCase("/aw/business-entities")]
    [TestCase("/aw/business-entities/history")]
    [TestCase("/aw/business-entities/history/1")]
    [TestCase("/aw/persons")]
    [TestCase("/aw/persons/history")]
    [TestCase("/aw/persons/history/1")]
    [TestCase("/aw/email-addresses")]
    [TestCase("/aw/email-addresses/history")]
    [TestCase("/aw/email-addresses/history?businessEntityId=1&emailAddressId=1")]
    [TestCase("/aw/person-phones")]
    [TestCase("/aw/person-phones/history")]
    [TestCase("/aw/person-phones/history?businessEntityId=1&phoneNumber=555&phoneNumberTypeId=1")]
    [TestCase("/aw/business-entity-addresses")]
    [TestCase("/aw/business-entity-addresses/history")]
    [TestCase("/aw/business-entity-addresses/history?businessEntityId=1&addressId=1&addressTypeId=1")]
    [TestCase("/aw/business-entity-contacts")]
    [TestCase("/aw/business-entity-contacts/history")]
    [TestCase("/aw/business-entity-contacts/history?businessEntityId=1&personId=1&contactTypeId=1")]
    // Batch 5 — Production / Product hierarchy
    [TestCase("/aw/products")]
    [TestCase("/aw/products/history")]
    [TestCase("/aw/products/history/1")]
    [TestCase("/aw/product-models")]
    [TestCase("/aw/product-models/history")]
    [TestCase("/aw/product-models/history/1")]
    [TestCase("/aw/illustrations")]
    [TestCase("/aw/illustrations/history")]
    [TestCase("/aw/illustrations/history/1")]
    [TestCase("/aw/product-photos")]
    [TestCase("/aw/product-photos/history")]
    [TestCase("/aw/product-photos/history/1")]
    [TestCase("/aw/product-reviews")]
    [TestCase("/aw/product-reviews/history")]
    [TestCase("/aw/product-reviews/history/1")]
    [TestCase("/aw/product-inventories")]
    [TestCase("/aw/product-inventories/history")]
    [TestCase("/aw/product-inventories/history?productId=1&locationId=1")]
    [TestCase("/aw/product-list-price-histories")]
    [TestCase("/aw/product-list-price-histories/history")]
    [TestCase("/aw/product-list-price-histories/history?productId=1&startDate=2020-01-01")]
    [TestCase("/aw/product-product-photos")]
    [TestCase("/aw/product-product-photos/history")]
    [TestCase("/aw/product-product-photos/history?productId=1&productPhotoId=1")]
    // Batch 6 — Production junction + transaction tables
    [TestCase("/aw/product-model-illustrations")]
    [TestCase("/aw/product-model-illustrations/history")]
    [TestCase("/aw/product-model-illustrations/history?productModelId=1&illustrationId=1")]
    [TestCase("/aw/product-model-product-description-cultures")]
    [TestCase("/aw/product-model-product-description-cultures/history")]
    [TestCase("/aw/product-model-product-description-cultures/history?productModelId=1&productDescriptionId=1&cultureId=en")]
    [TestCase("/aw/work-order-routings")]
    [TestCase("/aw/work-order-routings/history")]
    [TestCase("/aw/work-order-routings/history?workOrderId=1&productId=1&operationSequence=1")]
    [TestCase("/aw/transaction-histories")]
    [TestCase("/aw/transaction-histories/history")]
    [TestCase("/aw/transaction-histories/history/1")]
    [TestCase("/aw/transaction-history-archives")]
    [TestCase("/aw/transaction-history-archives/history")]
    [TestCase("/aw/transaction-history-archives/history/1")]
    // Batch 7 — Sales: CreditCard, PersonCreditCard, SalesOrderHeader/Detail, SalesTerritoryHistory, SpecialOfferProduct, Store
    [TestCase("/aw/credit-cards")]
    [TestCase("/aw/credit-cards/history")]
    [TestCase("/aw/credit-cards/history/1")]
    [TestCase("/aw/person-credit-cards")]
    [TestCase("/aw/person-credit-cards/history")]
    [TestCase("/aw/person-credit-cards/history?businessEntityId=1&creditCardId=1")]
    [TestCase("/aw/sales-order-headers")]
    [TestCase("/aw/sales-order-headers/history")]
    [TestCase("/aw/sales-order-headers/history/1")]
    [TestCase("/aw/sales-order-details")]
    [TestCase("/aw/sales-order-details/history")]
    [TestCase("/aw/sales-order-details/history?salesOrderId=1&salesOrderDetailId=1")]
    [TestCase("/aw/sales-territory-histories")]
    [TestCase("/aw/sales-territory-histories/history")]
    [TestCase("/aw/sales-territory-histories/history?businessEntityId=1&startDate=2020-01-01&territoryId=1")]
    [TestCase("/aw/special-offer-products")]
    [TestCase("/aw/special-offer-products/history")]
    [TestCase("/aw/special-offer-products/history?specialOfferId=1&productId=1")]
    [TestCase("/aw/stores")]
    [TestCase("/aw/stores/history")]
    [TestCase("/aw/stores/history/1")]
    // Batch 8 — Purchasing + HR entities
    [TestCase("/aw/vendors")]
    [TestCase("/aw/vendors/history")]
    [TestCase("/aw/vendors/history/1")]
    [TestCase("/aw/product-vendors")]
    [TestCase("/aw/product-vendors/history")]
    [TestCase("/aw/product-vendors/history?productId=1&businessEntityId=1")]
    [TestCase("/aw/purchase-order-headers")]
    [TestCase("/aw/purchase-order-headers/history")]
    [TestCase("/aw/purchase-order-headers/history/1")]
    [TestCase("/aw/purchase-order-details")]
    [TestCase("/aw/purchase-order-details/history")]
    [TestCase("/aw/purchase-order-details/history?purchaseOrderId=1&purchaseOrderDetailId=1")]
    [TestCase("/aw/employees")]
    [TestCase("/aw/employees/history")]
    [TestCase("/aw/employees/history/1")]
    [TestCase("/aw/employee-pay-histories")]
    [TestCase("/aw/employee-pay-histories/history")]
    [TestCase("/aw/employee-pay-histories/history?businessEntityId=1&rateChangeDate=2020-01-01")]
    [TestCase("/aw/job-candidates")]
    [TestCase("/aw/job-candidates/history")]
    [TestCase("/aw/job-candidates/history/1")]
    // Batch 9 — Production: Document + ProductDocument (hierarchyid PK)
    [TestCase("/aw/documents")]
    [TestCase("/aw/documents/history")]
    [TestCase("/aw/product-documents")]
    [TestCase("/aw/product-documents/history")]
    // Reports / Database explorer
    [TestCase("/reports")]
    public async Task Protected_Page_Redirects_Anonymous_To_Login(string path)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var response = await client.GetAsync(path);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect).Or.EqualTo(HttpStatusCode.Found));
        Assert.That(response.Headers.Location?.ToString(), Does.Contain("/Account/Login"));
    }

    [Test]
    public async Task ForgotPassword_Page_Returns_Success()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/Account/ForgotPassword");
        Assert.That(response.IsSuccessStatusCode, Is.True);
    }

    [Test]
    public async Task Register_Page_Returns_Success()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/Account/Register");
        Assert.That(response.IsSuccessStatusCode, Is.True);
    }

    // ── Health check tests ────────────────────────────────────────────────────────────────────

    [Test]
    public async Task Healthz_Liveness_Returns_Success()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/healthz");
        Assert.That(response.IsSuccessStatusCode, Is.True);
    }

    // ── Form-POST tests ──────────────────────────────────────────────────────────────────────
    // These exist to catch the kind of regressions we hit during Phase 4: MudBlazor inputs
    // not emitting the right `name` attribute, [SupplyParameterFromForm] models being null,
    // EditForm Model parameter null, etc. They drive the form the same way a browser would.

    [Test]
    public async Task Login_Form_Post_With_Valid_Credentials_Redirects()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var response = await FormPostHelper.PostFormAsync(client, "/Account/Login", "login",
            new Dictionary<string, string>
            {
                ["Input.Email"] = "admin@email.com",
                ["Input.Password"] = "p@55wOrd",
                ["Input.RememberMe"] = "false",
            });

        // SignInManager.PasswordSignInAsync writes the auth cookie and IdentityRedirectManager
        // redirects to ReturnUrl (defaults to "/").
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect).Or.EqualTo(HttpStatusCode.Found),
            $"Expected redirect, got {(int)response.StatusCode} {response.StatusCode}");
        Assert.That(response.Headers.Location?.ToString(), Does.Not.Contain("/Account/Login"),
            "Should redirect away from the login page on success.");
    }

    [Test]
    public async Task Login_Form_Post_With_Invalid_Credentials_Stays_On_Login_Page()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var response = await FormPostHelper.PostFormAsync(client, "/Account/Login", "login",
            new Dictionary<string, string>
            {
                ["Input.Email"] = "admin@email.com",
                ["Input.Password"] = "definitely-wrong",
                ["Input.RememberMe"] = "false",
            });

        Assert.That(response.IsSuccessStatusCode, Is.True,
            "Login page should re-render with an error message rather than redirect.");

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain("Invalid login attempt"));
    }

    [Test]
    public async Task ForgotPassword_Form_Post_Redirects_To_Confirmation()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var response = await FormPostHelper.PostFormAsync(client, "/Account/ForgotPassword", "forgot-password",
            new Dictionary<string, string>
            {
                ["Input.Email"] = "admin@email.com",
            });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect).Or.EqualTo(HttpStatusCode.Found));
        Assert.That(response.Headers.Location?.ToString(), Does.Contain("/Account/ForgotPasswordConfirmation"));
    }

    [Test]
    public async Task ResendEmailConfirmation_Form_Post_Returns_Confirmation_Message()
    {
        var client = factory.CreateClient();
        var response = await FormPostHelper.PostFormAsync(client, "/Account/ResendEmailConfirmation", "resend-confirmation",
            new Dictionary<string, string>
            {
                ["Input.Email"] = "definitely-not-a-real-user@example.com",
            });

        // The page should render (success or re-render with status message). Don't reveal
        // whether the email actually exists in the system.
        Assert.That(response.IsSuccessStatusCode, Is.True,
            $"Expected page to re-render, got {(int)response.StatusCode}");
    }

    [Test]
    public async Task Auth_Endpoint_Rate_Limiter_Returns_429_After_Threshold()
    {
        // Auth limiter is 5 req/min. Fire 10 rapid requests; at least one should hit 429.
        // Spin up a dedicated factory with rate limiting ENABLED (the shared test factory
        // disables it so it doesn't leak between unrelated tests).
        using var rateLimitFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(b =>
            {
                b.UseEnvironment("Development");
                b.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Features:Hangfire"] = "false",
                        ["RequestLogs:Enabled"] = "false",
                        ["Features:RateLimiting"] = "true",
                    });
                });
            });

        var client = rateLimitFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        // Use raw GET requests instead of FormPostHelper because the helper does GET-then-POST
        // and the GET will start hitting 429 partway through, throwing.
        var statusCodes = new List<HttpStatusCode>();
        for (var i = 0; i < 12; i++)
        {
            var response = await client.GetAsync("/Account/Login");
            statusCodes.Add(response.StatusCode);
        }

        Assert.That(statusCodes, Has.Some.EqualTo(HttpStatusCode.TooManyRequests),
            $"Expected at least one 429 from rate limiter after 12 requests. Got: [{string.Join(", ", statusCodes.Select(c => (int)c))}]");
    }

    // ── API key auth tests ──────────────────────────────────────────────────────────────────
    // The Phase 4 ApiKeyAuthenticationHandler had zero coverage. Seed an ApiKey for the seeded
    // admin user, then exercise the X-Api-Key header against /api/forecasts.

    [Test]
    public async Task Forecasts_Endpoint_With_Valid_ApiKey_Returns_Data()
    {
        const string testKey = "ek_test_integration_key";

        await using (var seedDb = await GetDbContextAsync())
        {
            var adminUser = await seedDb.Users.FirstAsync(u => u.Email == "admin@email.com");
            if (!await seedDb.ApiKeys.AnyAsync(k => k.Key == testKey))
            {
                seedDb.ApiKeys.Add(new ApiKey
                {
                    Name = "integration-test",
                    Key = testKey,
                    UserId = adminUser.Id,
                    CreatedDate = DateTime.UtcNow,
                });
                await seedDb.SaveChangesAsync();
            }
        }

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", testKey);

        var response = await client.GetAsync("/api/forecasts");
        Assert.That(response.IsSuccessStatusCode, Is.True,
            $"Expected 200, got {(int)response.StatusCode} {response.StatusCode}");

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain("\"items\"").IgnoreCase,
            "Response should be a paged result with an items array.");
    }

    [Test]
    public async Task Forecasts_Endpoint_With_Invalid_ApiKey_Returns_Unauthorized()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        client.DefaultRequestHeaders.Add("X-Api-Key", "ek_definitely_not_a_real_key");

        var response = await client.GetAsync("/api/forecasts");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Forecasts_Endpoint_With_Revoked_ApiKey_Returns_Unauthorized()
    {
        const string revokedKey = "ek_revoked_integration_key";

        await using (var seedDb = await GetDbContextAsync())
        {
            var adminUser = await seedDb.Users.FirstAsync(u => u.Email == "admin@email.com");
            if (!await seedDb.ApiKeys.AnyAsync(k => k.Key == revokedKey))
            {
                seedDb.ApiKeys.Add(new ApiKey
                {
                    Name = "revoked-integration-test",
                    Key = revokedKey,
                    UserId = adminUser.Id,
                    CreatedDate = DateTime.UtcNow,
                    RevokedDate = DateTime.UtcNow,
                });
                await seedDb.SaveChangesAsync();
            }
        }

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        client.DefaultRequestHeaders.Add("X-Api-Key", revokedKey);

        var response = await client.GetAsync("/api/forecasts");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    // ── ToolSlot audit history tests ────────────────────────────────────────────────────────
    // Exercise the /api/tool-slots endpoints end-to-end and verify a ToolSlotAuditLog row is
    // written for each create / update / delete.

    [Test]
    public async Task ToolSlot_Create_Update_Delete_Writes_Audit_Rows()
    {
        const string key = "ek_toolslot_audit_test";

        await using (var seedDb = await GetDbContextAsync())
        {
            var adminUser = await seedDb.Users.FirstAsync(u => u.Email == "admin@email.com");
            if (!await seedDb.ApiKeys.AnyAsync(k => k.Key == key))
            {
                seedDb.ApiKeys.Add(new ApiKey
                {
                    Name = "toolslot-audit-test",
                    Key = key,
                    UserId = adminUser.Id,
                    CreatedDate = DateTime.UtcNow,
                });
                await seedDb.SaveChangesAsync();
            }
        }

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", key);

        // Create
        var createPayload = new CreateToolSlotConfigurationRequest
        {
            Family = "AUDIT-FAM",
            MtCode = "AUDIT-MT",
            Destination = "AUDIT-DEST",
            Fcl1 = "slot-1",
            IsActive = true,
        };
        var createResponse = await client.PostAsJsonAsync("/api/tool-slots", createPayload);
        Assert.That(createResponse.IsSuccessStatusCode, Is.True,
            $"Create returned {(int)createResponse.StatusCode} {createResponse.StatusCode}");
        // IdResponse.Id is typed `object` on the wire, so fish the int out of the JsonElement directly.
        var createJson = await createResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var newId = createJson.GetProperty("id").GetInt32();

        // Update — change Family + IsActive; leave the rest null (UpdateToolSlotConfigurationRequest
        // treats nulls as "don't touch").
        var updatePayload = new UpdateToolSlotConfigurationRequest
        {
            Family = "AUDIT-FAM-2",
            IsActive = false,
        };
        var updateResponse = await client.PatchAsJsonAsync($"/api/tool-slots/{newId}", updatePayload);
        Assert.That(updateResponse.IsSuccessStatusCode, Is.True,
            $"Update returned {(int)updateResponse.StatusCode} {updateResponse.StatusCode}");

        // Delete
        var deleteResponse = await client.DeleteAsync($"/api/tool-slots/{newId}");
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Verify: three audit rows, one per action, all for the same slot id.
        await using var verifyDb = await GetDbContextAsync();
        var rows = await verifyDb.ToolSlotAuditLogs
            .Where(a => a.ToolSlotConfigurationId == newId)
            .OrderBy(a => a.ChangedDate)
            .ThenBy(a => a.Id)
            .ToListAsync();

        Assert.That(rows, Has.Count.EqualTo(3),
            "Expected exactly one audit row each for Created, Updated, and Deleted.");
        Assert.That(rows[0].Action, Is.EqualTo("Created"));
        Assert.That(rows[0].Family, Is.EqualTo("AUDIT-FAM"));
        Assert.That(rows[0].IsActive, Is.True);

        Assert.That(rows[1].Action, Is.EqualTo("Updated"));
        Assert.That(rows[1].Family, Is.EqualTo("AUDIT-FAM-2"));
        Assert.That(rows[1].IsActive, Is.False);
        Assert.That(rows[1].ChangeSummary, Does.Contain("Family: AUDIT-FAM → AUDIT-FAM-2"));
        Assert.That(rows[1].ChangeSummary, Does.Contain("IsActive: True → False"));

        Assert.That(rows[2].Action, Is.EqualTo("Deleted"));
        Assert.That(rows[2].Family, Is.EqualTo("AUDIT-FAM-2"),
            "Delete audit should snapshot the row's final state.");

        // All three should be attributed to admin@email.com (the owner of the API key used above).
        foreach (var row in rows)
        {
            Assert.That(row.ChangedBy, Is.EqualTo("admin@email.com"),
                "Audit row ChangedBy should match the API key owner.");
        }

        // Tests run against the real AdventureWorks2022_dev, so clean up after ourselves:
        // delete the three audit rows produced by this test. The source ToolSlotConfiguration
        // row was already deleted by the DELETE call above.
        await using var cleanupDb = await GetDbContextAsync();
        await cleanupDb.ToolSlotAuditLogs
            .Where(a => a.ToolSlotConfigurationId == newId)
            .ExecuteDeleteAsync();
    }

    // ── Email confirmation flow test ──────────────────────────────────────────────────────────
    // Exercises the full registration → email confirmation flow: register a new user, capture the
    // confirmation email via TestEmailSender, follow the confirmation link, and verify success.

    [Test]
    public async Task Register_Then_Confirm_Email_Completes_Successfully()
    {
        var uniqueEmail = $"test-{Guid.NewGuid():N}@integration-test.local";
        const string password = "Test@1234!Xyz";

        testEmailSender.SentEmails.Clear();

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        // 1. POST the registration form via FormPostHelper.
        var registerResponse = await FormPostHelper.PostFormAsync(client, "/Account/Register", "register",
            new Dictionary<string, string>
            {
                ["Input.Email"] = uniqueEmail,
                ["Input.Password"] = password,
                ["Input.ConfirmPassword"] = password,
            });

        // Registration with RequireConfirmedAccount=true redirects to /Account/Login.
        Assert.That(registerResponse.StatusCode,
            Is.EqualTo(HttpStatusCode.Redirect).Or.EqualTo(HttpStatusCode.Found),
            $"Expected redirect after registration, got {(int)registerResponse.StatusCode} {registerResponse.StatusCode}");

        // 2. Verify the test email sender captured a confirmation email.
        Assert.That(testEmailSender.SentEmails, Has.Count.GreaterThanOrEqualTo(1),
            "Expected at least one email to be sent after registration.");

        var confirmationEmail = testEmailSender.SentEmails
            .FirstOrDefault(e => e.Email == uniqueEmail && e.Subject == "Confirm your email");
        Assert.That(confirmationEmail, Is.Not.EqualTo(default((ApplicationUser, string, string, string))),
            $"Expected a confirmation email sent to {uniqueEmail}.");

        // 3. Extract the confirmation link from the email HTML.
        var hrefMatch = Regex.Match(confirmationEmail.HtmlMessage, @"href='([^']+)'");
        Assert.That(hrefMatch.Success, Is.True,
            "Expected the confirmation email to contain an href link.");

        // The confirmation link in the email is HTML-encoded (e.g. &amp; instead of &).
        var confirmationUrl = HttpUtility.HtmlDecode(hrefMatch.Groups[1].Value);
        // The URL from NavigationManager is absolute; convert to relative for the test client.
        var uri = new Uri(confirmationUrl);
        var relativePath = uri.PathAndQuery;

        // 4. GET the confirmation link. Use a new client that follows redirects so we land on
        //    the final rendered page (ConfirmEmail.razor may redirect through enhanced navigation).
        var followClient = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true,
        });
        var confirmResponse = await followClient.GetAsync(relativePath);

        Assert.That(confirmResponse.IsSuccessStatusCode, Is.True,
            $"Expected 200 from confirmation link, got {(int)confirmResponse.StatusCode} {confirmResponse.StatusCode}");

        var confirmBody = await confirmResponse.Content.ReadAsStringAsync();
        Assert.That(confirmBody, Does.Contain("Thank you for confirming your email"),
            "Expected the confirmation page to show a success message.");

        // 5. Clean up: delete the test user from the database.
        await using var db = await GetDbContextAsync();
        var testUser = await db.Users.FirstOrDefaultAsync(u => u.Email == uniqueEmail);
        if (testUser is not null)
        {
            db.Users.Remove(testUser);
            await db.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Resolves an <see cref="ApplicationDbContext"/> from the test factory's service provider
    /// so test setup code can read/write the AdventureWorks2022_dev schema directly.
    /// </summary>
    private async Task<ApplicationDbContext> GetDbContextAsync()
    {
        var scope = factory.Services.CreateScope();
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        return await dbFactory.CreateDbContextAsync();
    }

    /// <summary>
    /// Test double for <see cref="IEmailSender{ApplicationUser}"/> that captures all sent emails
    /// so tests can inspect confirmation links, password reset codes, etc.
    /// </summary>
    private sealed class TestEmailSender : IEmailSender<ApplicationUser>
    {
        public List<(ApplicationUser User, string Email, string Subject, string HtmlMessage)> SentEmails { get; } = [];

        public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        {
            SentEmails.Add((user, email, "Confirm your email", $"<a href='{confirmationLink}'>Confirm</a>"));
            return Task.CompletedTask;
        }

        public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            SentEmails.Add((user, email, "Reset your password", $"<a href='{resetLink}'>Reset</a>"));
            return Task.CompletedTask;
        }

        public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            SentEmails.Add((user, email, "Password reset code", resetCode));
            return Task.CompletedTask;
        }
    }
}
