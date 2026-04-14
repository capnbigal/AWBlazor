# ASP.NET Core Identity + Blazor Web App Authentication Reference

Concise reference for .NET 10 Blazor Web Apps using ASP.NET Core Identity. Sourced from `learn.microsoft.com/aspnet/core/*?view=aspnetcore-10.0` (fetched 2026-04-13). Focused on patterns relevant to this codebase (Interactive Server + static SSR Identity pages + API key auth).

---

## 1. AuthenticationStateProvider patterns

Blazor flows auth via a `CascadingAuthenticationState` that wraps `Task<AuthenticationState>` and cascades it to every component. The provider implementation differs by render mode.

### Interactive Server — `RevalidatingServerAuthenticationStateProvider`

The standard template ships `IdentityRevalidatingAuthenticationStateProvider` (subclass of `RevalidatingServerAuthenticationStateProvider`). It re-checks the security stamp at a fixed interval so role/password changes propagate into long-lived SignalR circuits.

```csharp
internal sealed class IdentityRevalidatingAuthenticationStateProvider(
    ILoggerFactory loggerFactory,
    IServiceScopeFactory scopeFactory,
    IOptions<IdentityOptions> options)
    : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();
        return await ValidateSecurityStampAsync(userManager, authenticationState.User);
    }
}
```

Shorten the interval (`TimeSpan.FromMinutes(1)`) to make "sign out everywhere" effective more quickly. The matching `SecurityStampValidatorOptions.ValidationInterval` controls the equivalent check on the cookie itself.

### WebAssembly / Auto — persisted state

If you later add WASM or Auto render mode, serialize auth state server-to-client via `PersistentComponentState`:

```csharp
// Server Program.cs
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization(o => o.SerializeAllClaims = true);

// Client Program.cs
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();
```

### Consuming auth state in components

```razor
@inject AuthenticationStateProvider Auth

@code {
    [CascadingParameter] private Task<AuthenticationState>? AuthState { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var state = await Auth.GetAuthenticationStateAsync();
        var user = state.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            var role = user.FindFirst(ClaimTypes.Role)?.Value;
        }
    }
}
```

Never cache `AuthenticationState` across awaits — always re-call `GetAuthenticationStateAsync`.

### Gotcha: prerendering with a custom provider

A custom `AuthenticationStateProvider` that isn't `IHostEnvironmentAuthenticationStateProvider`-aware will render the unauthenticated branch during prerender, then flip to authenticated after the circuit opens. Either implement the interface or disable prerender on the `Routes` component.

---

## 2. Identity scaffolded pages — keep vs customize

The .NET 10 Blazor Web App template scaffolds Razor components (not Razor Pages) under `Components/Account/Pages/`. Each carries `@attribute [ExcludeFromInteractiveRouting]` so it runs as static SSR — required for `SignInManager` cookie writes.

| Page | Action |
|---|---|
| `Login.razor`, `Logout.razor` | **Keep & customize branding.** Add rate limiting, audit logging. |
| `Register.razor`, `ConfirmEmail.razor`, `ResendEmailConfirmation.razor` | **Keep.** Wire an `IEmailSender<ApplicationUser>` in production. |
| `ForgotPassword.razor`, `ResetPassword.razor` | **Keep.** Same email-sender requirement. |
| `LoginWith2fa.razor`, `LoginWithRecoveryCode.razor`, `Lockout.razor` | **Keep.** |
| `Manage/Index.razor`, `Email.razor`, `ChangePassword.razor` | **Keep.** Customize the user profile fields. |
| `Manage/TwoFactorAuthentication.razor`, `EnableAuthenticator.razor`, `Disable2fa.razor`, `GenerateRecoveryCodes.razor`, `ShowRecoveryCodes.razor`, `ResetAuthenticator.razor` | **Keep.** Add QR-code rendering (the template ships only the manual key + otpauth URI). |
| `Manage/ExternalLogins.razor`, `SetPassword.razor` | **Keep if external providers are wired up.** |
| `Manage/PersonalData.razor`, `DeletePersonalData.razor`, `DownloadPersonalData.razor` | **Keep for GDPR;** delete if not needed. |
| `AccessDenied.razor`, `InvalidUser.razor`, `InvalidPasswordReset.razor` | **Keep.** Brand them. |

