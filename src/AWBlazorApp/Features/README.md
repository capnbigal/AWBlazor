# Features

One folder per business domain / feature. Each `Features/<Name>/` folder
owns everything for that feature:

```
Features/<Name>/
├── Components/
│   ├── Pages/            # Blazor pages with @page routes
│   └── Shared/           # feature-only widgets
├── Domain/               # entities
├── Endpoints/            # minimal API (group per feature)
├── Services/             # application services / use cases
├── Audit/                # per-entity audit logic (optional)
├── Models/               # DTOs
└── Validators/           # FluentValidation
```

Rules:

- **If ≥2 features use a thing, it belongs in `Shared/`**, not here.
- **If it talks to SQL, SMTP, Hangfire, or external auth, it belongs in
  `Infrastructure/`.**
- **Namespaces match folders**: `AWBlazorApp.Features.Sales.Domain`,
  `AWBlazorApp.Features.Sales.Services`, etc.
- **Pages keep their existing `@page` routes** across moves so URLs don't break.
