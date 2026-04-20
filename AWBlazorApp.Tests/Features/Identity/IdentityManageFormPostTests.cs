using System.Net;
using AWBlazorApp.Features.Identity.Domain;
using AWBlazorApp.Infrastructure.Authentication;
using AWBlazorApp.Shared.Domain;
using AWBlazorApp.Tests.Infrastructure.Testing;
using AWBlazorApp.Tests.Shared.Testing;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Features.Identity;

/// <summary>
/// Form-POST integration coverage for /Account/Manage/* pages. These catch the same class
/// of SSR-binding regressions that the pre-existing Login/Register/ForgotPassword tests
/// guard: MudBlazor inputs silently dropping <c>name</c> attributes, [SupplyParameterFromForm]
/// properties being nulled by the binder, Blazor EditForm Model parameter null, etc.
///
/// Each mutating test creates a throwaway user so it can freely change passwords / delete
/// the account without mutating the seeded admin and poisoning sibling tests.
/// </summary>
public class IdentityManageFormPostTests : IntegrationTestFixtureBase
{
    private const string TestPassword = "Test@1234!Xyz";

    // ── Manage Index (profile) ────────────────────────────────────────────────────────────────

    [Test]
    public async Task Manage_Index_Page_Requires_Authentication()
    {
        var client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var response = await client.GetAsync("/Account/Manage");

        // Manage pages protect themselves via IdentityUserAccessor.GetRequiredUserAsync,
        // which redirects anonymous requests to /Account/InvalidUser (not /Account/Login) —
        // same behavior as the stock .NET Identity scaffold for this family of pages.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect).Or.EqualTo(HttpStatusCode.Found));
        Assert.That(response.Headers.Location?.ToString(), Does.Contain("/Account/InvalidUser"));
    }

    [Test]
    public async Task Manage_Index_Page_Get_For_Authenticated_User_Succeeds()
    {
        var (email, password, _) = await CreateTestUserAsync("manage-index-get");
        try
        {
            var client = await CreateAuthenticatedClientAsync(email, password);
            var response = await client.GetAsync("/Account/Manage");
            Assert.That(response.IsSuccessStatusCode, Is.True,
                $"Expected 200 on /Account/Manage, got {(int)response.StatusCode}.");

            var body = await response.Content.ReadAsStringAsync();
            Assert.That(body, Does.Contain("My profile"),
                "Manage index should render the profile heading.");
            Assert.That(body, Does.Contain("name=\"Input.FirstName\""),
                "Manage index should emit an SSR-safe <input name=\"Input.FirstName\"> from <InputText>.");
        }
        finally
        {
            await DeleteTestUserAsync(email);
        }
    }

    [Test]
    public async Task Manage_Index_Form_Post_Updates_Profile_Fields()
    {
        var (email, password, _) = await CreateTestUserAsync("manage-index-update");
        try
        {
            var client = await CreateAuthenticatedClientAsync(email, password);

            var response = await FormPostHelper.PostFormAsync(client, "/Account/Manage", "profile",
                new Dictionary<string, string>
                {
                    ["Input.FirstName"] = "Integration",
                    ["Input.LastName"] = "Tester",
                    ["Input.DisplayName"] = "Int. Tester",
                    ["Input.PhoneNumber"] = "+1-555-0100",
                });

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect).Or.EqualTo(HttpStatusCode.Found),
                $"Expected redirect on profile save, got {(int)response.StatusCode}.");

