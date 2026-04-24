using AWBlazorApp.Tests.Infrastructure.Testing;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Features.Processes.Timelines.Api;

public class EndpointTests : IntegrationTestFixtureBase
{
    [TestCase("/api/processes/chains")]
    [TestCase("/api/processes/chains/sales-to-ship/timeline?rootEntityId=43659")]
    [TestCase("/api/processes/chains/recent")]
    public async Task Endpoints_Without_Auth_Return_Unauthorized_Or_Redirect(string path)
    {
        using var client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var resp = await client.GetAsync(path);
        var code = (int)resp.StatusCode;
        Assert.That(code == 401 || code == 403 || (code >= 300 && code < 400),
            $"Expected auth challenge for {path}, got {code}");
    }
}
