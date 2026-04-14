# Project Conventions

The non-obvious rules that hold this codebase together. Follow these for new code.

## File naming

| Kind | Convention | Example |
|---|---|---|
| Entity | `Singular.cs` PascalCase | `Address.cs` |
| DTO file | `{Entity}Dtos.cs` (contains all DTOs + Mappings) | `AddressDtos.cs` |
| Validator file | `{Entity}Validators.cs` (Create + Update validators in one file) | `AddressValidators.cs` |
| Endpoint group | `{Entity}Endpoints.cs` | `AddressEndpoints.cs` |
| Audit service | `{Entity}AuditService.cs` | `AddressAuditService.cs` |
| Razor page | `Index.razor`, `History.razor`, `{Entity}Dialog.razor` | `AddressDialog.razor` |
| Razor folder | Plural | `Components/Pages/AdventureWorks/Addresses/` |

## DTO conventions

Always `sealed record`. Properties are `init`-only or `set` based on whether they're inputs vs outputs.

```csharp
public sealed record AddressDto(
    int Id, string Line1, string? Line2, string City, int StateProvinceId, string PostalCode);

public sealed record CreateAddressRequest
{
    public string Line1 { get; set; } = "";
    public string? Line2 { get; set; }
    public string City { get; set; } = "";
    public int StateProvinceId { get; set; }
    public string PostalCode { get; set; } = "";
}

public sealed record UpdateAddressRequest
{
    // All properties nullable for patch semantics
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? City { get; set; }
    public int? StateProvinceId { get; set; }
    public string? PostalCode { get; set; }
}
```

Mapping in the same file:

```csharp
public static class AddressMappings
{
    public static AddressDto ToDto(this Address e) => new(e.Id, e.Line1, e.Line2, e.City, e.StateProvinceId, e.PostalCode);
    public static Address ToEntity(this CreateAddressRequest r) => new() { Line1 = r.Line1, /*...*/ };
    public static void ApplyTo(this UpdateAddressRequest r, Address e)
    {
        if (r.Line1 is not null) e.Line1 = r.Line1;
        // ...
    }
}
```

## Endpoint conventions

```csharp
public static class AddressEndpoints
{
    public static IEndpointRouteBuilder MapAddressEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/addresses")
            .WithTags("Addresses")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListAddresses");
        group.MapGet("/{id:int}", GetAsync).WithName("GetAddress");
        group.MapPost("/", CreateAsync).WithName("CreateAddress")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id:int}", UpdateAsync).WithName("UpdateAddress")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id:int}", DeleteAsync).WithName("DeleteAddress")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id:int}/history", HistoryAsync).WithName("ListAddressHistory");

        return app;
    }
    // private static async Task<...> ListAsync(...) { ... }
}
```

Rules:
- Route prefix is `/api/aw/{kebab-plural}`
- Use `RequireAuthorization("ApiOrCookie")` so endpoints work via cookies and API key
- Role checks: Employee+ for create/update, Manager+ for delete
- Use `AsNoTracking()` on read paths
- Wrap entity+audit writes in transactions (or use `AuditedSaveExtensions`)

## Validator conventions

```csharp
public sealed class CreateAddressValidator : AbstractValidator<CreateAddressRequest>
{
    public CreateAddressValidator()
    {
        RuleFor(x => x.Line1).NotEmpty().MaximumLength(60);
        RuleFor(x => x.City).NotEmpty().MaximumLength(30);
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(15);
    }
}

public sealed class UpdateAddressValidator : AbstractValidator<UpdateAddressRequest>
{
    public UpdateAddressValidator()
    {
        // Patch semantics: only validate fields that are actually being patched
        When(x => x.Line1 is not null, () =>
            RuleFor(x => x.Line1!).NotEmpty().MaximumLength(60));
        // ...
    }
}
```

## Component conventions

```razor
@page "/aw/addresses"
@attribute [Authorize]
@inject IDbContextFactory<ApplicationDbContext> DbFactory
@inject IDialogService DialogService
@inject ISnackbar Snackbar
```

Rules:
- Always inject `IDbContextFactory<>` not `ApplicationDbContext` (CLAUDE.md §6)
- Pages must declare `@attribute [Authorize]` if they should require login
- All Identity pages declare `@attribute [ExcludeFromInteractiveRouting]` (CLAUDE.md §1)
- Forms in static SSR pages use Blazor's `<InputText>`/`<InputCheckbox>`/`<InputSelect>`, NEVER MudBlazor inputs (CLAUDE.md §2)
- `[SupplyParameterFromForm]` properties always re-init in `OnInitialized` (CLAUDE.md §3)

## Dialog conventions

```razor
@code {
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public AddressDto? Model { get; set; }

    private bool IsNew => Model is null;
    private bool saving;

    private void Cancel() => MudDialog.Cancel();

    private async Task SaveAsync()
    {
        saving = true;
        try
        {
            // 1. Validate
            // 2. Open DbContext + transaction
            // 3. Save entity, save audit, commit
            // 4. MudDialog.Close(DialogResult.Ok(entity.Id));
        }
        finally { saving = false; }
    }
}
```

## Service conventions

- Singleton lifetime by default
- Inject `IDbContextFactory<ApplicationDbContext>` if DB access needed
- Constructor injection only (no service locator)
- Stateless — if you need state, use `IMemoryCache` or similar

## Audit conventions

- Every entity that supports CRUD has a corresponding `{Entity}AuditLog` table (EF-managed)
- Every entity has a corresponding `{Entity}AuditService` static class with `RecordCreate`, `RecordUpdate`, `RecordDelete`
- Audit writes are atomic with the entity write (transaction)
- Audit logs are pruned daily by `AuditLogCleanupJob` (default 365-day retention)

## Migration conventions

- New EF migration → add an entry to `DatabaseInitializer.MigrationMarkers` with the suffix and a marker table name (CLAUDE.md §4)
- Migrations only for tables that everyone's database needs from day one
- Most new tables go through runtime model diffing (`EnsureMissingTablesAsync`)
- Index-only changes go through `EnsureCompositeIndexesAsync` (idempotent IF NOT EXISTS) — NOT through migrations

## Testing conventions

- Integration tests live in `AWBlazorApp.Tests/`
- Use `WebApplicationFactory<Program>` with `UseEnvironment("Development")`
- Disable rate limiting in the shared test factory (`Features:RateLimiting=false`)
- Use `FormPostHelper.PostFormAsync` for form-based tests
- Always clean up test data — tests share `AdventureWorks2022_dev`

## Commit message conventions

Title under 70 chars. Body explains *why*. Group related changes per commit. Examples in `git log`.

End every commit with:
```
Co-Authored-By: Claude Opus 4.6 (1M context) <noreply@anthropic.com>
```

## What NEVER to do

- Don't reintroduce SQLite, ServiceStack, Vue, Tailwind, or NPM
- Don't put MudBlazor inputs in static SSR forms
- Don't inject scoped `ApplicationDbContext` into Blazor components
- Don't bypass the audit service when modifying audited entities
- Don't commit secrets to `appsettings.json` — use User Secrets or env vars
- Don't disable antiforgery without documenting why
- Don't add `[Authorize]` to non-`@page` components
- Don't modify `dbo.ToolSlotConfigurations` schema without coordinating with DBA
