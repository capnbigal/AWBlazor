using System.Net;
using System.Net.Http.Json;
using AWBlazorApp.Features.Identity.Domain;
using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using AWBlazorApp.Tests.Infrastructure.Testing;

namespace AWBlazorApp.Tests.Features.Admin.Api;

/// <summary>
/// Integration tests for the admin user-management endpoints, focused on the admin password-reset
/// path that lets an Admin rotate any account's password WITHOUT knowing the old one (the durable
/// replacement for hand-editing AspNetUsers in SQL). Runs against the real SQL Server dev database.
/// </summary>
public class UserAdminEndpointTests : IntegrationTestFixtureBase
{
    [Test]
    public async Task Admin_Can_Reset_User_Password_Without_Old_Password()
    {
        var apiKey = await EnsureAdminApiKeyAsync();
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        var email = $"reset-target-{Guid.NewGuid():N}@email.com";
        const string oldPassword = "Old@Pass123";
        const string newPassword = "New@Pass456";
        var userId = await CreateUserAsync(email, oldPassword);

        try
        {
            var response = await client.PostAsJsonAsync($"/api/users/{userId}/reset-password", new { newPassword });
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                $"Expected 200, got {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");

            using var scope = Factory.Services.CreateScope();
            var um = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var reloaded = await um.FindByIdAsync(userId);
            Assert.That(reloaded, Is.Not.Null);
            Assert.That(await um.CheckPasswordAsync(reloaded!, newPassword), Is.True, "New password should validate after reset.");
            Assert.That(await um.CheckPasswordAsync(reloaded!, oldPassword), Is.False, "Old password must no longer validate.");
        }
        finally
        {
            await DeleteUserAsync(userId);
        }
    }

    [Test]
    public async Task Reset_Password_Without_Admin_Is_Rejected()
    {
        var client = Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var response = await client.PostAsJsonAsync("/api/users/does-not-matter/reset-password", new { newPassword = "New@Pass456" });
        Assert.That(response.StatusCode,
            Is.EqualTo(HttpStatusCode.Unauthorized).Or.EqualTo(HttpStatusCode.Forbidden)
              .Or.EqualTo(HttpStatusCode.Redirect).Or.EqualTo(HttpStatusCode.Found),
            $"Expected 401/403/redirect for unauthenticated reset, got {(int)response.StatusCode}");
    }

    [Test]
    public async Task Reset_Password_Too_Weak_Returns_ValidationProblem()
    {
        var apiKey = await EnsureAdminApiKeyAsync();
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        var email = $"reset-weak-{Guid.NewGuid():N}@email.com";
        var userId = await CreateUserAsync(email, "Old@Pass123");
        try
        {
            // "short" violates the configured policy (length 8, upper/lower/digit/non-alphanumeric).
            var response = await client.PostAsJsonAsync($"/api/users/{userId}/reset-password", new { newPassword = "short" });
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
                $"Weak password should fail policy with a 400 ValidationProblem, got {(int)response.StatusCode}.");
        }
        finally
        {
            await DeleteUserAsync(userId);
        }
    }

    [Test]
    public async Task Reset_Password_For_Unknown_User_Returns_NotFound()
    {
        var apiKey = await EnsureAdminApiKeyAsync();
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        var response = await client.PostAsJsonAsync($"/api/users/{Guid.NewGuid():N}/reset-password", new { newPassword = "New@Pass456" });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound),
            $"Expected 404 for unknown user, got {(int)response.StatusCode}.");
    }

    private async Task<string> CreateUserAsync(string email, string password)
    {
        using var scope = Factory.Services.CreateScope();
        var um = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            Email = email,
            UserName = email,
            EmailConfirmed = true,
            FirstName = "Reset",
            LastName = "Target",
            DisplayName = "Reset Target",
        };
        var create = await um.CreateAsync(user, password);
        Assert.That(create.Succeeded, Is.True, string.Join("; ", create.Errors.Select(e => e.Description)));
        return user.Id;
    }

    private async Task DeleteUserAsync(string userId)
    {
        using var scope = Factory.Services.CreateScope();
        var um = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var u = await um.FindByIdAsync(userId);
        if (u is not null) await um.DeleteAsync(u);
    }

    private static string? _adminApiKey;

    private async Task<string> EnsureAdminApiKeyAsync()
    {
        if (_adminApiKey is not null) return _adminApiKey;

        await using var db = await GetDbContextAsync();
        var adminRoleId = await db.Roles.Where(r => r.Name == AppRoles.Admin).Select(r => r.Id).FirstAsync();
        var adminUserId = await db.UserRoles.Where(ur => ur.RoleId == adminRoleId).Select(ur => ur.UserId).FirstAsync();

        var rawKey = "ek_useradmin_" + Guid.NewGuid().ToString("N");
        db.ApiKeys.Add(new ApiKey
        {
            Name = "user-admin-tests",
            Key = ApiKeyHasher.Hash(rawKey),
            UserId = adminUserId,
            CreatedDate = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
        _adminApiKey = rawKey;
        return rawKey;
    }
}