Add your own `/Account/Manage/ApiKeys.razor` style pages inside `Manage/` with the same `[ExcludeFromInteractiveRouting]` attribute — this lets you use SSR forms consistently.

**Do NOT inject `SignInManager<TUser>` or `UserManager<TUser>` into interactive Razor components.** They're designed for HTTP request/response; use them from static-SSR pages or from a minimal-API endpoint.

---

## 3. Cookie configuration + secure defaults

`ConfigureApplicationCookie` must be called **after** `AddIdentity`/`AddDefaultIdentity`:

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name          = "__Host-Auth";           // __Host- prefix = stricter
    options.Cookie.HttpOnly      = true;                    // no JS access
    options.Cookie.SecurePolicy  = CookieSecurePolicy.Always;
    options.Cookie.SameSite      = SameSiteMode.Lax;        // Lax for OAuth callbacks
    options.ExpireTimeSpan       = TimeSpan.FromHours(8);
    options.SlidingExpiration    = true;
    options.LoginPath            = "/Account/Login";
    options.LogoutPath           = "/Account/Logout";
    options.AccessDeniedPath     = "/Account/AccessDenied";
    options.ReturnUrlParameter   = CookieAuthenticationDefaults.ReturnUrlParameter;
});

// Force security-stamp revalidation every minute (default 30m)
builder.Services.Configure<SecurityStampValidatorOptions>(o =>
    o.ValidationInterval = TimeSpan.FromMinutes(1));
```

**Defaults worth overriding:**

| Option | Default | Recommended |
|---|---|---|
| `Password.RequiredLength` | 6 | 10–12 |
| `Password.RequiredUniqueChars` | 1 | 4 |
| `Lockout.MaxFailedAccessAttempts` | 5 | 5 (keep) |
| `Lockout.DefaultLockoutTimeSpan` | 5 min | 15 min |
| `SignIn.RequireConfirmedAccount` | `false` | `true` |
| `User.RequireUniqueEmail` | `false` | `true` |
| `PasswordHasherOptions.IterationCount` | 100,000 | 210,000+ (OWASP 2023) |

```csharp
builder.Services.Configure<IdentityOptions>(o =>
{
    o.Password.RequiredLength     = 12;
    o.Password.RequiredUniqueChars = 4;
    o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    o.SignIn.RequireConfirmedAccount = true;
    o.User.RequireUniqueEmail     = true;
});
builder.Services.Configure<PasswordHasherOptions>(o => o.IterationCount = 210_000);
```

### "Sign out everywhere"

```csharp
await userManager.UpdateSecurityStampAsync(user);
// All existing cookies invalidated on next validation interval.
```

---

## 4. Antiforgery in Blazor

### When it works automatically

- `AddRazorComponents()` auto-registers antiforgery services.
- `app.UseAntiforgery()` must come **after** `UseAuthentication` / `UseAuthorization`.
- `<EditForm method="post">` on a static-SSR page auto-emits a token (via the built-in `<AntiforgeryToken />` component injected by the form handler).
- Interactive circuits don't need tokens per POST — the SignalR handshake already carries a negotiation token.

### When it doesn't

- **Minimal API POST endpoints** validate antiforgery by default for form posts. For a JSON/bearer API, opt out:

  ```csharp
  app.MapPost("/api/keys", (...) => ...).DisableAntiforgery();
  ```

- **Blazor Server circuit logout**: the circuit's token can desync with the HTTP pipeline. In this codebase `/Account/Logout` calls `.DisableAntiforgery()` for this reason.
- **AJAX from interactive components** against static-SSR endpoints needs the token in a header. Grab it via `AntiforgeryStateProvider`:

  ```razor
  @inject AntiforgeryStateProvider Antiforgery
  @code {
      var token = Antiforgery.GetAntiforgeryToken();
      // token.Value -> stick into X-CSRF-TOKEN header
  }
  ```

### Configuration

```csharp
builder.Services.AddAntiforgery(o =>
{
    o.HeaderName = "X-CSRF-TOKEN";
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.Cookie.Name = "__Host-Antiforgery";
});
```

### Common gotchas

1. MudBlazor inputs in static-SSR `<EditForm method="post">` don't emit `name` — use `<InputText>`/`<InputCheckbox>`. (Already documented in `CLAUDE.md`.)
2. `[SupplyParameterFromForm]` properties get null-reset by the binder; re-initialize in `OnInitialized`.
3. `DisableAntiforgery()` on a cookie-auth endpoint re-opens CSRF. Only use it when the endpoint authenticates via bearer token or API key header (which aren't sent by cross-origin browser forms).

---

## 5. Authorization: policies vs roles vs claims vs custom requirements

### Roles (simplest)

```csharp
[Authorize(Roles = "Admin,Manager")]
public class AdminController : Controller { }
```

```razor
<AuthorizeView Roles="Admin, Manager">
    <Authorized>...</Authorized>
