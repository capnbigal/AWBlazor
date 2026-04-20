using System.Net;
using AWBlazorApp.Features.Identity.Domain;
using AWBlazorApp.Tests.Infrastructure.Testing;
using AWBlazorApp.Tests.Shared.Testing;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Features.Maintenance;

/// <summary>
/// Guards the ToolSlots route normalization: the canonical URLs are under
/// <c>/maintenance/tool-slots</c> (matching the feature folder's Maintenance ownership), and
/// the pre-refactor URLs under <c>/tool-slots</c> remain available as legacy aliases so
/// existing bookmarks keep working.
/// </summary>
public class ToolSlotsRouteTests : IntegrationTestFixtureBase
{
    private const string TestPassword = "Test@1234!Xyz";

    [TestCase("/maintenance/tool-slots")]
    [TestCase("/tool-slots")]
    public async Task ToolSlots_Index_Resolves_For_Authenticated_User(string path)
    {
        var (email, password) = await CreateTestUserAsync("toolslots-route");
        try
        {
            var client = await CreateAuthenticatedClientAsync(email, password);
            var response = await client.GetAsync(path);
            Assert.That(response.IsSuccessStatusCode, Is.True,
                $"Expected 200 on {path}, got {(int)response.StatusCode}.");

            var body = await response.Content.ReadAsStringAsync();
            Assert.That(body, Does.Contain("Tool slot configurations"),
                "Both the canonical and legacy route must render the ToolSlots Index page.");
        }
        finally
        {
            await DeleteTestUserAsync(email);
        }
    }

    [TestCase("/maintenance/tool-slots/history")]
    [TestCase("/tool-slots/history")]
    public async Task ToolSlots_History_Resolves_For_Authenticated_User(string path)
    {
        var (email, password) = await CreateTestUserAsync("toolslots-history");
        try
        {
            var client = await CreateAuthenticatedClientAsync(email, password);
            var response = await client.GetAsync(path);
            Assert.That(response.IsSuccessStatusCode, Is.True,
                $"Expected 200 on {path}, got {(int)response.StatusCode}.");

            var body = await response.Content.ReadAsStringAsync();
            Assert.That(body, Does.Contain("Tool slot history"),
                "Both the canonical and legacy history route must render the History page.");
        }
        finally
        {
            await DeleteTestUserAsync(email);
        }
    }

    [TestCase("/maintenance/tool-slots/history/42")]
    [TestCase("/tool-slots/history/42")]
    public async Task ToolSlots_History_With_Id_Resolves_For_Authenticated_User(string path)
    {
        var (email, password) = await CreateTestUserAsync("toolslots-history-id");
        try
        {
            var client = await CreateAuthenticatedClientAsync(email, password);
            var response = await client.GetAsync(path);
            Assert.That(response.IsSuccessStatusCode, Is.True,
                $"Expected 200 on {path}, got {(int)response.StatusCode}.");

            var body = await response.Content.ReadAsStringAsync();
            Assert.That(body, Does.Contain("History for tool slot #42"),
                "Both the canonical and legacy history-by-id route must bind {SlotId:int}.");
        }
        finally
        {
            await DeleteTestUserAsync(email);
        }
    }

    // ── Helpers (mirrors the pattern used by IdentityManageFormPostTests) ──────────────────────

    private async Task<(string email, string password)> CreateTestUserAsync(string prefix)
    {
        var email = $"{prefix}-{Guid.NewGuid():N}@integration-test.local";

        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, TestPassword);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create test user {email}: " +
                string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}")));
        }
        return (email, TestPassword);
    }

    private async Task DeleteTestUserAsync(string email)
    {
        await using var db = await GetDbContextAsync();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null) return;
        db.Users.Remove(user);
        await db.SaveChangesAsync();
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string email, string password)
    {
        var client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var response = await FormPostHelper.PostFormAsync(client, "/Account/Login", "login",
            new Dictionary<string, string>
            {
                ["Input.Email"] = email,
                ["Input.Password"] = password,
                ["Input.RememberMe"] = "false",
            });
        if (response.StatusCode != HttpStatusCode.Redirect && response.StatusCode != HttpStatusCode.Found)
        {
            throw new InvalidOperationException(
                $"Sign-in for {email} failed: expected 302 from /Account/Login, got {(int)response.StatusCode}.");
        }
        return client;
    }
}
