using System.Net;
using AWBlazorApp.Tests.Infrastructure.Testing;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Features.Processes.Timelines.Pages;

public class IndexPageTests : IntegrationTestFixtureBase
{
    [Test]
    public async Task Timeline_Page_Redirects_Anonymous()
    {
        using var client = Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var resp = await client.GetAsync("/processes/timeline");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Redirect).Or.EqualTo(HttpStatusCode.Found));
    }
}