</AuthorizeView>
```

### Claims-based policy

```csharp
builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("CanEditProducts", p =>
        p.RequireClaim("permission", "products.edit"));
    o.AddPolicy("AtLeast21", p =>
        p.RequireAssertion(ctx => ctx.User.HasClaim("age", v => int.Parse(v.Value) >= 21)));
});
```

### Custom requirement + handler

```csharp
public record MinimumAgeRequirement(int MinimumAge) : IAuthorizationRequirement;

public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext ctx, MinimumAgeRequirement req)
    {
        var dob = ctx.User.FindFirst(ClaimTypes.DateOfBirth)?.Value;
        if (dob is not null && DateTime.Parse(dob).AddYears(req.MinimumAge) <= DateTime.Today)
            ctx.Succeed(req);
        return Task.CompletedTask;
    }
}

builder.Services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();
builder.Services.AddAuthorization(o =>
    o.AddPolicy("AtLeast21", p => p.Requirements.Add(new MinimumAgeRequirement(21))));
```

Multiple handlers for one requirement = **OR**. Multiple requirements in one policy = **AND**. `ctx.Fail()` forces failure regardless of other handlers.

### Resource-based authorization

```csharp
@inject IAuthorizationService Auth
@code {
    var result = await Auth.AuthorizeAsync(user, document, "DocumentEdit");
    if (!result.Succeeded) return Forbid();
}
```

### Default / fallback policies

```csharp
builder.Services.AddAuthorization(o =>
{
    o.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
```

`FallbackPolicy` applies to any endpoint without an explicit `[Authorize]` or `[AllowAnonymous]`.

---

## 6. Multi-scheme auth (cookie + API key)

Pattern used in this codebase: the default scheme is the Identity cookie; `X-Api-Key` adds a second scheme. A combined policy accepts either.

```csharp
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme)
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        "ApiKey", _ => { });

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("ApiOrCookie", p =>
    {
        p.AuthenticationSchemes = new[] { IdentityConstants.ApplicationScheme, "ApiKey" };
        p.RequireAuthenticatedUser();
    });
});

// Apply to minimal APIs
app.MapGet("/api/products", (...) => ...)
   .RequireAuthorization("ApiOrCookie");
```

The handler's `AuthenticateResult.Success` should attach a `ClaimsPrincipal` that includes role claims so `[Authorize(Roles=...)]` works via API key too. Hash keys (SHA-256) rather than storing plain text.

---

## 7. 2FA / MFA patterns

Identity ships authenticator-app TOTP via `AuthenticatorTokenProvider`. The scaffolded pages (`EnableAuthenticator.razor`) generate a shared key and `otpauth://` URI; you supply the QR code rendering.

