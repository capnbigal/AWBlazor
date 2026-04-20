using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AWBlazorApp.Tests.Infrastructure.Testing;

/// <summary>
/// Shared fixture base — provides Factory and GetDbContextAsync without each test class
/// rebuilding the WebApplicationFactory (host startup is slow against the real SQL Server).
/// Test classes inherit this; NUnit reuses the OneTimeSetUp across [TestCaseSource] enumerations.
/// </summary>
public abstract class IntegrationTestFixtureBase
{
    private static WebApplicationFactory<Program>? _sharedFactory;
    private static readonly object _factoryLock = new();

    protected WebApplicationFactory<Program> Factory
    {
        get
        {
            if (_sharedFactory is not null) return _sharedFactory;
            lock (_factoryLock)
            {
                _sharedFactory ??= new WebApplicationFactory<Program>()
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
                        // Production runs on HTTPS, so AddApplicationCookieHardening pins the
                        // Identity application cookie to CookieSecurePolicy.Always. The
                        // in-process TestServer only speaks HTTP, so the auth cookie would be
                        // set by the server but never echoed back by the client on subsequent
                        // requests. Relax the policy for tests so authenticated flows work end-
                        // to-end. Production wiring is unchanged.
                        builder.ConfigureTestServices(services =>
                        {
                            services.ConfigureApplicationCookie(options =>
                            {
                                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                            });

                            // Replace IAntiforgery with a permissive implementation. Production
                            // antiforgery binds the request token to the user's claims at token-
                            // issue time; an HTTP-driven sign-in flow (GET /Account/Login →
                            // POST /Account/Login → authenticated GET → POST) leaves the client
                            // holding a token issued for an anonymous user, which fails server
                            // validation for the now-authenticated follow-up POST. Real browsers
                            // work because the Identity redirect chain gives the browser a
                            // chance to re-issue the antiforgery cookie, but HttpClient +
                            // TestServer doesn't reliably reproduce that chain. Tests substitute
                            // a pass-through implementation so we can exercise the SSR form-
                            // binding path end-to-end. Production wiring is unchanged.
                            services.Replace(ServiceDescriptor.Singleton<IAntiforgery, PermissiveTestAntiforgery>());
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

    /// <summary>
    /// Test-only <see cref="IAntiforgery"/>: renders stable placeholder tokens so the SSR form
    /// pipeline still emits a <c>&lt;input name=&quot;__RequestVerificationToken&quot;&gt;</c>
    /// element (which is what the form-binder regex in FormPostHelper looks for), but skips
    /// validation on POST. See the rationale comment on the registration above.
    /// </summary>
    private sealed class PermissiveTestAntiforgery : IAntiforgery
    {
        private const string PlaceholderToken = "test-antiforgery-token";

        public AntiforgeryTokenSet GetAndStoreTokens(HttpContext httpContext) => GetTokens(httpContext);

        public AntiforgeryTokenSet GetTokens(HttpContext httpContext) => new(
            requestToken: PlaceholderToken,
            cookieToken: PlaceholderToken,
            formFieldName: "__RequestVerificationToken",
            headerName: "RequestVerificationToken");

        public Task<bool> IsRequestValidAsync(HttpContext httpContext) => Task.FromResult(true);

        public Task ValidateRequestAsync(HttpContext httpContext) => Task.CompletedTask;

        public void SetCookieTokenAndHeader(HttpContext httpContext) { }
    }
}
