# ElementaryApp — Migration phase plan

## Status

| Phase | Scope | Status |
|---|---|---|
| **Phase 1** | Foundation: csproj, Program.cs, MudBlazor, EF Core entities, AuditingInterceptor, project collapse from 4 → 2 | DONE |
| **Phase 2** | DTOs in `Models/`, FluentValidation in `Validators/`, Minimal-API endpoints in `Endpoints/`, Swashbuckle Swagger UI | DONE |
| **Phase 3** | MudBlazor CRUD pages (Bookings, Coupons, ToolSlots, Admin, Users), `IDbContextFactory` for components, FluentValidation→MudForm adapter, Identity scaffold rebuild | DONE |
| **Phase 4** | SQL Server (SHOOSHEE / AdventureWorks2022), Hangfire, Serilog request log, API Keys (entity + auth handler + UI), Markdig blog, all the post-deploy patches (migration reconciler, schema patcher, missing-table creator, render-mode fix, MudBlazor SSR form fix, ToolSlotConfiguration column mapping) | DONE |
| **Phase 5** | Form-POST integration tests, API key auth tests, restored 2FA / ExternalLogins / PersonalData pages, project README | DONE |

**The original 5-phase migration is complete.** Phases 6 onwards in this document are
forward-looking — they capture work that was intentionally deferred during the migration
plus the production-readiness items you'd naturally want before going live.

Each phase below is **independent** and can be tackled in any order. The "Prerequisite" notes
point out the few cases where one phase needs another to be done first.

---

## Phase 6 — Deployment & CI cleanup

**Why this phase exists:** the `.kamal/` and `.github/workflows/` files are still the
ServiceStack-era originals. They reference packages, secrets, and project structures that no
longer exist. They will silently rot until someone tries to deploy and discovers a build failure.

**Risk level:** medium. Deployment configs are easy to break in ways that only show up at deploy
time. Don't tackle this without confirming whether you actually use Kamal / GitHub Actions and
having a way to test the deploy.

**Prerequisite:** confirm whether you're actively using Kamal and which GitHub workflows fire on
push. If you're not using either, the cleanest answer is to delete them.

### Tasks

