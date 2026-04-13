using System.Security.Claims;
using System.Text.Encodings.Web;
using AWBlazorApp.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AWBlazorApp.Authentication;

public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string Scheme = "ApiKey";
    public const string HeaderName = "X-Api-Key";
}

/// <summary>
/// Authenticates a request by reading the <c>X-Api-Key</c> header and looking up an active
/// <see cref="Data.Entities.ApiKey"/>. On success the request runs as the owning user with
/// their stored Identity roles.
/// </summary>
public sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<ApiKeyAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IDbContextFactory<ApplicationDbContext> dbFactory,
    UserManager<ApplicationUser> userManager)
    : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyAuthenticationOptions.HeaderName, out var headerValues))
        {
            return AuthenticateResult.NoResult();
        }

        var providedKey = headerValues.ToString();
        if (string.IsNullOrWhiteSpace(providedKey))
        {
            return AuthenticateResult.NoResult();
        }

        await using var db = await dbFactory.CreateDbContextAsync();

        // Support both legacy plain-text keys and SHA-256 hashed keys.
        // Try exact match first (plain-text), then hash match.
        var hashedKey = ApiKeyHasher.Hash(providedKey);
        var apiKey = await db.ApiKeys
            .Include(k => k.User)
            .FirstOrDefaultAsync(k => k.Key == providedKey || k.Key == hashedKey);

        if (apiKey is null)
        {
            return AuthenticateResult.Fail("Invalid API key.");
        }

        if (apiKey.RevokedDate is not null)
        {
            return AuthenticateResult.Fail("API key has been revoked.");
        }

        if (apiKey.ExpiresDate is not null && apiKey.ExpiresDate < DateTime.UtcNow)
        {
            return AuthenticateResult.Fail("API key has expired.");
        }

        if (apiKey.User is null)
        {
            return AuthenticateResult.Fail("API key has no owning user.");
        }

        // Pull the user's roles so [Authorize(Roles = ...)] checks work via API key auth.
        var roles = await userManager.GetRolesAsync(apiKey.User);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, apiKey.User.Id),
            new(ClaimTypes.Name, apiKey.User.UserName ?? apiKey.User.Email ?? apiKey.User.Id),
            new("ApiKeyId", apiKey.Id.ToString()),
            new("ApiKeyName", apiKey.Name),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        // Best-effort last-used tracking — fire-and-forget so it never blocks the request.
        _ = Task.Run(async () =>
        {
            try
            {
                await using var inner = await dbFactory.CreateDbContextAsync();
                var entity = await inner.ApiKeys.FirstOrDefaultAsync(k => k.Id == apiKey.Id);
                if (entity is not null)
                {
                    entity.LastUsedDate = DateTime.UtcNow;
                    await inner.SaveChangesAsync();
                }
            }
            catch
            {
                // Best-effort; intentionally swallowed.
            }
        });

        var identity = new ClaimsIdentity(claims, ApiKeyAuthenticationOptions.Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, ApiKeyAuthenticationOptions.Scheme);
        return AuthenticateResult.Success(ticket);
    }
}
