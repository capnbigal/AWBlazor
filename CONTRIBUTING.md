# Contributing to AWBlazorApp

## Prerequisites

- .NET 10 SDK
- SQL Server instance named `ELITE` with `AdventureWorks2022` database
- Windows authentication access to SQL Server
- Git

## Quick start

```pwsh
git clone https://github.com/capnbigal/AWBlazor.git
cd AWBlazor
dotnet restore AWBlazorApp.slnx
dotnet build AWBlazorApp.slnx
dotnet test AWBlazorApp.slnx
dotnet run --project src/AWBlazorApp
```

Open https://localhost:5001/

## Seed users

| Email | Password | Roles |
|---|---|---|
| test@email.com | p@55wOrd | (none) |
| employee@email.com | p@55wOrd | Employee |
| manager@email.com | p@55wOrd | Manager, Employee |
| admin@email.com | p@55wOrd | Admin, Manager, Employee |

## Code style

- 4 spaces indentation (no tabs)
- File-scoped namespaces
- Top-level statements in Program.cs
- Startup extensions in `App/Extensions/` (`ServiceRegistration.cs`, `MiddlewarePipeline.cs`); endpoint mapping in `App/Routing/EndpointMappingExtensions.cs`
- Entities in `Features/<Feature>/<Entity>/Domain/`, DTOs in `Features/<Feature>/<Entity>/Dtos/`, validators in `Features/<Feature>/<Entity>/Application/Validators/`
- One endpoint group per entity in `Features/<Feature>/<Entity>/Api/`
- Blazor pages in `Features/<Feature>/<Entity>/UI/Pages/`

## Branch naming

- `feature/description` for new features
- `fix/description` for bug fixes
- `refactor/description` for refactoring
- `phase-N-description` for phase work

## Commit messages

Use descriptive commit messages. Include Co-Authored-By for AI-assisted commits.

## Adding a new entity CRUD page

1. Create entity in `Features/{Feature}/{Entity}/Domain/`
2. Add `DbSet` to `Infrastructure/Persistence/ApplicationDbContext.cs`
3. Create DTOs in `Features/{Feature}/{Entity}/Dtos/` with `ToDto()` / `ToEntity()` mappings
4. Create FluentValidation validators in `Features/{Feature}/{Entity}/Application/Validators/`
5. Create Minimal API endpoints in `Features/{Feature}/{Entity}/Api/`
6. Register endpoints in `App/Routing/EndpointMappingExtensions.cs`
7. Create Blazor page in `Features/{Feature}/{Entity}/UI/Pages/Index.razor`
8. Create dialog in `Features/{Feature}/{Entity}/UI/Pages/{Entity}Dialog.razor`
9. Add nav link in `Shared/UI/Layout/NavMenu.razor`
10. Add to global search in `Shared/UI/Components/GlobalSearch.razor`
11. Run `dotnet ef migrations add {Name}` from `src/AWBlazorApp/`

## Testing

Tests run against real SQL Server (ELITE/AdventureWorks2022_dev). Hangfire and Serilog SQL sink are disabled in tests via in-memory config overrides.

```pwsh
dotnet test AWBlazorApp.slnx
dotnet test AWBlazorApp.slnx --filter "FullyQualifiedName~Login_Form_Post"
```

## Key architectural patterns

- `IDbContextFactory<ApplicationDbContext>` for Blazor components (not scoped DbContext)
- `[ExcludeFromInteractiveRouting]` on Identity pages (static SSR for cookie writes)
- `FkSelect` / `FkAutocomplete` for FK dropdown fields
- `LookupService` for cached lookup data
- `AnalyticsCacheService` for analytics dashboard caching
- `PermissionArea` + `AreaPermissionMiddleware` for data-level authorization
