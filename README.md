# ElementaryApp

A Blazor Server application running on .NET 10 with EF Core and MudBlazor, backed by SQL Server.

Originally a ServiceStack + Vue template; migrated to a pure open-source .NET stack across five
phases. The current state is documented below.

## Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 |
| Web | ASP.NET Core / Blazor Web App (Server interactive + static SSR) |
| UI | MudBlazor 9.x |
| ORM | EF Core 10 (SQL Server provider) |
| Identity | ASP.NET Core Identity (cookies + custom API key scheme) |
| Validation | FluentValidation 12 |
| OpenAPI | Swashbuckle.AspNetCore 10 |
| Background jobs | Hangfire 1.8 (SQL Server storage) |
| Logging | Serilog 10 (Console + MSSqlServer sink for request logs) |
| Markdown | Markdig 1.1 + YamlDotNet 16 |
| Testing | NUnit 4 + Microsoft.AspNetCore.Mvc.Testing + EF Core SQLite (in-memory) |

## Prerequisites

- **.NET 10 SDK**
- **SQL Server instance** named `SHOOSHEE` reachable from your dev machine, with a database
  named `AdventureWorks2022`. Adjust `ConnectionStrings:DefaultConnection` in
  `appsettings.json` (or via user secrets) if your server is named differently.
- **Windows authentication** access to the SQL Server (the default connection string uses
  `Trusted_Connection=True`). If you need SQL auth, change the connection string accordingly.

## Quick start

```pwsh
git clone <this repo>
cd ElementaryApp
dotnet restore ElementaryApp.slnx
dotnet build  ElementaryApp.slnx
dotnet test   ElementaryApp.slnx       # 18 integration + unit tests
dotnet run --project ElementaryApp
```

Then open `https://localhost:5001/`.

## First-run database behavior

The app talks to **SHOOSHEE / AdventureWorks2022**. On the first start, `DatabaseInitializer`
in `ElementaryApp/Data/DatabaseInitializer.cs` runs four steps in order:

1. **`ReconcileMigrationHistoryAsync`** — if your database already contains tables that one of
   our EF migrations would create (e.g. you ran an earlier prerelease and the `AspNetRoles`
   table is already present), it stamps the relevant migration as already-applied in
   `__EFMigrationsHistory` so `MigrateAsync` doesn't crash with "object already exists."
2. **`MigrateAsync`** — applies any genuinely-pending EF migrations.
3. **`EnsureMissingTablesAsync`** — uses `IMigrationsModelDiffer` to create any tables defined
   in the model that don't yet exist in the database. This handles the partial-state case
   where a stamped migration's primary marker table existed but its other tables didn't.
4. **`PatchMissingColumnsAsync`** — walks every entity in the design-time model, queries
   `INFORMATION_SCHEMA.COLUMNS` for each existing table, and `ALTER TABLE ADD`s any nullable
   columns the model expects but the live table is missing. NOT NULL columns are logged as
   errors and must be added manually.
5. **`SeedAsync`** — creates Identity roles, the four seed users below, and reference data
   (10 booking coupons, 3 sample bookings).

The `dbo.ToolSlotConfigurations` table is **excluded from migrations** (`ExcludeFromMigrations`
in `ApplicationDbContext.OnModelCreating`). EF reads from and writes to it but never tries to
create, alter, or drop it. The C# entity uses `[Column(...)]` attributes to map property names
to the real database column casing (`Id` ↔ `CID`, `MtCode` ↔ `MT_CODE`, etc.).

### Seed users

| Email | Password | Roles |
|---|---|---|
| `test@email.com` | `p@55wOrd` | (none) |
| `employee@email.com` | `p@55wOrd` | Employee |
| `manager@email.com` | `p@55wOrd` | Manager, Employee |
| `admin@email.com` | `p@55wOrd` | Admin, Manager, Employee |

## Configuration

