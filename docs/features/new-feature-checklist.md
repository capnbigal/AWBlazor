# New Feature Checklist

A reusable checklist for adding a new feature module to AWBlazorApp. Copy this into your PR description and tick items as you go.

## Planning

- [ ] Feature documented in `docs/features/backlog.md` or in a new `docs/features/{feature-name}.md`
- [ ] User stories / acceptance criteria written
- [ ] Major design decisions captured as ADRs in `docs/adr/`

## Branch + scope

- [ ] Branch created off `main`: `feature/{name}` or `refactor/{name}`
- [ ] Scope is bounded — if it grows, split into multiple PRs

## Domain

- [ ] Entity classes in `Data/Entities/{Module}/`
  - [ ] `[Table]` attribute (schema if non-`dbo`)
  - [ ] `[Key]`, `[MaxLength]`, `[Required]` attributes
  - [ ] No business methods (anemic by convention)
- [ ] DbSet added to `ApplicationDbContext`
- [ ] `OnModelCreating` configures relationships, indexes
- [ ] If it's a brand-new table → decide migration vs runtime diffing
- [ ] If it's an externally-managed table → `ExcludeFromMigrations()` + manual `[Column]` mapping
- [ ] Audit log entity (`{Entity}AuditLog`) if mutable

## Services

- [ ] `{Entity}AuditService` static class with `RecordCreate`, `RecordUpdate`, `RecordDelete`
- [ ] Any business logic in `Services/` (not in components or endpoints)
- [ ] Service registered in `Startup/ServiceRegistration.cs` with appropriate lifetime

## DTOs

- [ ] `Models/{Module}/{Entity}Dtos.cs` with:
  - [ ] `{Entity}Dto` (sealed record)
  - [ ] `Create{Entity}Request` (sealed record, all required props non-nullable)
  - [ ] `Update{Entity}Request` (sealed record, all props nullable for patch semantics)
  - [ ] `{Entity}Mappings` static class with `ToDto`, `ToEntity`, `ApplyTo` methods

## Validation

- [ ] `Validators/{Module}/{Entity}Validators.cs` with:
  - [ ] `Create{Entity}Validator : AbstractValidator<Create{Entity}Request>`
  - [ ] `Update{Entity}Validator : AbstractValidator<Update{Entity}Request>` (patch semantics)
- [ ] Auto-discovered via `AddValidatorsFromAssemblyContaining<Program>()`
- [ ] Unit tests for validators (`*ValidatorTests.cs`)

## API endpoints

- [ ] `Endpoints/{Module}/{Entity}Endpoints.cs` with 6 handlers:
  - [ ] `GET /api/{path}` — paged list + filters
  - [ ] `GET /api/{path}/{id}` — single
  - [ ] `POST /api/{path}` — create (Employee+ role)
  - [ ] `PATCH /api/{path}/{id}` — update (Employee+ role)
  - [ ] `DELETE /api/{path}/{id}` — delete (Manager+ role)
  - [ ] `GET /api/{path}/{id}/history` — audit log
- [ ] Group has `RequireAuthorization("ApiOrCookie")`
- [ ] `AsNoTracking()` on read paths
- [ ] Audit + entity writes wrapped in transaction (or use `AuditedSaveExtensions`)
- [ ] Registration added to `EndpointMappingExtensions.cs`

## Blazor UI

- [ ] `Components/Pages/{Module}/{Plural}/Index.razor` — list page with filters + grid + actions
- [ ] `Components/Pages/{Module}/{Plural}/{Entity}Dialog.razor` — create/edit dialog
- [ ] `Components/Pages/{Module}/{Plural}/History.razor` — audit log viewer
- [ ] `@attribute [Authorize]` on routable pages
- [ ] `IDbContextFactory<>` injected (NOT scoped DbContext)
- [ ] No MudBlazor inputs in static SSR forms (use `<InputText>` etc.)
- [ ] `aria-label` on every icon-only button
- [ ] Loading state via `<MudProgressLinear>` or skeleton
- [ ] Empty state via `<EmptyState>` shared component
- [ ] Confirmation dialog for destructive actions
- [ ] Snackbar feedback after save/delete

## Navigation

- [ ] Link added to `Components/Layout/NavMenu.razor`
- [ ] Item added to `Components/Shared/GlobalSearch.razor` if user-discoverable

## Permissions

- [ ] If new permission area: enum entry in `Data/PermissionArea.cs`
- [ ] Mapping added to `Data/PermissionAreaMapping.cs`

## Tests

- [ ] Smoke test added to `AWBlazorApp.Tests/ApiSmokeTests.cs` if it's an `/api/aw/*` endpoint
- [ ] Form POST test added if it's an Identity-style page
- [ ] Critical workflow integration test in `IntegrationTest.cs`
- [ ] Validator unit tests
- [ ] All tests pass: `dotnet test AWBlazorApp.slnx`

## Documentation

- [ ] Page-level help in `_pages/` if the feature warrants user docs
- [ ] Blog post in `_posts/` if announcing
- [ ] CLAUDE.md updated if new architectural pattern introduced
- [ ] Conventions doc updated if new naming convention
- [ ] ADR added if major design decision

## Final checks

- [ ] `dotnet build` — 0 errors
- [ ] `dotnet test` — all green
- [ ] Code review by another developer (or ask Claude for a review)
- [ ] No new package vulnerabilities (`dotnet list package --vulnerable`)
- [ ] Manual smoke test in dev (run app, click around, verify)
- [ ] Dark mode looks right
- [ ] Mobile viewport looks reasonable

## PR + merge

- [ ] PR description includes summary + test plan + screenshots if UI changes
- [ ] Squash or rebase as needed for clean history
- [ ] Merged to `main` after CI passes
- [ ] Branch deleted after merge
