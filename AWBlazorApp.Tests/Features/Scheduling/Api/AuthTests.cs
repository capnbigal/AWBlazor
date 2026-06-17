using System.Net;
using AWBlazorApp.Tests.Infrastructure.Testing;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Features.Scheduling.Api;

public class SchedulingApiAuthTests : IntegrationTestFixtureBase
{
    [TestCase("/api/scheduling/weekly-plans")]
    [TestCase("/api/scheduling/alerts")]
    [TestCase("/api/scheduling/lines")]
    [TestCase("/api/scheduling/line-products")]
    [TestCase("/api/scheduling/delivery?weekId=202601&locationId=60")]
    [TestCase("/api/scheduling/exceptions")]
    public async Task Endpoints_Without_Auth_Return_Unauthorized(string path)
    {
        using var client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var resp = await client.GetAsync(path);
        var code = (int)resp.StatusCode;
        // API endpoints return 401, cookie-protected pages may redirect (302) — both are non-200.
        Assert.That(code == 401 || code == 403 || (code >= 300 && code < 400),
            $"Expected auth challenge for {path}, got {code}");
    }
}
