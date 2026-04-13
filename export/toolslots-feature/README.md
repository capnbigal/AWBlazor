# ToolSlots Feature — Drop-in for Blazor CRUD Template

## What's included

| File | Purpose |
|------|---------|
| `Entities/ToolSlotConfiguration.cs` | EF Core entity mapping to external `dbo.ToolSlotConfigurations` table |
| `Entities/ToolSlotAuditLog.cs` | Audit log entity (EF-managed) tracking create/update/delete |
| `Models/ToolSlotConfigurationDtos.cs` | DTOs + mapping extensions (ToDto, ToEntity, ApplyTo) |
| `Validators/ToolSlotConfigurationValidators.cs` | FluentValidation for create/update |
| `Endpoints/ToolSlotConfigurationEndpoints.cs` | Minimal API: GET/POST/PATCH/DELETE at `/api/tool-slots` |
| `Services/ToolSlotAuditService.cs` | Static audit service (creates diffs, snapshots) |
| `Components/Pages/ToolSlots/Index.razor` | MudDataGrid CRUD page with server-side paging/sorting/filtering |
| `Components/Pages/ToolSlots/ToolSlotDialog.razor` | MudDialog create/edit form with FluentValidation |
| `Components/Pages/ToolSlots/History.razor` | Audit history viewer with per-record filtering |

## Shared infrastructure (may already be in your template)

| File | Purpose | Skip if you already have... |
|------|---------|----------------------------|
| `AppRoles.cs` | Static role constants (Admin, Manager, Employee) | A roles class |
| `AuditingInterceptor.cs` | EF SaveChanges interceptor for audit fields | An audit interceptor |
| `Validators/MudFormValidator.cs` | Bridges FluentValidation → MudBlazor MudForm | MudForm validation |

## Prerequisites

Your target app must have:
- .NET 10+ with Blazor Server (Interactive)
- **MudBlazor 9** (`dotnet add package MudBlazor`)
- **FluentValidation** (`dotnet add package FluentValidation.DependencyInjectionExtensions`)
- **EF Core 10 with SQL Server** (`dotnet add package Microsoft.EntityFrameworkCore.SqlServer`)
- ASP.NET Core Identity (for `[Authorize]` and `ClaimsPrincipal`)

## Integration steps

### 1. Copy files into your project

```
YourApp/
├── Data/
│   ├── Entities/
│   │   ├── ToolSlotConfiguration.cs    ← copy, update namespace
│   │   └── ToolSlotAuditLog.cs         ← copy, update namespace
│   ├── AppRoles.cs                     ← copy or merge with yours
│   └── AuditingInterceptor.cs          ← copy if you don't have one
├── Models/
│   └── ToolSlotConfigurationDtos.cs    ← copy, update namespace
├── Validators/
│   ├── ToolSlotConfigurationValidators.cs  ← copy, update namespace
│   └── MudFormValidator.cs             ← copy if you don't have one
├── Endpoints/
│   └── ToolSlotConfigurationEndpoints.cs   ← copy, update namespace
├── Services/
│   └── ToolSlotAuditService.cs         ← copy, update namespace
└── Components/Pages/ToolSlots/
    ├── Index.razor                     ← copy
    ├── ToolSlotDialog.razor            ← copy
    └── History.razor                   ← copy
```

### 2. Update namespaces

Find/replace in all copied files:
- `AWBlazorApp.Data.Entities` → `YourApp.Data.Entities`
- `AWBlazorApp.Data` → `YourApp.Data`
- `AWBlazorApp.Models` → `YourApp.Models`
- `AWBlazorApp.Validators` → `YourApp.Validators`
- `AWBlazorApp.Endpoints` → `YourApp.Endpoints`
- `AWBlazorApp.Services` → `YourApp.Services`

### 3. Register in your DbContext

Add to your `ApplicationDbContext`:

```csharp
public DbSet<ToolSlotConfiguration> ToolSlotConfigurations => Set<ToolSlotConfiguration>();
public DbSet<ToolSlotAuditLog> ToolSlotAuditLogs => Set<ToolSlotAuditLog>();
```

Add to `OnModelCreating`:

```csharp
// ToolSlotConfigurations — pre-existing external table, not managed by EF migrations
builder.Entity<ToolSlotConfiguration>(b =>
{
    b.ToTable("ToolSlotConfigurations", t => t.ExcludeFromMigrations());
    b.HasIndex(x => new { x.Family, x.MtCode, x.Destination });
});

// ToolSlotAuditLog — EF-managed
builder.Entity<ToolSlotAuditLog>(b =>
{
    b.HasIndex(x => x.ToolSlotConfigurationId);
});
```

### 4. Register the endpoint

In your endpoint mapping (Program.cs or extension method):

```csharp
app.MapToolSlotConfigurationEndpoints();
```

### 5. Register validators (if not auto-discovered)

If your app uses `AddValidatorsFromAssemblyContaining<Program>()`, the validators are auto-discovered. Otherwise:

```csharp
services.AddTransient<IValidator<CreateToolSlotConfigurationRequest>, CreateToolSlotConfigurationValidator>();
services.AddTransient<IValidator<UpdateToolSlotConfigurationRequest>, UpdateToolSlotConfigurationValidator>();
services.AddTransient(typeof(MudFormValidator<>));
```

### 6. Add authorization policy

The endpoints require an `"ApiOrCookie"` policy. Add to your auth config:

```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("ApiOrCookie", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddAuthenticationSchemes(IdentityConstants.ApplicationScheme);
    });
});
```

### 7. Add navigation link

In your NavMenu:

```razor
<MudNavLink Href="tool-slots" Icon="@Icons.Material.Filled.Build">Tool Slots</MudNavLink>
```

### 8. Generate the audit log migration

```bash
dotnet ef migrations add AddToolSlotAuditLogs
```

Note: The `ToolSlotConfigurations` table itself is NOT created by EF — it must already exist in your database (DBA-managed). Only the `ToolSlotAuditLogs` table is created via migration.

### 9. Ensure _Imports.razor has required usings

```razor
@using Microsoft.EntityFrameworkCore
@using MudBlazor
@using YourApp.Data
@using YourApp.Models
```

## Database requirements

The `dbo.ToolSlotConfigurations` table must exist with these columns:

| C# Property | SQL Column | Type |
|-------------|-----------|------|
| Id | CID | int (PK, identity) |
| MtCode | MT_CODE | nvarchar(20) |
| Family | FAMILY | nvarchar(50) |
| Destination | DESTINATION | nvarchar(50) |
| Fcl1-4 | FCL1-FCL4 | nvarchar(50) each |
| Ffl1-2 | FFL1-FFL2 | nvarchar(50) each |
| Ffr1-4 | FFR1-FFR4 | nvarchar(50) each |
| Rcl1 | RCL1 | nvarchar(50) |
| Rcr1-2 | RCR1-RCR2 | nvarchar(50) each |
| Rfl1 | RFL1 | nvarchar(50) |
| Rfr1-2 | RFR1-RFR2 | nvarchar(50) each |
| IsActive | IsActive | bit |

## Features included

- **Server-side MudDataGrid** with paging, sorting, and text search filter
- **Create/edit dialog** with FluentValidation and MudForm integration
- **Audit trail** — every create/update/delete recorded with who, when, and what changed
- **History viewer** — browse all audit logs or filter by specific configuration
- **Role-based access** — Employee+ can create/edit, Manager+ can delete
- **Confirmation dialogs** on delete with error handling for FK violations
- **Snackbar notifications** on all CRUD operations
