using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