- [ ] **Decide: keep, rewrite, or delete** the deployment infrastructure
  - `.kamal/secrets` references `SERVICESTACK_LICENSE`, `KAMAL_REGISTRY_USERNAME`, `KAMAL_REGISTRY_PASSWORD`, `APPSETTINGS_JSON_BASE64`
  - `.github/workflows/build.yml` runs `dotnet restore/build/test` against the (no longer existing) ServiceModel/ServiceInterface project structure
  - `.github/workflows/build-container.yml` builds a Docker image and references a Node `ElementaryApp.Client` project that no longer exists
  - `.github/workflows/release.yml` (haven't read it yet, probably similar)
- [ ] If keeping Kamal:
  - Drop `SERVICESTACK_LICENSE` from `.kamal/secrets`
  - Add `ConnectionStrings__DefaultConnection` and `Smtp__*` env vars to whatever `.kamal/deploy.yml` uses
  - Update Dockerfile if there is one (the Phase 1 csproj had a `<PublishProfile>DefaultContainer</PublishProfile>` reference that I removed; the .NET 10 SDK has built-in container publishing via `dotnet publish /t:PublishContainer`)
- [ ] If keeping GitHub Actions:
  - Update `build.yml` to target `ElementaryApp.slnx` (not the old per-project paths) and remove the Node setup steps
  - Update or remove `build-container.yml` based on whether you still want container artifacts
  - Add a step that runs the tests (`dotnet test ElementaryApp.slnx`)
  - Consider replacing `actions/checkout@v3` with `@v4`, `actions/setup-dotnet@v3` with `@v4`, etc. — they're all stale
- [ ] **Initialize git** (the repo is currently NOT under version control)
  - `git init`
  - Create `.gitignore` for .NET (the existing one references the ServiceStack-era projects)
  - Initial commit
  - Push to a remote of your choice
- [ ] Confirm the existing `.gitignore` excludes `App_Data/`, `bin/`, `obj/`, `.vs/`

**Estimated effort:** 1-2 hours if you have a deploy target to test against. Half a day if you
have to verify against a real environment.

---

## Phase 7 — Identity completeness

**Why this phase exists:** I deliberately built the minimum viable Identity surface in Phases 1-5.
A few features were skipped because they require external dependencies or non-trivial extra
endpoints. If you ever need them in production, this phase covers the work.

**Risk level:** low to medium. These are additive — none of them break existing functionality.

### 7a — External authentication providers (Google, Microsoft, GitHub, etc.)

Currently `ExternalLogins.razor` shows an empty list and a placeholder explaining that no
providers are configured. To enable real external auth:

- [ ] Pick the provider(s) you want and install their NuGet packages
  - Google: `Microsoft.AspNetCore.Authentication.Google`
  - Microsoft / Entra: `Microsoft.AspNetCore.Authentication.MicrosoftAccount`
  - GitHub: `AspNet.Security.OAuth.GitHub` (community)
- [ ] Register them in `Program.cs`:
  ```csharp
  services.AddAuthentication()
      .AddGoogle(options => { options.ClientId = ...; options.ClientSecret = ...; })
      .AddMicrosoftAccount(options => { ... })
      .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(...);
  ```
- [ ] Restore the form-handler endpoints in `IdentityComponentsEndpointRouteBuilderExtensions.cs`:
  - `MapPost("/PerformExternalLogin", ...)` — initiates the OAuth challenge from the Login page
  - `MapPost("/Manage/LinkExternalLogin", ...)` — links a new provider to a signed-in user
  - `ExternalLogin.razor` page (currently deleted) — handles the OAuth callback. Restore from the standard .NET 9 scaffold.
- [ ] Store provider secrets in user secrets / environment variables, never in `appsettings.json`
- [ ] Add an integration test that verifies the OAuth challenge is issued correctly (you can't end-to-end test the actual provider response without mocking)

### 7b — 2FA QR code rendering

Currently `EnableAuthenticator.razor` shows the manual setup key + the `otpauth://` URI.
Mobile authenticator apps can parse the URI directly when opened on a phone, but a proper QR
code is friendlier.

- [ ] Install `QRCoder` NuGet package (BSD-style license, no JS dependency)
- [ ] In `EnableAuthenticator.razor`, generate a PNG via `QRCoder.QRCodeGenerator` + `PngByteQRCode` and render as `<img src="data:image/png;base64,...">`
- [ ] Test on iOS Authenticator + Microsoft Authenticator + Google Authenticator
- [ ] Update the README "Things explicitly NOT in this project" section to remove the QR code item

### 7c — Email confirmation flow tests

Currently the registration flow generates a confirmation token and emails a callback link
(via the Hangfire-backed SmtpEmailSender), but there's no integration test that walks the
full flow.

- [ ] Add an integration test that:
  1. Registers a new user via `POST /Account/Register`
  2. Reads the captured email from a test `IEmailSender<ApplicationUser>` registered in
     `ConfigureTestServices` (replace the production sender with one that captures messages
     in-memory)
  3. Extracts the confirmation link from the captured email
  4. Issues a GET against the link
  5. Asserts the user's `EmailConfirmed` flag is now true

**Estimated effort:** 7a is a half-day if you only want one provider. 7b is an hour. 7c is
an hour or two depending on how much you wire into the test sender.

---

## Phase 8 — Observability & operations

**Why this phase exists:** the migration was about replacing ServiceStack, not adding observability
features. For production you'll want healthchecks, structured logging enrichment, and a way to
see what's happening from outside the application.

**Risk level:** low. All additive.

### Tasks

- [ ] **HealthChecks**
  - Install `Microsoft.Extensions.Diagnostics.HealthChecks` (in the BCL, no NuGet) +
    `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` +
    `AspNetCore.HealthChecks.SqlServer`
  - Register checks for: `ApplicationDbContext` (EF can-connect), `Hangfire` (dashboard reachable), `SmtpClient` (TCP probe to the SMTP host)
  - Map `/healthz` (anonymous) for liveness/readiness probes — returns 200 when all checks pass
  - Map `/healthz/details` (Admin only) for a JSON dump of every check's status, useful in dev
- [ ] **Serilog enrichers**
  - `Serilog.Enrichers.Environment` — adds `MachineName`, `EnvironmentName`
  - `Serilog.Enrichers.ClientInfo` — adds `ClientIp`, `ClientAgent`
  - Custom `IUserNameEnricher` that adds the current user's email/UserId to every log entry inside an authenticated request
- [ ] **Request log browser improvements**
  - Add filter inputs (status code, level, date range) to `Components/Pages/Admin/RequestLog.razor`
  - Add a "View details" dialog showing the full structured log entry as JSON
  - Add a delete-old-rows scheduled Hangfire job (e.g. retain 30 days)
- [ ] **Hangfire dashboard polish**
  - Already mounted at `/hangfire` (Admin only). Add some simple recurring jobs as examples (e.g. a daily cleanup job that prunes old request logs).
  - Configure Hangfire's job retention settings via `BackgroundJobServerOptions`
- [ ] **Application metrics**
  - Optional: add `OpenTelemetry.Extensions.Hosting` + `OpenTelemetry.Instrumentation.AspNetCore` + `OpenTelemetry.Instrumentation.EntityFrameworkCore` to expose a `/metrics` endpoint compatible with Prometheus
  - Wire to a dashboarding tool (Grafana, Datadog) if you have one

**Estimated effort:** healthchecks + Serilog enrichers is half a day. The full set with
OpenTelemetry is a day or two.

---

## Phase 9 — AI chat (Anthropic) — OPTIONAL

**Why this phase exists:** you explicitly skipped Anthropic AI chat during Phase 4. If you ever
want it back, here's the path. Skip this whole phase if AI chat isn't part of your roadmap.

**Risk level:** low. Self-contained feature.

### Tasks

- [ ] Install the official Anthropic SDK: `dotnet add package Anthropic` (NOT the community
      `Anthropic.SDK` package — the official one has the same package name as the .NET 9 era)
- [ ] Add `Data/Entities/ChatConversation.cs` and `ChatMessage.cs`:
  - `ChatConversation`: Id, UserId, Title, CreatedDate
  - `ChatMessage`: Id, ConversationId, Role (user/assistant/system), Content, CreatedDate
- [ ] Add `DbSet<ChatConversation>` and `DbSet<ChatMessage>` to `ApplicationDbContext` and
      generate a new EF migration
- [ ] Create `Services/AnthropicChatService.cs` that wraps `IAnthropicClient` and persists each
      turn to the DB
- [ ] Create `Components/Pages/Chat/Index.razor` (interactive server) with a MudBlazor chat UI:
  - Conversation list sidebar
  - Message bubble list
  - Streaming responses via `client.Messages.CreateStreaming(...)`
- [ ] Add `Anthropic:ApiKey` and `Anthropic:Model` to `appsettings.json`
- [ ] Document the Anthropic API key setup in the README
- [ ] Update the README "Things explicitly NOT in this project" section to remove the AI chat item

**Estimated effort:** one or two days depending on how polished you want the chat UI.

---

## Phase 10 — Hardening & security review

**Why this phase exists:** the migration focused on functional parity. Production deployment
needs explicit attention to attack surface, rate limiting, and OWASP fundamentals.

**Risk level:** medium. Some changes (rate limiting, security headers) can break existing
functionality if misconfigured.

### Tasks

- [ ] **Rate limiting** — use the built-in `Microsoft.AspNetCore.RateLimiting` middleware
  - Apply a global limit on all `/api/*` endpoints (e.g. 100 req/minute per IP)
  - Apply a stricter limit on `/Account/Login`, `/Account/Register`, `/Account/ForgotPassword` (e.g. 5 req/minute per IP) to make brute force harder
  - Per-API-key limits via the `partitionKey` selector
- [ ] **Security headers** — add a small middleware or use `NetEscapades.AspNetCore.SecurityHeaders`
  - `Content-Security-Policy` — restrict to self + MudBlazor's inline styles
  - `Strict-Transport-Security` — already handled by `UseHsts()` in production
  - `X-Frame-Options: DENY`
  - `X-Content-Type-Options: nosniff`
  - `Referrer-Policy: strict-origin-when-cross-origin`
- [ ] **CORS** — currently no CORS policy is configured because everything is same-origin. If
  you ever want to allow API key clients from a different origin, add an explicit policy
- [ ] **Antiforgery audit** — verify every form POST has `<AntiforgeryToken />` (the form-POST
  tests should catch any that don't, but a manual sweep is worth doing)
- [ ] **Secrets audit** — make sure no real credentials are in `appsettings.json`. Move the
  SQL Server password (if/when you switch from Trusted_Connection), SMTP credentials, and
  Hangfire dashboard authorization to user secrets / environment variables
- [ ] **Password policy review** — currently `IdentityCore` uses default settings:
  - Required length: 6
  - Required digit: yes
  - Required lowercase: yes
  - Required uppercase: yes
  - Required non-alphanumeric: yes
  - Adjust via `services.AddIdentityCore<>(options => { options.Password.RequiredLength = 12; ... })` if your security policy needs stronger
- [ ] **API key rotation policy** — currently keys never expire. Consider:
  - Default `ExpiresDate` to `CreatedDate + 1 year`
  - Add a reminder banner on `/Account/Manage/ApiKeys` for keys expiring in <30 days
  - Add a Hangfire job that emails users when their keys are about to expire
- [ ] **Audit logging** — every Booking/Coupon/ApiKey CRUD operation already populates audit
  fields via `AuditingInterceptor`. Consider also writing a separate `AuditLog` table for
  high-value events (login, password change, API key generation, role grant)

**Estimated effort:** rate limiting + security headers is half a day. The full security
review is a multi-day effort with manual testing.

---

## Phase 11 — Performance & scale

**Why this phase exists:** the app currently handles small datasets fine. If your real
`AdventureWorks2022.dbo.ToolSlotConfigurations` has 100k+ rows, the MudDataGrid pagination
will work but the table-scan queries will get slow. This phase is the "make it fast" pass.

**Risk level:** low. All optimizations.

### Tasks

- [ ] **EF query profiling**
  - Enable Serilog at Information level for `Microsoft.EntityFrameworkCore.Database.Command` in
    development
  - Walk every page and check the SQL output for: full table scans on indexed columns,
    accidental N+1 patterns, query result sets bigger than the page
- [ ] **Index review** on the SQL Server side
  - Add indexes to `Bookings(CreatedDate)`, `Bookings(DeletedDate)` (already declared in EF
    but excluded from migrations for ToolSlotConfigurations — verify they're created on
    the real Bookings table)
  - Add an index to `ApiKeys(Key)` (unique, already declared)
  - Run SQL Server's missing-index DMV (`sys.dm_db_missing_index_details`) after a few days
    of real usage and add the suggested indexes
- [ ] **MudDataGrid virtualization**
  - Add `Virtualize="true"` to grids that might display thousands of rows
  - Test that ServerData loading still pages correctly with virtualization on
- [ ] **Output / response caching**
  - Add `[OutputCache]` to the Hello endpoint (it's anonymous, deterministic)
  - Add `[ResponseCache]` to static-content responses
  - Configure cache profiles in `services.AddResponseCaching()`
- [ ] **Connection pooling**
  - Currently the SQL Server connection string doesn't tune the pool. For high concurrency
    add `Max Pool Size=200` (or appropriate for your environment) and configure
    `services.AddDbContextFactory<>(opts => opts.UseSqlServer(connStr, sql => sql.MaxBatchSize(...)))`
- [ ] **Hangfire SQL Server tuning**
  - Adjust `QueuePollInterval`, `JobExpirationCheckInterval`, `CountersAggregateInterval`
    via `SqlServerStorageOptions` if the dashboard shows queue lag

**Estimated effort:** half a day for the basics. A full week if you do real load testing.

---

## Phase 12 — Documentation & contributor onboarding

**Why this phase exists:** the README from Phase 5 is the user-facing documentation. This
phase adds the developer-facing material for anyone who has to extend the codebase later.

**Risk level:** zero. All documentation.

### Tasks

- [ ] **Architecture Decision Records** in `docs/adr/`
  - 0001 — Why Blazor Server (interactive) instead of WebAssembly
  - 0002 — Why MudBlazor (vs alternatives like Radzen, FluentUI, Ant Design Blazor)
  - 0003 — Why API Keys are stored in plain text (vs hashed)
  - 0004 — Why ToolSlotConfigurations is excluded from EF migrations
  - 0005 — Why Identity scaffold pages are static SSR while everything else is interactive
  - 0006 — Why we use [InputText] in Identity forms instead of MudBlazor inputs
- [ ] **`CONTRIBUTING.md`**
  - How to run the app locally
  - How to run tests
  - Code style: 4 spaces, file-scoped namespaces, top-level statements in Program.cs, etc.
  - Branch naming convention
  - Commit message style (conventional commits, semantic prefix, whatever you prefer)
- [ ] **`docs/api.md`** — generated from the Swagger spec via `dotnet swagger tofile` or similar
- [ ] **`.github/ISSUE_TEMPLATE/`** with bug-report and feature-request templates
- [ ] **`.github/PULL_REQUEST_TEMPLATE.md`**
- [ ] **Local dev setup script**
  - `scripts/setup.ps1` (or `setup.sh`) that:
    - Verifies .NET 10 SDK is installed
    - Restores packages
    - Verifies SHOOSHEE is reachable (or prompts for an alternate connection string)
    - Runs the migrations + seed
    - Opens the app
- [ ] **`docs/troubleshooting.md`** — common issues + their fixes, mostly drawing from the
  gotchas captured in the README's "Architecture notes" section

**Estimated effort:** ADRs are a few hours each. The full doc set is a couple of days.

---

## Quick reference: priority + risk matrix

| Phase | Priority | Risk | Blocked by | Skip if... |
|---|---|---|---|---|
| 6 — Deployment & CI | high if deploying soon | medium | confirm Kamal usage | not deploying |
| 7a — External logins | medium | medium | none | local-only auth is fine |
| 7b — 2FA QR code | low | low | none | manual key is acceptable |
| 7c — Email confirmation tests | low | low | none | already tested manually |
| 8 — Observability | high before production | low | none | dev-only |
| 9 — AI chat | optional | low | none | not in roadmap |
| 10 — Hardening | high before production | medium | none | internal-only app |
| 11 — Performance | low until scale matters | low | real usage data | small dataset |
| 12 — Documentation | low but cumulative | none | none | solo developer |

---

## Things explicitly NOT planned

- **Reintroducing Vue / Tailwind / NPM tooling** — this was the entire point of the migration
- **Reintroducing ServiceStack** — same
- **Switching back to SQLite for production** — SQL Server is the locked-in choice per
  `memory/project_database_target.md`
- **Switching the UI library away from MudBlazor** — locked in per
  `memory/project_migration_status.md`
- **Adding a separate API project** — the Minimal API endpoints live alongside the Blazor
  host in the same csproj. This is intentional to keep deployment simple. If the API ever
  needs to scale independently, that's a different architectural conversation.

---

*Last updated: end of Phase 5. Next update: when Phase 6 work is started.*