```razor
@* EnableAuthenticator.razor — QR code *@
@inject UrlEncoder UrlEncoder
@code {
    string otpauthUri = string.Format(
        "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
        UrlEncoder.Encode("ElementaryApp"),
        UrlEncoder.Encode(email),
        sharedKey);
    // Render via QRCoder or similar:
    // new QRCodeGenerator().CreateQrCode(otpauthUri, ECCLevel.Q) -> PngByteQRCode -> base64
}
```

Login flow: `PasswordSignInAsync` returns `SignInResult.RequiresTwoFactor` → redirect to `/Account/LoginWith2fa`.

```csharp
var result = await signInManager.PasswordSignInAsync(
    email, password, rememberMe, lockoutOnFailure: true);
if (result.RequiresTwoFactor) return Redirect($"/Account/LoginWith2fa?ReturnUrl={returnUrl}");
if (result.IsLockedOut)       return Redirect("/Account/Lockout");
```

Recovery codes are generated via `userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10)` — show once, never retrievable.

**Enforce 2FA by role / policy:**

```csharp
o.AddPolicy("RequireMfa", p =>
    p.RequireAuthenticatedUser()
     .RequireClaim("amr", "mfa"));   // amr claim set by Identity when 2FA succeeds
```

---

## 8. External auth providers (Google, Microsoft)

```csharp
builder.Services.AddAuthentication()
    .AddGoogle(o =>
    {
        o.ClientId     = builder.Configuration["Auth:Google:ClientId"]!;
        o.ClientSecret = builder.Configuration["Auth:Google:ClientSecret"]!;
        o.SignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddMicrosoftAccount(o =>
    {
        o.ClientId     = builder.Configuration["Auth:Microsoft:ClientId"]!;
        o.ClientSecret = builder.Configuration["Auth:Microsoft:ClientSecret"]!;
    })
    .AddGitHub(o => { /* third-party package */ });
```

Store secrets in **User Secrets** (dev) or environment variables / Key Vault (prod) — never `appsettings.json`.

The scaffolded `ExternalLogin.razor` page handles the callback: `SignInManager.GetExternalLoginInfoAsync()` → `ExternalLoginSignInAsync()` → on first login, create the local `IdentityUser` and link via `AddLoginAsync`.

Redirect URIs (Google console / Azure AD): `https://yourapp/signin-google`, `https://yourapp/signin-microsoft`.

---

## 9. Rate limiting strategy

Different partitions for different attack surfaces:

```csharp
builder.Services.AddRateLimiter(o =>
{
    // Anonymous auth endpoints: tight window per IP (brute-force protection)
    o.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window      = TimeSpan.FromMinutes(1),
                QueueLimit  = 0
            }));

    // Authenticated API: generous per-user budget
    o.AddPolicy("api", httpContext =>
        RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: httpContext.User.Identity?.Name
                       ?? httpContext.Connection.RemoteIpAddress?.ToString()
                       ?? "anon",
            factory: _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit        = 100,
                TokensPerPeriod   = 20,
                ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                AutoReplenishment = true,
                QueueLimit        = 0
            }));

    // Global backstop
    o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 300,
                Window      = TimeSpan.FromMinutes(1)
            }));

    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.OnRejected = async (ctx, ct) =>
    {
        if (ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry))
            ctx.HttpContext.Response.Headers.RetryAfter =
                ((int)retry.TotalSeconds).ToString();
        await ctx.HttpContext.Response.WriteAsync("Too many requests.", ct);
    };
});

app.UseRateLimiter();           // after UseRouting

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.MapPost("/Account/Login",  (...) => ...).RequireRateLimiting("auth");
app.MapGroup("/api").RequireAuthorization("ApiOrCookie").RequireRateLimiting("api");
```

**Partition-key warning:** partitioning by client IP is vulnerable to IP spoofing DoS (see RFC 2827). For sensitive endpoints, prefer authenticated identity as the partition key and fall back to IP only for anonymous paths.

Per-Blazor-component override:

```razor
@attribute [EnableRateLimiting("override")]
@attribute [DisableRateLimiting]   // for health probes
```

---

## 10. Audit logging for auth events