| Setting | Default | Purpose |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | `Server=SHOOSHEE;Database=AdventureWorks2022;Trusted_Connection=True;TrustServerCertificate=True` | Production database (also used by Hangfire and the Serilog request-log sink) |
| `Smtp:Host` | empty | When empty, registration / forgot-password emails are logged but not delivered. Set to enable real SMTP. |
| `Smtp:Port`, `Smtp:Username`, `Smtp:Password`, `Smtp:EnableSsl`, `Smtp:FromEmail`, `Smtp:FromName` | various | Standard SMTP settings |
| `Smtp:DevToEmail` | null | If set, all outbound mail is redirected here (useful for dev) |
| `Smtp:Bcc` | null | If set, all outbound mail is BCC'd here |
| `Features:Hangfire` | `true` | Set to `false` to skip Hangfire registration entirely (used by tests so they don't need a real SQL Server reachable) |
| `RequestLogs:Enabled` | `true` | Set to `false` to disable the Serilog MSSqlServer sink (used by tests) |

Use user secrets in development to keep your SMTP credentials out of source control:

```pwsh
cd ElementaryApp
dotnet user-secrets set "Smtp:Host" "smtp.example.com"
dotnet user-secrets set "Smtp:Username" "myuser"
dotnet user-secrets set "Smtp:Password" "mypassword"
```

## Endpoints / pages

| Path | Notes |
|---|---|
| `/` | Home page |
| `/blog` | Markdown blog index (reads `_posts/*.md`) |
| `/blog/{slug}` | Single blog post |
| `/bookings` | Bookings CRUD (any authenticated user) |
| `/coupons` | Coupons CRUD (any authenticated user) |
| `/tool-slots` | Tool slot configurations CRUD (any authenticated user) |
| `/admin` | Admin dashboard (Admin role) |
| `/admin/users` | Identity user list (Admin role) |
| `/admin/request-log` | Browse the Serilog `RequestLogs` table (Admin role) |
| `/hangfire` | Hangfire dashboard (Admin role; only mounted when `Features:Hangfire=true`) |
| `/swagger` | Swagger UI for the REST API (Development environment only) |
| `/Account/Login`, `/Account/Register`, `/Account/ForgotPassword`, etc. | Identity scaffold pages (static SSR) |
| `/Account/Manage` | Profile + linked sub-pages: Email, Password, Two-factor, External logins, Personal data, API keys |

### REST API

| Method | Route | Auth |
|---|---|---|
| `GET` | `/api/hello/{name}` | anonymous |
| `GET` | `/api/admin/data` | Admin |
| `GET POST PATCH DELETE` | `/api/bookings` | cookie or `X-Api-Key` |
| `GET POST PATCH DELETE` | `/api/coupons` | cookie or `X-Api-Key` |
| `GET POST PATCH DELETE` | `/api/tool-slots` | cookie or `X-Api-Key` |
| `GET` | `/api/users`, `/api/users/{id}` | Admin |

API keys can be generated by any signed-in user from `/Account/Manage/ApiKeys`. They authenticate
via the `X-Api-Key: ek_...` header and inherit the owning user's roles.

## Project layout

```
ElementaryApp/
├── Authentication/             ApiKeyAuthenticationHandler, HangfireDashboardAuthFilter
├── Components/
│   ├── Account/                Identity scaffold pages (all static SSR)
│   ├── Layout/                 MainLayout + NavMenu
│   └── Pages/                  Home, Blog, Bookings, Coupons, ToolSlots, Admin
├── Data/
│   ├── Entities/               Booking, Coupon, ToolSlotConfiguration, ApiKey, AuditableEntity
│   ├── ApplicationDbContext.cs
│   ├── ApplicationUser.cs      Extends IdentityUser with FirstName/LastName/DisplayName/ProfileUrl
│   ├── AuditingInterceptor.cs  Populates audit fields via SaveChangesInterceptor
│   ├── DatabaseInitializer.cs  Migrate/reconcile/patch/seed pipeline
│   └── Migrations/             EF Core migrations (SQL Server)
├── Endpoints/                  Minimal-API endpoint groups
├── Models/                     Request/response DTOs (no entity coupling)
├── Validators/                 FluentValidation rules + MudFormValidator adapter
├── Services/                   MarkdownBlogService, SmtpEmailJob, HangfireSmtpEmailSender, SmtpConfig
├── _posts/                     Markdown blog posts (read at startup)
├── _pages, _includes, _videos/ Other content folders (currently unused; reserved for future)
├── wwwroot/
│   ├── img/                    Static images
│   └── css/account-forms.css   Shared styling for the static-SSR Identity forms
├── Program.cs
└── ElementaryApp.csproj

ElementaryApp.Tests/
├── IntegrationTest.cs          18 tests covering pages, endpoints, form POSTs, API keys
├── FormPostHelper.cs           GET → parse antiforgery → POST helper
├── UnitTest.cs                 Standalone EF unit test
└── ElementaryApp.Tests.csproj  Includes Microsoft.EntityFrameworkCore.Sqlite for in-memory testing
```

## Architecture notes

### Render mode strategy

`App.razor` uses **conditional render mode**:

```razor
<HeadOutlet @rendermode="@PageRenderMode" />
<Routes @rendermode="@PageRenderMode" />

@code {
    [CascadingParameter] private HttpContext HttpContext { get; set; } = default!;
    private IComponentRenderMode? PageRenderMode
        => HttpContext.AcceptsInteractiveRouting() ? InteractiveServer : null;
}
```

Most pages render with **Interactive Server** (so MudBlazor's interactive features work).
Identity scaffold pages each declare `@attribute [ExcludeFromInteractiveRouting]`, which makes
`AcceptsInteractiveRouting()` return false for them — they render as **static SSR**, which is
required for Identity form POSTs to work correctly (Identity's `SignInManager` writes the auth
cookie via the HTTP response, which is only available outside the interactive circuit).

### MudBlazor inputs in static SSR forms — DON'T

This is the biggest gotcha in this codebase: **MudBlazor input components (`MudTextField`,
`MudCheckBox`, `MudSelect`, etc.) do not emit the HTML `name` attribute** that Blazor's
static-SSR form binder needs to populate `[SupplyParameterFromForm]` properties. If you put
them inside an `<EditForm method="post">` on a statically-rendered page (any page with
`[ExcludeFromInteractiveRouting]`), every form POST will fail validation with "field required"
because the form data never makes it to the model.

**Use Blazor's built-in `<InputText>`, `<InputCheckbox>`, `<InputSelect>` inside SSR forms
instead.** They auto-emit `name="Input.Email"` derived from the `@bind-Value` expression. The
shared CSS in `wwwroot/css/account-forms.css` styles them with MudBlazor's CSS variables so
they look reasonably consistent with the rest of the app.

`MudPaper`, `MudContainer`, `MudText`, `MudButton`, `MudDivider`, `MudStack`, `MudLink`,
`MudAlert`, `MudChip` are all fine to use inside static SSR forms — only the `<input>`-emitting
components are the problem.

### `[SupplyParameterFromForm]` + property initializers

In static SSR, the framework binder calls the property setter on every render — not just on
POSTs — and sets the property to `null` when there's no matching form data. The field
initializer (`= new()`) survives the constructor but gets clobbered by the binder before
`OnParametersSet` runs, which crashes `EditForm.OnParametersSet` with "Model parameter
required."

**Always re-initialize form-bound properties in `OnInitialized`:**

```csharp
[SupplyParameterFromForm]
private InputModel Input { get; set; } = new();

protected override void OnInitialized() => Input ??= new();
```

This is the BL0008 analyzer warning's actual practical fix.

### `IdentityRedirectManager` and the `NavigationException` debugger noise

`IdentityRedirectManager.RedirectTo` calls `NavigationManager.NavigateTo`, which throws
`NavigationException` as Blazor's intentional **control-flow signal** to abort the current
render and trigger an HTTP 302 (or client-side navigation in interactive mode). The framework
catches it; the user never sees an error.

But Visual Studio's "Just My Code" debugger pauses execution on first-chance exceptions thrown
from user code, and the throw happens inside `IdentityRedirectManager.RedirectTo`. The class
is annotated with `[DebuggerNonUserCode]` on every `RedirectTo*` method so Just My Code treats
the throw as framework code and skips the break.

If you ever see VS pop up on a `NavigationException` here, either:
- Press F5/Continue — the redirect is already happening and the user's flow completes correctly
- Or check Tools → Options → Debugging → General → ensure "Enable Just My Code" is checked
- Or open Debug → Windows → Exception Settings, search "NavigationException", and uncheck

## Tests

```pwsh
dotnet test ElementaryApp.slnx
```

The test fixture spins up a `WebApplicationFactory<Program>` with the production EF Core
SQL Server registration **replaced** by a SQLite-in-memory connection. `Features:Hangfire`
and `RequestLogs:Enabled` are set to `false` so the test host doesn't try to reach a real
SQL Server. `DatabaseInitializer` detects the non-SQL-Server provider and falls back to
`EnsureCreatedAsync` instead of running migrations.

Coverage as of Phase 5:

- Page renders: `/Account/Login`, `/Account/Register`, `/Account/ForgotPassword`
- Anonymous redirects from `/bookings`, `/coupons`, `/tool-slots`, `/admin`, `/admin/users`
- `/api/hello/{name}` returns greeting JSON
- `/api/bookings` returns 401 for anonymous request
- Swagger spec is served and contains all CRUD groups
- **Form POST**: valid login credentials → redirect; invalid login → re-render with error;
  forgot-password → redirect to confirmation
- **API key auth**: valid key returns booking data; invalid key → 401; revoked key → 401
- Standalone unit test: Booking ↔ Coupon FK relationship via in-memory DbContext

The form-POST tests use `FormPostHelper.PostFormAsync`, which does the standard browser flow
(GET → parse antiforgery token → POST). This is the only kind of test that catches MudBlazor
SSR form-binding regressions — page-render tests alone are not enough.

## Things explicitly NOT in this project

These were considered and intentionally left out:

- **Anthropic AI chat** — was originally part of Phase 4 but skipped at the request of the
  user. To re-add, install the `Anthropic` NuGet package, register a `ChatService` against
  `IAnthropicClient`, and add a Blazor chat UI with InteractiveServer rendering.
- **External login providers (Google, Microsoft, GitHub, etc.)** — the code path is referenced
  in `ExternalLogins.razor` but the form-handler endpoints are not wired up. To enable, add
  `services.AddAuthentication().AddGoogle(...)` etc. in `Program.cs` and restore the
  `MapPost("/PerformExternalLogin", ...)` and `MapPost("/Manage/LinkExternalLogin", ...)`
  endpoints in `IdentityComponentsEndpointRouteBuilderExtensions.cs`.
- **2FA QR code rendering** — the EnableAuthenticator page shows the manual setup key and the
  `otpauth://` URI but does not render a QR code. Most authenticator apps can parse the URI
  directly when opened on a mobile device. To add proper QR codes, install `qrcode.js` (or a
  server-side library like `QRCoder`) and render the URI as an image.
- **Tailwind / Vue / NPM** — all removed during Phase 1. Don't reintroduce them; this app
  uses MudBlazor for UI and has no JS toolchain.

## Known migration history

This repo started as the ServiceStack `blazor-vue` template and was migrated to its current
state across five phases:

1. **Phase 1** — Foundation: csproj/Program.cs/layouts, MudBlazor, EF Core entities,
   collapsed 4 projects → 2.
2. **Phase 2** — DTOs + Minimal API endpoints + FluentValidation + Swashbuckle.
3. **Phase 3** — MudBlazor CRUD pages + Identity scaffold rebuild + role-aware nav.
4. **Phase 4** — SQL Server switch + Hangfire + Serilog + API Keys + Markdig blog.
5. **Phase 5** — Form-POST tests, API key tests, restored 2FA/ExternalLogins/PersonalData
   pages, this README.

The original Vue/Tailwind/ServiceStack files have all been removed. `_posts/`, `_pages/`,
`_includes/`, `_videos/` are kept as content folders for the markdown blog system but the
original ServiceStack-aware markdown rendering pipeline is gone — `Services/MarkdownBlogService.cs`
is a lightweight 120-line replacement.