            await using var db = await GetDbContextAsync();
            var updated = await db.Users.AsNoTracking().FirstAsync(u => u.Email == email);
            Assert.That(updated.FirstName, Is.EqualTo("Integration"));
            Assert.That(updated.LastName, Is.EqualTo("Tester"));
            Assert.That(updated.DisplayName, Is.EqualTo("Int. Tester"));
            Assert.That(updated.PhoneNumber, Is.EqualTo("+1-555-0100"));
        }
        finally
        {
            await DeleteTestUserAsync(email);
        }
    }

    // ── ChangePassword ────────────────────────────────────────────────────────────────────────

    [Test]
    public async Task ChangePassword_Page_Requires_Authentication()
    {
        var client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var response = await client.GetAsync("/Account/Manage/ChangePassword");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect).Or.EqualTo(HttpStatusCode.Found));
        Assert.That(response.Headers.Location?.ToString(), Does.Contain("/Account/InvalidUser"));
    }

    [Test]
    public async Task ChangePassword_Form_Post_With_Valid_Current_Password_Changes_Password()
    {
        var (email, oldPassword, _) = await CreateTestUserAsync("cp-valid");
        const string newPassword = "Test@9999!Abc";
        try
        {
            var client = await CreateAuthenticatedClientAsync(email, oldPassword);

            var response = await FormPostHelper.PostFormAsync(client, "/Account/Manage/ChangePassword", "change-password",
                new Dictionary<string, string>
                {
                    ["Input.OldPassword"] = oldPassword,
                    ["Input.NewPassword"] = newPassword,
                    ["Input.ConfirmPassword"] = newPassword,
                });

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect).Or.EqualTo(HttpStatusCode.Found),
                $"Expected redirect after successful password change, got {(int)response.StatusCode}.");

            // Verify: new password signs in, old password no longer does.
            var newLoginClient = Factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });
            var newLogin = await FormPostHelper.PostFormAsync(newLoginClient, "/Account/Login", "login",
                new Dictionary<string, string>
                {
                    ["Input.Email"] = email,
                    ["Input.Password"] = newPassword,
                    ["Input.RememberMe"] = "false",
                });
            Assert.That(newLogin.StatusCode, Is.EqualTo(HttpStatusCode.Redirect).Or.EqualTo(HttpStatusCode.Found),
                "New password should allow sign-in.");

            var oldLoginClient = Factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });
            var oldLogin = await FormPostHelper.PostFormAsync(oldLoginClient, "/Account/Login", "login",
                new Dictionary<string, string>
                {
                    ["Input.Email"] = email,
                    ["Input.Password"] = oldPassword,
                    ["Input.RememberMe"] = "false",
                });
            Assert.That(oldLogin.IsSuccessStatusCode, Is.True,
                "Old password attempt should re-render login page (not redirect).");
            var body = await oldLogin.Content.ReadAsStringAsync();
            Assert.That(body, Does.Contain("Invalid login attempt"),
                "Old password should no longer sign the user in.");
        }
        finally
        {
            await DeleteTestUserAsync(email);
        }
    }

    [Test]
    public async Task ChangePassword_Form_Post_With_Wrong_Current_Password_Leaves_Password_Unchanged()
    {
        var (email, password, _) = await CreateTestUserAsync("cp-wrong");
        try
        {
            var client = await CreateAuthenticatedClientAsync(email, password);

            var response = await FormPostHelper.PostFormAsync(client, "/Account/Manage/ChangePassword", "change-password",
                new Dictionary<string, string>
                {
                    ["Input.OldPassword"] = "definitely-not-the-password",
                    ["Input.NewPassword"] = "Unused@1234!Xyz",
                    ["Input.ConfirmPassword"] = "Unused@1234!Xyz",
                });

            // Wrong current password → stay on page, status message set, NO redirect away.
            Assert.That(response.IsSuccessStatusCode, Is.True,
                $"Expected 200 with error message, got {(int)response.StatusCode}.");

            // Original password must still work.
            var verifyClient = Factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });
            var verifyLogin = await FormPostHelper.PostFormAsync(verifyClient, "/Account/Login", "login",
                new Dictionary<string, string>
                {
                    ["Input.Email"] = email,
                    ["Input.Password"] = password,
                    ["Input.RememberMe"] = "false",
                });
            Assert.That(verifyLogin.StatusCode, Is.EqualTo(HttpStatusCode.Redirect).Or.EqualTo(HttpStatusCode.Found),
                "Original password must still authenticate after a failed change-password attempt.");
        }
        finally
        {
            await DeleteTestUserAsync(email);
        }
    }

    [Test]
    public async Task ChangePassword_Form_Post_With_Mismatched_Confirmation_Stays_On_Page()
    {
        var (email, password, _) = await CreateTestUserAsync("cp-mismatch");
        try
        {
            var client = await CreateAuthenticatedClientAsync(email, password);

            var response = await FormPostHelper.PostFormAsync(client, "/Account/Manage/ChangePassword", "change-password",
                new Dictionary<string, string>
                {
                    ["Input.OldPassword"] = password,
                    ["Input.NewPassword"] = "Mismatch@1234!",
                    ["Input.ConfirmPassword"] = "Different@1234!",
                });

            Assert.That(response.IsSuccessStatusCode, Is.True,
                "Validation error should re-render the page, not redirect.");
            var body = await response.Content.ReadAsStringAsync();
            Assert.That(body, Does.Contain("password and confirmation password do not match").IgnoreCase);
        }
        finally
        {
            await DeleteTestUserAsync(email);
        }
    }

    // ── DeletePersonalData ────────────────────────────────────────────────────────────────────

    [Test]
    public async Task DeletePersonalData_Page_Get_For_Authenticated_User_Shows_Form()
    {
        var (email, password, _) = await CreateTestUserAsync("delete-get");
        try
        {
            var client = await CreateAuthenticatedClientAsync(email, password);
            var response = await client.GetAsync("/Account/Manage/DeletePersonalData");
            Assert.That(response.IsSuccessStatusCode, Is.True);

            var body = await response.Content.ReadAsStringAsync();
            Assert.That(body, Does.Contain("Delete account"));
            Assert.That(body, Does.Contain("name=\"Input.ConfirmIntent\""),
                "DeletePersonalData should emit an SSR-safe <input name=\"Input.ConfirmIntent\"> from <InputCheckbox>.");
        }
        finally
        {
            await DeleteTestUserAsync(email);
        }
    }

    [Test]
    public async Task DeletePersonalData_Form_Post_With_Wrong_Password_Leaves_User_Intact()
    {
        var (email, password, _) = await CreateTestUserAsync("delete-wrong");
        try
        {
            var client = await CreateAuthenticatedClientAsync(email, password);

            var response = await FormPostHelper.PostFormAsync(client, "/Account/Manage/DeletePersonalData", "delete-account",
                new Dictionary<string, string>
                {
                    ["Input.Password"] = "not-the-password",
                    ["Input.ConfirmIntent"] = "true",
                });

            Assert.That(response.IsSuccessStatusCode, Is.True,
                "Wrong password should re-render the page, not redirect.");

            await using var db = await GetDbContextAsync();
            var stillExists = await db.Users.AnyAsync(u => u.Email == email);
            Assert.That(stillExists, Is.True, "User must not be deleted when the password check fails.");
        }
        finally
        {
            await DeleteTestUserAsync(email);
        }
    }

    [Test]
    public async Task DeletePersonalData_Form_Post_With_Correct_Password_And_Confirm_Removes_User()
    {
        var (email, password, _) = await CreateTestUserAsync("delete-ok");
        var client = await CreateAuthenticatedClientAsync(email, password);

        var response = await FormPostHelper.PostFormAsync(client, "/Account/Manage/DeletePersonalData", "delete-account",
            new Dictionary<string, string>
            {
                ["Input.Password"] = password,
                ["Input.ConfirmIntent"] = "true",
            });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect).Or.EqualTo(HttpStatusCode.Found),
            $"Expected redirect after successful self-delete, got {(int)response.StatusCode}.");

        await using var db = await GetDbContextAsync();
        var stillExists = await db.Users.AnyAsync(u => u.Email == email);
        Assert.That(stillExists, Is.False, "User should be deleted after a valid self-delete POST.");
    }

    // ── Email ─────────────────────────────────────────────────────────────────────────────────

    [Test]
    public async Task Email_Page_Get_Shows_Current_Email()
    {
        var (email, password, _) = await CreateTestUserAsync("email-get");
        try
        {
            var client = await CreateAuthenticatedClientAsync(email, password);
            var response = await client.GetAsync("/Account/Manage/Email");
            Assert.That(response.IsSuccessStatusCode, Is.True);

            var body = await response.Content.ReadAsStringAsync();
            Assert.That(body, Does.Contain(email),
                "Email page should render the current user's email.");
            Assert.That(body, Does.Contain("name=\"Input.NewEmail\""),
                "Email page should emit an SSR-safe <input name=\"Input.NewEmail\">.");
        }
        finally
        {
            await DeleteTestUserAsync(email);
        }
    }

    [Test]
    public async Task Email_Form_Post_Same_Email_Does_Not_Send_Change_Request()
    {
        var (email, password, _) = await CreateTestUserAsync("email-same");
        try
        {
            var client = await CreateAuthenticatedClientAsync(email, password);

            var response = await FormPostHelper.PostFormAsync(client, "/Account/Manage/Email", "change-email",
                new Dictionary<string, string>
                {
                    ["Input.NewEmail"] = email,
                });

            Assert.That(response.IsSuccessStatusCode, Is.True);
            var body = await response.Content.ReadAsStringAsync();
            Assert.That(body, Does.Contain("unchanged").IgnoreCase,
                "Posting the same email should produce the 'email is unchanged' status message.");

            // Email not mutated in the DB.
            await using var db = await GetDbContextAsync();
            var row = await db.Users.AsNoTracking().FirstAsync(u => u.Email == email);
            Assert.That(row.Email, Is.EqualTo(email));
        }
        finally
        {
            await DeleteTestUserAsync(email);
        }
    }

    [Test]
    public async Task Email_Form_Post_With_Different_Email_Shows_Confirmation_Link_Sent_Message()
    {
        var (email, password, _) = await CreateTestUserAsync("email-new");
        try
        {
            var client = await CreateAuthenticatedClientAsync(email, password);
            var newEmail = $"new-{Guid.NewGuid():N}@integration-test.local";

            var response = await FormPostHelper.PostFormAsync(client, "/Account/Manage/Email", "change-email",
                new Dictionary<string, string>
                {
                    ["Input.NewEmail"] = newEmail,
                });

            Assert.That(response.IsSuccessStatusCode, Is.True);
            var body = await response.Content.ReadAsStringAsync();
            Assert.That(body, Does.Contain("Confirmation link to change email sent"),
                "Expected the 'confirmation link sent' status message.");

            // Change-email flow is token-based: the underlying email must NOT be changed yet.
            await using var db = await GetDbContextAsync();
            var row = await db.Users.AsNoTracking().FirstAsync(u => u.Id ==
                (db.Users.AsNoTracking().First(x => x.Email == email)).Id);
            Assert.That(row.Email, Is.EqualTo(email),
                "Email should stay unchanged until the confirmation link is visited.");
        }
        finally
        {
            await DeleteTestUserAsync(email);
        }
    }

    [Test]
    public async Task Email_Form_Post_With_Invalid_Email_Shows_Validation_Error()
    {
        var (email, password, _) = await CreateTestUserAsync("email-invalid");
        try
        {
            var client = await CreateAuthenticatedClientAsync(email, password);

            var response = await FormPostHelper.PostFormAsync(client, "/Account/Manage/Email", "change-email",
                new Dictionary<string, string>
                {
                    ["Input.NewEmail"] = "not-an-email",
                });

            Assert.That(response.IsSuccessStatusCode, Is.True,
                "Validation error should re-render the page, not redirect.");
            var body = await response.Content.ReadAsStringAsync();
            // DataAnnotations [EmailAddress] default message wording.
            Assert.That(body, Does.Contain("valid e-mail").IgnoreCase.Or.Contain("valid email").IgnoreCase);
        }
        finally
        {
            await DeleteTestUserAsync(email);
        }
    }

    // ── Two-factor pages ──────────────────────────────────────────────────────────────────────

    [Test]
    public async Task TwoFactorAuthentication_Page_Get_Returns_Page()
    {
        var (email, password, _) = await CreateTestUserAsync("tfa-get");
        try
        {
            var client = await CreateAuthenticatedClientAsync(email, password);
            var response = await client.GetAsync("/Account/Manage/TwoFactorAuthentication");
            Assert.That(response.IsSuccessStatusCode, Is.True);

            var body = await response.Content.ReadAsStringAsync();
            Assert.That(body, Does.Contain("Two-factor authentication").IgnoreCase);
        }
        finally
        {
            await DeleteTestUserAsync(email);
        }
    }

    [Test]
    public async Task Disable2fa_Form_Post_When_Enabled_Disables_2FA()
    {
        var (email, password, createdUser) = await CreateTestUserAsync("disable-2fa");
        try
        {
            // Sign in FIRST (via the standard password flow). Turning 2FA on before sign-in
            // would make PasswordSignInAsync return RequiresTwoFactor instead of Succeeded,
            // which the Login page renders as "Invalid login attempt" — we'd never get the
            // auth cookie. Enabling 2FA directly against the user row after sign-in is
            // equivalent to the user toggling it via EnableAuthenticator, which is what the
            // Disable2fa page is designed to revert.
            var client = await CreateAuthenticatedClientAsync(email, password);

            using (var scope = Factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var u = await userManager.FindByIdAsync(createdUser.Id);
                Assert.That(u, Is.Not.Null);
                var enable = await userManager.SetTwoFactorEnabledAsync(u!, true);
                Assert.That(enable.Succeeded, Is.True);
            }

            var response = await FormPostHelper.PostFormAsync(client, "/Account/Manage/Disable2fa", "disable-2fa",
                new Dictionary<string, string>());

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect).Or.EqualTo(HttpStatusCode.Found),
                $"Expected redirect after disabling 2FA, got {(int)response.StatusCode}.");
            Assert.That(response.Headers.Location?.ToString(),
                Does.Contain("Account/Manage/TwoFactorAuthentication").IgnoreCase);

            await using var db = await GetDbContextAsync();
            var row = await db.Users.AsNoTracking().FirstAsync(u => u.Email == email);
            Assert.That(row.TwoFactorEnabled, Is.False, "TwoFactorEnabled should be cleared after Disable2fa post.");
        }
        finally
        {
            await DeleteTestUserAsync(email);
        }
    }

    // ── ApiKeys ───────────────────────────────────────────────────────────────────────────────

    [Test]
    public async Task ApiKeys_Page_Requires_Authentication()
    {
        var client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var response = await client.GetAsync("/Account/Manage/ApiKeys");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect).Or.EqualTo(HttpStatusCode.Found));
        Assert.That(response.Headers.Location?.ToString(), Does.Contain("/Account/InvalidUser"));
    }

    [Test]
    public async Task ApiKeys_Form_Post_Generate_Creates_Hashed_Row_And_Returns_Plaintext_Once()
    {
        var (email, password, _) = await CreateTestUserAsync("apikeys-generate");
        try
        {
            var client = await CreateAuthenticatedClientAsync(email, password);

            var response = await FormPostHelper.PostFormAsync(client, "/Account/Manage/ApiKeys", "generate-key",
                new Dictionary<string, string>
                {
                    ["GenerateInput.Name"] = "integration-test-key",
                });

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect).Or.EqualTo(HttpStatusCode.Found),
                $"Expected redirect after key generation, got {(int)response.StatusCode}.");

            var location = response.Headers.Location?.ToString() ?? "";
            Assert.That(location, Does.Contain("new=ek_"),
                "The redirect should carry the plaintext key in the `new` query parameter so the user can copy it once.");

            // Extract the raw key from the redirect URL.
            var newParam = System.Web.HttpUtility.ParseQueryString(new Uri(location, UriKind.RelativeOrAbsolute).Query);
            var rawKey = newParam["new"];
            Assert.That(rawKey, Is.Not.Null.And.StartsWith("ek_"));

            // Database row must store the SHA-256 hash, not the plaintext.
            await using var db = await GetDbContextAsync();
            var user = await db.Users.AsNoTracking().FirstAsync(u => u.Email == email);
            var row = await db.ApiKeys.AsNoTracking().FirstAsync(k =>
                k.UserId == user.Id && k.Name == "integration-test-key");
            Assert.That(row.Key, Is.EqualTo(ApiKeyHasher.Hash(rawKey!)),
                "Stored key should be the SHA-256 hash of the plaintext shown to the user.");
            Assert.That(row.Key, Is.Not.EqualTo(rawKey),
                "Plaintext must never be persisted.");
        }
        finally
        {
            await DeleteTestUserAsync(email);
        }
    }

    [Test]
    public async Task ApiKeys_Form_Post_Generate_With_Empty_Name_Shows_Validation_Error()
    {
        var (email, password, _) = await CreateTestUserAsync("apikeys-empty");
        try
        {
            var client = await CreateAuthenticatedClientAsync(email, password);

            var response = await FormPostHelper.PostFormAsync(client, "/Account/Manage/ApiKeys", "generate-key",
                new Dictionary<string, string>
                {
                    ["GenerateInput.Name"] = "",
                });

            Assert.That(response.IsSuccessStatusCode, Is.True,
                "Empty name should re-render the page, not redirect.");

            await using var db = await GetDbContextAsync();
            var user = await db.Users.AsNoTracking().FirstAsync(u => u.Email == email);
            var anyKey = await db.ApiKeys.AnyAsync(k => k.UserId == user.Id);
            Assert.That(anyKey, Is.False, "No key row should be inserted on a validation failure.");
        }
        finally
        {
            await DeleteTestUserAsync(email);
        }
    }

    [Test]
    public async Task ApiKeys_Form_Post_Revoke_Marks_Key_Revoked()
    {
        var (email, password, createdUser) = await CreateTestUserAsync("apikeys-revoke");
        int keyId;

        // Seed an active key directly in the DB so the test doesn't depend on the generate flow.
        await using (var seedDb = await GetDbContextAsync())
        {
            var key = new ApiKey
            {
                Name = "to-revoke",
                Key = ApiKeyHasher.Hash("ek_revoke_integration_seed"),
                UserId = createdUser.Id,
                CreatedDate = DateTime.UtcNow,
            };
            seedDb.ApiKeys.Add(key);
            await seedDb.SaveChangesAsync();
            keyId = key.Id;
        }

        try
        {
            var client = await CreateAuthenticatedClientAsync(email, password);

            var response = await FormPostHelper.PostFormAsync(client, "/Account/Manage/ApiKeys", $"revoke-{keyId}",
                new Dictionary<string, string>
                {
                    ["RevokeInput.Id"] = keyId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                });

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect).Or.EqualTo(HttpStatusCode.Found),
                $"Expected redirect after revoke, got {(int)response.StatusCode}.");

            await using var db = await GetDbContextAsync();
            var revoked = await db.ApiKeys.AsNoTracking().FirstAsync(k => k.Id == keyId);
            Assert.That(revoked.RevokedDate, Is.Not.Null,
                "Revoke form post should stamp RevokedDate on the row.");
        }
        finally
        {
            await DeleteTestUserAsync(email);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a confirmed test user directly via <see cref="UserManager{TUser}"/> so the tests
    /// can sign in without needing to exercise the email-confirmation flow.
    /// </summary>
    private async Task<(string email, string password, ApplicationUser user)> CreateTestUserAsync(string prefix)
    {
        var email = $"{prefix}-{Guid.NewGuid():N}@integration-test.local";

        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
        };
        var result = await userManager.CreateAsync(user, TestPassword);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create test user {email}: " +
                string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}")));
        }

        return (email, TestPassword, user);
    }

    /// <summary>
    /// Best-effort cleanup; tests call this in a <c>finally</c> block. The user may have already
    /// been deleted by the test itself (e.g. the DeletePersonalData positive path).
    /// </summary>
    private async Task DeleteTestUserAsync(string email)
    {
        await using var db = await GetDbContextAsync();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null) return;

        // Remove any API keys the test may have created so FK constraints don't block the delete.
        var keys = await db.ApiKeys.Where(k => k.UserId == user.Id).ToListAsync();
        if (keys.Count > 0) db.ApiKeys.RemoveRange(keys);

        db.Users.Remove(user);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Signs in the supplied user over a fresh client with redirects DISABLED. The caller reuses
    /// the returned client for subsequent requests; the auth cookie persists in the client's
    /// cookie jar. Authenticated Manage pages render directly (no redirect on GET), so
    /// disabling auto-follow is safe for every test in this fixture.
    /// </summary>
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
                $"Sign-in for {email} failed: expected 302 from /Account/Login, got {(int)response.StatusCode} {response.StatusCode}.");
        }

        return client;
    }
}