Hook the cookie events for login/logout/forbid, and emit structured Serilog events:

```csharp
builder.Services.ConfigureApplicationCookie(o =>
{
    o.Events.OnSignedIn = ctx =>
    {
        ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>()
            .LogInformation("AuthEvent {Event} user={User} ip={Ip}",
                "SignedIn",
                ctx.Principal?.Identity?.Name,
                ctx.HttpContext.Connection.RemoteIpAddress);
        return Task.CompletedTask;
    };
    o.Events.OnSigningOut = ctx => { /* ... */ return Task.CompletedTask; };
});
```

Identity itself logs to `ILogger<SignInManager<TUser>>` / `ILogger<UserManager<TUser>>`. Capture the specific events you need:

| Event | Source | Where |
|---|---|---|
| Login success | `SignInManager` | `"User logged in."` |
| Login failure | `SignInManager` | `"User account locked out."` / `"Invalid login attempt."` |
| Lockout | `UserManager.AccessFailedAsync` | Custom — log before calling |
| Password changed | `Manage/ChangePassword.razor` | `userManager.ChangePasswordAsync` return value |
| 2FA enabled/disabled | `EnableAuthenticator.razor` / `Disable2fa.razor` | Log in page handler |
| External login linked | `ExternalLogins.razor` | `userManager.AddLoginAsync` |
| API key created/revoked | Your `ApiKeys.razor` | Log entity changes |
| Role changed | Any admin page calling `userManager.AddToRoleAsync` | Log + `UpdateSecurityStampAsync` |

Emit to a dedicated `AuditLogs` table (Serilog MSSqlServer sink with a custom logger context) rather than mixing into the generic `RequestLogs`.

---

## 11. Common security mistakes in Blazor apps

1. **Trusting client-side auth checks.** `[Authorize]` on a WASM component is cosmetic. Always re-check on the server endpoint (`RequireAuthorization()`).
2. **Injecting scoped `DbContext` into interactive components.** The circuit outlives any logical operation. Use `IDbContextFactory<T>` (already the rule in this codebase).
3. **MudBlazor inputs inside static-SSR `<EditForm method="post">`.** They don't emit `name`, so model binding silently fails.
4. **Forgetting `[ExcludeFromInteractiveRouting]` on Identity pages.** `SignInManager` cookie writes require static SSR — interactive mode produces "headers already sent" errors.
5. **`[SupplyParameterFromForm]` without `OnInitialized` re-init.** The binder nulls your field initializer between constructor and `OnParametersSet`.
6. **Storing API keys in plain text.** Hash them (SHA-256 minimum) and compare hashes.
7. **Sharing one rate-limit partition across authenticated and anonymous traffic.** Authenticated users end up punished for anonymous abuse. Partition separately.
8. **`DisableAntiforgery()` on cookie-auth endpoints** because "it was easier." Re-opens CSRF for any authenticated user.
9. **Not calling `UpdateSecurityStampAsync` after role / password changes.** Old cookies remain valid until natural expiry.
10. **Exposing `otpauth://` URIs or recovery codes in logs.** Scrub these from Serilog via destructuring policies.
11. **Leaving `Password.RequiredLength = 6` and default iteration count.** 2023+ OWASP guidance is 10+ chars, 210k+ PBKDF2 iterations.
12. **Fallback to IP-partition rate limiting on sensitive endpoints** without IP-spoofing mitigation at the edge (WAF / reverse proxy).
13. **Prerendering a custom `AuthenticationStateProvider` without `IHostEnvironmentAuthenticationStateProvider`.** UI flashes unauthenticated content briefly before the circuit attaches.
14. **Using `[Authorize]` on a non-`@page` component.** The attribute only works at the route level — use `<AuthorizeView>` for in-page authorization.

---

### Reference URLs

- https://learn.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-10.0
- https://learn.microsoft.com/en-us/aspnet/core/blazor/security/server/?view=aspnetcore-10.0
- https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-10.0
- https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-10.0
- https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-10.0
- https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-10.0
