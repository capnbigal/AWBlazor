# Adding a new feature

Checklist for starting a new feature (e.g. `Features/Invoicing/`).

## 1. Create the feature folder

```
Features/Invoicing/
├── Components/
│   ├── Pages/
│   └── Shared/             # only if you have feature-only widgets
├── Domain/
├── Endpoints/
├── Services/
├── Models/
└── Validators/
```

Not every feature needs every subfolder — omit what you don't use. Add an
`Audit/` subfolder if entities have per-field audit logging.

## 2. Namespaces match folders

- `AWBlazorApp.Features.Invoicing.Domain`
- `AWBlazorApp.Features.Invoicing.Services`
- `AWBlazorApp.Features.Invoicing.Endpoints`
- etc.

## 3. Wire it up

- **Entities:** add `DbSet<T>` to `Infrastructure/Persistence/ApplicationDbContext.cs`.
- **Endpoints:** add a `MapXxxEndpoints` extension; call it from
  `App/Routing/EndpointMappingExtensions.cs`.
- **Hangfire jobs:** register in `App/Extensions/MiddlewarePipeline.cs` (for
  recurring) or let the feature's page code enqueue per-request.
- **Services:** register in `App/Extensions/ServiceRegistration.cs`.
- **Imports:** if pages use types from the feature, add
  `@using AWBlazorApp.Features.Invoicing.Domain` etc. to
  `Components/_Imports.razor` and `Features/_Imports.razor`
  (both locations — the second covers pages under `Features/`).
- **Nav:** add a `MudNavLink` to `Shared/Components/Layout/NavMenu.razor`,
  typically inside `<AuthorizeView>` blocks for the right role.
- **Global search:** add entries to `Shared/Components/Widgets/GlobalSearch.razor`
  if the feature has top-level pages users should find.

## 4. Tests

Add test files to `AWBlazorApp.Tests/`. Integration tests follow the
existing pattern using `WebApplicationFactory<Program>`. Form-post helpers
live in `FormPostHelper.cs`.

## 5. Before you commit

- `dotnet build AWBlazorApp.slnx` — clean build
- `dotnet test AWBlazorApp.slnx` — all tests pass
- Feature branch + PR per the rule in
  `memory/feedback_branching_workflow.md`

## What does NOT go in a feature folder

- Cross-feature widgets → `Shared/Components/Widgets/`
- Base entities like `AuditableEntity` → `Shared/Domain/`
- DbContext, migrations, auth plumbing → `Infrastructure/`
- Composition / middleware → `App/`
- Generated code → `Scaffold/`
