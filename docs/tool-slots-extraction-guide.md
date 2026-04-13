# Tool Slots Feature Extraction Guide

Instructions for copying the Tool Slots page, audit history, and supporting code from AWBlazor into a separate Blazor / .NET 10 / MudBlazor project.

## Prerequisites

Your target project must have these NuGet packages installed:

```xml
<PackageReference Include="MudBlazor" Version="9.*" />
<PackageReference Include="FluentValidation" Version="12.*" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="12.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.*" />
```

Your target project must already have MudBlazor configured in `Program.cs` (`AddMudServices()`, `UseMudStaticResources()`, etc.) and a working `DbContext` registered via `AddDbContextFactory<>`.

---

## Step 1: Create the SQL tables

### 1a. `dbo.ToolSlotConfigurations` (the source table)

This table is DBA-owned in AWBlazor and excluded from EF migrations. You need to create it manually in your database:

```sql
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ToolSlotConfigurations')
CREATE TABLE dbo.ToolSlotConfigurations (
    CID          INT IDENTITY(1,1) PRIMARY KEY,
    FAMILY       NVARCHAR(255) NULL,
    MT_CODE      NVARCHAR(255) NULL,
    DESTINATION  NVARCHAR(255) NULL,
    FCL1         NVARCHAR(255) NULL,
    FCL2         NVARCHAR(255) NULL,
    FCR1         NVARCHAR(255) NULL,
    FFL1         NVARCHAR(255) NULL,
    FFL2         NVARCHAR(255) NULL,
    FFR1         NVARCHAR(255) NULL,
    FFR2         NVARCHAR(255) NULL,
    FFR3         NVARCHAR(255) NULL,
    FFR4         NVARCHAR(255) NULL,
    RCL1         NVARCHAR(255) NULL,
    RCR1         NVARCHAR(255) NULL,
    RCR2         NVARCHAR(255) NULL,
    RFL1         NVARCHAR(255) NULL,
    RFR1         NVARCHAR(255) NULL,
    RFR2         NVARCHAR(255) NULL,
    IsActive     BIT NOT NULL DEFAULT 1
);
```

### 1b. `dbo.ToolSlotAuditLogs` (EF-managed)

This table will be created by EF migrations (Step 5), but here's the SQL if you prefer manual creation:

```sql
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ToolSlotAuditLogs')
CREATE TABLE dbo.ToolSlotAuditLogs (
    Id                        INT IDENTITY(1,1) PRIMARY KEY,
    ToolSlotConfigurationId   INT NOT NULL,
    Action                    NVARCHAR(16) NOT NULL,
    ChangedBy                 NVARCHAR(256) NULL,
    ChangedDate               DATETIME2 NOT NULL,
    ChangeSummary             NVARCHAR(2048) NULL,
    Family       NVARCHAR(255) NULL,
    MtCode       NVARCHAR(255) NULL,
    Destination  NVARCHAR(255) NULL,
    Fcl1         NVARCHAR(255) NULL,
    Fcl2         NVARCHAR(255) NULL,
    Fcr1         NVARCHAR(255) NULL,
    Ffl1         NVARCHAR(255) NULL,
    Ffl2         NVARCHAR(255) NULL,
    Ffr1         NVARCHAR(255) NULL,
    Ffr2         NVARCHAR(255) NULL,
    Ffr3         NVARCHAR(255) NULL,
    Ffr4         NVARCHAR(255) NULL,
    Rcl1         NVARCHAR(255) NULL,
    Rcr1         NVARCHAR(255) NULL,
    Rcr2         NVARCHAR(255) NULL,
    Rfl1         NVARCHAR(255) NULL,
    Rfr1         NVARCHAR(255) NULL,
    Rfr2         NVARCHAR(255) NULL,
    IsActive     BIT NOT NULL
);

CREATE INDEX IX_ToolSlotAuditLogs_ToolSlotConfigurationId
    ON dbo.ToolSlotAuditLogs (ToolSlotConfigurationId);
CREATE INDEX IX_ToolSlotAuditLogs_ChangedDate
    ON dbo.ToolSlotAuditLogs (ChangedDate);
```

---

## Step 2: Copy entity classes

### 2a. `Data/Entities/ToolSlotConfiguration.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourApp.Data.Entities;

[Table("ToolSlotConfigurations")]
public class ToolSlotConfiguration
{
    [Key]
    [Column("CID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("FAMILY")]
    [MaxLength(255)]
    public string? Family { get; set; }

    [Column("MT_CODE")]
    [MaxLength(255)]
    public string? MtCode { get; set; }

    [Column("DESTINATION")]
    [MaxLength(255)]
    public string? Destination { get; set; }

    [Column("FCL1")][MaxLength(255)] public string? Fcl1 { get; set; }
    [Column("FCL2")][MaxLength(255)] public string? Fcl2 { get; set; }
    [Column("FCR1")][MaxLength(255)] public string? Fcr1 { get; set; }
    [Column("FFL1")][MaxLength(255)] public string? Ffl1 { get; set; }
    [Column("FFL2")][MaxLength(255)] public string? Ffl2 { get; set; }
    [Column("FFR1")][MaxLength(255)] public string? Ffr1 { get; set; }
    [Column("FFR2")][MaxLength(255)] public string? Ffr2 { get; set; }
    [Column("FFR3")][MaxLength(255)] public string? Ffr3 { get; set; }
    [Column("FFR4")][MaxLength(255)] public string? Ffr4 { get; set; }
    [Column("RCL1")][MaxLength(255)] public string? Rcl1 { get; set; }
    [Column("RCR1")][MaxLength(255)] public string? Rcr1 { get; set; }
    [Column("RCR2")][MaxLength(255)] public string? Rcr2 { get; set; }
    [Column("RFL1")][MaxLength(255)] public string? Rfl1 { get; set; }
    [Column("RFR1")][MaxLength(255)] public string? Rfr1 { get; set; }
    [Column("RFR2")][MaxLength(255)] public string? Rfr2 { get; set; }

    [Column("IsActive")]
    public bool IsActive { get; set; }
}
```

**Important:** The `[Column]` attributes map PascalCase C# properties to the uppercase/snake_case SQL columns (`CID`, `MT_CODE`, `FAMILY`, etc.). These are required because the SQL table uses non-standard naming.

### 2b. `Data/Entities/ToolSlotAuditLog.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace YourApp.Data.Entities;

public class ToolSlotAuditLog
{
    public int Id { get; set; }
    public int ToolSlotConfigurationId { get; set; }

    [MaxLength(16)]
    public string Action { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? ChangedBy { get; set; }

    public DateTime ChangedDate { get; set; }

    [MaxLength(2048)]
    public string? ChangeSummary { get; set; }

    // Snapshot columns
    [MaxLength(255)] public string? Family { get; set; }
    [MaxLength(255)] public string? MtCode { get; set; }
    [MaxLength(255)] public string? Destination { get; set; }
    [MaxLength(255)] public string? Fcl1 { get; set; }
    [MaxLength(255)] public string? Fcl2 { get; set; }
    [MaxLength(255)] public string? Fcr1 { get; set; }
    [MaxLength(255)] public string? Ffl1 { get; set; }
    [MaxLength(255)] public string? Ffl2 { get; set; }
    [MaxLength(255)] public string? Ffr1 { get; set; }
    [MaxLength(255)] public string? Ffr2 { get; set; }
    [MaxLength(255)] public string? Ffr3 { get; set; }
    [MaxLength(255)] public string? Ffr4 { get; set; }
    [MaxLength(255)] public string? Rcl1 { get; set; }
    [MaxLength(255)] public string? Rcr1 { get; set; }
    [MaxLength(255)] public string? Rcr2 { get; set; }
    [MaxLength(255)] public string? Rfl1 { get; set; }
    [MaxLength(255)] public string? Rfr1 { get; set; }
    [MaxLength(255)] public string? Rfr2 { get; set; }
    public bool IsActive { get; set; }
}
```

---

## Step 3: Register in your DbContext

Add the `DbSet` properties and `OnModelCreating` configuration to your existing `DbContext`:

```csharp
// DbSet properties
public DbSet<ToolSlotConfiguration> ToolSlotConfigurations => Set<ToolSlotConfiguration>();
public DbSet<ToolSlotAuditLog> ToolSlotAuditLogs => Set<ToolSlotAuditLog>();
```

In `OnModelCreating`:

```csharp
// ToolSlotConfigurations is externally managed — EF reads/writes but never creates/alters/drops.
builder.Entity<ToolSlotConfiguration>(b =>
{
    b.ToTable("ToolSlotConfigurations", t => t.ExcludeFromMigrations());
    b.HasIndex(x => new { x.Family, x.MtCode, x.Destination });
});

// ToolSlotAuditLogs IS EF-managed. No FK to ToolSlotConfigurations because that table
// is excluded from migrations; we just store the integer id.
builder.Entity<ToolSlotAuditLog>(b =>
{
    b.HasIndex(x => x.ToolSlotConfigurationId);
    b.HasIndex(x => x.ChangedDate);
});
```

**Key point:** `ExcludeFromMigrations()` means EF will never try to CREATE or ALTER the `ToolSlotConfigurations` table. If you want EF to manage both tables, remove that call and remove the `[Column]` attributes (or rename your SQL columns to match PascalCase).

---

## Step 4: Copy DTOs and mapping extensions

### 4a. `Models/ToolSlotConfigurationDtos.cs`

```csharp
using YourApp.Data.Entities;

namespace YourApp.Models;

public sealed record ToolSlotConfigurationDto(
    int Id,
    string? Family, string? MtCode, string? Destination,
    string? Fcl1, string? Fcl2, string? Fcr1,
    string? Ffl1, string? Ffl2,
    string? Ffr1, string? Ffr2, string? Ffr3, string? Ffr4,
    string? Rcl1,
    string? Rcr1, string? Rcr2,
    string? Rfl1,
    string? Rfr1, string? Rfr2,
    bool IsActive);

public sealed record CreateToolSlotConfigurationRequest
{
    public string? Family { get; set; }
    public string? MtCode { get; set; }
    public string? Destination { get; set; }
    public string? Fcl1 { get; set; }
    public string? Fcl2 { get; set; }
    public string? Fcr1 { get; set; }
    public string? Ffl1 { get; set; }
    public string? Ffl2 { get; set; }
    public string? Ffr1 { get; set; }
    public string? Ffr2 { get; set; }
    public string? Ffr3 { get; set; }
    public string? Ffr4 { get; set; }
    public string? Rcl1 { get; set; }
    public string? Rcr1 { get; set; }
    public string? Rcr2 { get; set; }
    public string? Rfl1 { get; set; }
    public string? Rfr1 { get; set; }
    public string? Rfr2 { get; set; }
    public bool IsActive { get; set; }
}

public sealed record UpdateToolSlotConfigurationRequest
{
    public string? Family { get; set; }
    public string? MtCode { get; set; }
    public string? Destination { get; set; }
    public string? Fcl1 { get; set; }
    public string? Fcl2 { get; set; }
    public string? Fcr1 { get; set; }
    public string? Ffl1 { get; set; }
    public string? Ffl2 { get; set; }
    public string? Ffr1 { get; set; }
    public string? Ffr2 { get; set; }
    public string? Ffr3 { get; set; }
    public string? Ffr4 { get; set; }
    public string? Rcl1 { get; set; }
    public string? Rcr1 { get; set; }
    public string? Rcr2 { get; set; }
    public string? Rfl1 { get; set; }
    public string? Rfr1 { get; set; }
    public string? Rfr2 { get; set; }
    public bool? IsActive { get; set; }
}

public static class ToolSlotConfigurationMappings
{
    public static ToolSlotConfigurationDto ToDto(this ToolSlotConfiguration t) => new(
        t.Id, t.Family, t.MtCode, t.Destination,
        t.Fcl1, t.Fcl2, t.Fcr1,
        t.Ffl1, t.Ffl2,
        t.Ffr1, t.Ffr2, t.Ffr3, t.Ffr4,
        t.Rcl1, t.Rcr1, t.Rcr2,
        t.Rfl1, t.Rfr1, t.Rfr2,
        t.IsActive);

    public static ToolSlotConfiguration ToEntity(this CreateToolSlotConfigurationRequest r) => new()
    {
        Family = r.Family, MtCode = r.MtCode, Destination = r.Destination,
        Fcl1 = r.Fcl1, Fcl2 = r.Fcl2, Fcr1 = r.Fcr1,
        Ffl1 = r.Ffl1, Ffl2 = r.Ffl2,
        Ffr1 = r.Ffr1, Ffr2 = r.Ffr2, Ffr3 = r.Ffr3, Ffr4 = r.Ffr4,
        Rcl1 = r.Rcl1, Rcr1 = r.Rcr1, Rcr2 = r.Rcr2,
        Rfl1 = r.Rfl1, Rfr1 = r.Rfr1, Rfr2 = r.Rfr2,
        IsActive = r.IsActive,
    };

    public static void ApplyTo(this UpdateToolSlotConfigurationRequest r, ToolSlotConfiguration t)
    {
        if (r.Family is not null) t.Family = r.Family;
        if (r.MtCode is not null) t.MtCode = r.MtCode;
        if (r.Destination is not null) t.Destination = r.Destination;
        if (r.Fcl1 is not null) t.Fcl1 = r.Fcl1;
        if (r.Fcl2 is not null) t.Fcl2 = r.Fcl2;
        if (r.Fcr1 is not null) t.Fcr1 = r.Fcr1;
        if (r.Ffl1 is not null) t.Ffl1 = r.Ffl1;
        if (r.Ffl2 is not null) t.Ffl2 = r.Ffl2;
        if (r.Ffr1 is not null) t.Ffr1 = r.Ffr1;
        if (r.Ffr2 is not null) t.Ffr2 = r.Ffr2;
        if (r.Ffr3 is not null) t.Ffr3 = r.Ffr3;
        if (r.Ffr4 is not null) t.Ffr4 = r.Ffr4;
        if (r.Rcl1 is not null) t.Rcl1 = r.Rcl1;
        if (r.Rcr1 is not null) t.Rcr1 = r.Rcr1;
        if (r.Rcr2 is not null) t.Rcr2 = r.Rcr2;
        if (r.Rfl1 is not null) t.Rfl1 = r.Rfl1;
        if (r.Rfr1 is not null) t.Rfr1 = r.Rfr1;
        if (r.Rfr2 is not null) t.Rfr2 = r.Rfr2;
        if (r.IsActive.HasValue) t.IsActive = r.IsActive.Value;
    }
}
```

### 4b. `Models/ToolSlotAuditLogDtos.cs`

```csharp
using YourApp.Data.Entities;

namespace YourApp.Models;

public sealed record ToolSlotAuditLogDto(
    int Id,
    int ToolSlotConfigurationId,
    string Action,
    string? ChangedBy,
    DateTime ChangedDate,
    string? ChangeSummary,
    string? Family, string? MtCode, string? Destination,
    string? Fcl1, string? Fcl2, string? Fcr1,
    string? Ffl1, string? Ffl2,
    string? Ffr1, string? Ffr2, string? Ffr3, string? Ffr4,
    string? Rcl1,
    string? Rcr1, string? Rcr2,
    string? Rfl1,
    string? Rfr1, string? Rfr2,
    bool IsActive);

public static class ToolSlotAuditLogMappings
{
    public static ToolSlotAuditLogDto ToDto(this ToolSlotAuditLog a) => new(
        a.Id, a.ToolSlotConfigurationId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Family, a.MtCode, a.Destination,
        a.Fcl1, a.Fcl2, a.Fcr1,
        a.Ffl1, a.Ffl2,
        a.Ffr1, a.Ffr2, a.Ffr3, a.Ffr4,
        a.Rcl1, a.Rcr1, a.Rcr2,
        a.Rfl1, a.Rfr1, a.Rfr2,
        a.IsActive);
}
```

---

## Step 5: Copy the audit service

### `Services/ToolSlotAuditService.cs`

```csharp
using System.Text;
using YourApp.Data.Entities;

namespace YourApp.Services;

public static class ToolSlotAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static ToolSlotSnapshot Snapshot(ToolSlotConfiguration slot) => new(slot);

    public static ToolSlotAuditLog RecordCreate(ToolSlotConfiguration slot, string? changedBy)
        => BuildLog(slot, ActionCreated, changedBy, changeSummary: "Created");

    public static ToolSlotAuditLog RecordUpdate(ToolSlotSnapshot before, ToolSlotConfiguration after, string? changedBy)
        => BuildLog(after, ActionUpdated, changedBy, changeSummary: BuildDiffSummary(before, after));

    public static ToolSlotAuditLog RecordDelete(ToolSlotConfiguration slot, string? changedBy)
        => BuildLog(slot, ActionDeleted, changedBy, changeSummary: "Deleted");

    private static ToolSlotAuditLog BuildLog(
        ToolSlotConfiguration slot, string action, string? changedBy, string? changeSummary)
        => new()
        {
            ToolSlotConfigurationId = slot.Id,
            Action = action,
            ChangedBy = changedBy,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = Truncate(changeSummary, 2048),
            Family = slot.Family, MtCode = slot.MtCode, Destination = slot.Destination,
            Fcl1 = slot.Fcl1, Fcl2 = slot.Fcl2, Fcr1 = slot.Fcr1,
            Ffl1 = slot.Ffl1, Ffl2 = slot.Ffl2,
            Ffr1 = slot.Ffr1, Ffr2 = slot.Ffr2, Ffr3 = slot.Ffr3, Ffr4 = slot.Ffr4,
            Rcl1 = slot.Rcl1, Rcr1 = slot.Rcr1, Rcr2 = slot.Rcr2,
            Rfl1 = slot.Rfl1, Rfr1 = slot.Rfr1, Rfr2 = slot.Rfr2,
            IsActive = slot.IsActive,
        };

    private static string BuildDiffSummary(ToolSlotSnapshot before, ToolSlotConfiguration after)
    {
        var sb = new StringBuilder();
        AppendIfChanged(sb, "Family", before.Family, after.Family);
        AppendIfChanged(sb, "MtCode", before.MtCode, after.MtCode);
        AppendIfChanged(sb, "Destination", before.Destination, after.Destination);
        AppendIfChanged(sb, "Fcl1", before.Fcl1, after.Fcl1);
        AppendIfChanged(sb, "Fcl2", before.Fcl2, after.Fcl2);
        AppendIfChanged(sb, "Fcr1", before.Fcr1, after.Fcr1);
        AppendIfChanged(sb, "Ffl1", before.Ffl1, after.Ffl1);
        AppendIfChanged(sb, "Ffl2", before.Ffl2, after.Ffl2);
        AppendIfChanged(sb, "Ffr1", before.Ffr1, after.Ffr1);
        AppendIfChanged(sb, "Ffr2", before.Ffr2, after.Ffr2);
        AppendIfChanged(sb, "Ffr3", before.Ffr3, after.Ffr3);
        AppendIfChanged(sb, "Ffr4", before.Ffr4, after.Ffr4);
        AppendIfChanged(sb, "Rcl1", before.Rcl1, after.Rcl1);
        AppendIfChanged(sb, "Rcr1", before.Rcr1, after.Rcr1);
        AppendIfChanged(sb, "Rcr2", before.Rcr2, after.Rcr2);
        AppendIfChanged(sb, "Rfl1", before.Rfl1, after.Rfl1);
        AppendIfChanged(sb, "Rfr1", before.Rfr1, after.Rfr1);
        AppendIfChanged(sb, "Rfr2", before.Rfr2, after.Rfr2);
        if (before.IsActive != after.IsActive)
        {
            AppendSeparator(sb);
            sb.Append("IsActive: ").Append(before.IsActive).Append(" -> ").Append(after.IsActive);
        }
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    private static void AppendIfChanged(StringBuilder sb, string name, string? a, string? b)
    {
        if (string.Equals(a, b, StringComparison.Ordinal)) return;
        AppendSeparator(sb);
        sb.Append(name).Append(": ").Append(Format(a)).Append(" -> ").Append(Format(b));
    }

    private static void AppendSeparator(StringBuilder sb)
    {
        if (sb.Length > 0) sb.Append("; ");
    }

    private static string Format(string? v) => string.IsNullOrEmpty(v) ? "(empty)" : v;

    private static string? Truncate(string? value, int maxLength)
    {
        if (value is null) return null;
        return value.Length <= maxLength ? value : value[..(maxLength - 1)] + "...";
    }

    public readonly record struct ToolSlotSnapshot(
        string? Family, string? MtCode, string? Destination,
        string? Fcl1, string? Fcl2, string? Fcr1,
        string? Ffl1, string? Ffl2,
        string? Ffr1, string? Ffr2, string? Ffr3, string? Ffr4,
        string? Rcl1,
        string? Rcr1, string? Rcr2,
        string? Rfl1,
        string? Rfr1, string? Rfr2,
        bool IsActive)
    {
        public ToolSlotSnapshot(ToolSlotConfiguration slot) : this(
            slot.Family, slot.MtCode, slot.Destination,
            slot.Fcl1, slot.Fcl2, slot.Fcr1,
            slot.Ffl1, slot.Ffl2,
            slot.Ffr1, slot.Ffr2, slot.Ffr3, slot.Ffr4,
            slot.Rcl1, slot.Rcr1, slot.Rcr2,
            slot.Rfl1, slot.Rfr1, slot.Rfr2,
            slot.IsActive)
        { }
    }
}
```

---

## Step 6: Copy validators and MudFormValidator adapter

### 6a. `Validators/ToolSlotConfigurationValidators.cs`

```csharp
using YourApp.Models;
using FluentValidation;

namespace YourApp.Validators;

public sealed class CreateToolSlotConfigurationValidator : AbstractValidator<CreateToolSlotConfigurationRequest>
{
    public CreateToolSlotConfigurationValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Family)
                        || !string.IsNullOrWhiteSpace(x.MtCode)
                        || !string.IsNullOrWhiteSpace(x.Destination))
            .WithMessage("At least one of Family, MtCode, or Destination must be set.");
    }
}

public sealed class UpdateToolSlotConfigurationValidator : AbstractValidator<UpdateToolSlotConfigurationRequest>
{
    // No constraints — every property is optional for patch semantics.
}
```

### 6b. `Validators/MudFormValidator.cs`

This generic adapter bridges FluentValidation with MudBlazor's `MudForm` validation. **You only need one copy of this class** even if you use it for other forms:

```csharp
using FluentValidation;

namespace YourApp.Validators;

public sealed class MudFormValidator<T>(IValidator<T> validator)
{
    public Func<object, string, Task<IEnumerable<string>>> ValidateField =>
        async (model, propertyName) =>
        {
            var context = ValidationContext<T>.CreateWithOptions(
                (T)model,
                strategy => strategy.IncludeProperties(propertyName));

            var result = await validator.ValidateAsync(context);
            return result.IsValid
                ? []
                : result.Errors.Select(e => e.ErrorMessage);
        };

    public async Task<bool> ValidateAllAsync(T instance)
    {
        var result = await validator.ValidateAsync(instance);
        return result.IsValid;
    }
}
```

### 6c. Register in `Program.cs`

```csharp
// FluentValidation — auto-discovers all validators in the assembly
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// MudFormValidator adapter — open generic so DI resolves MudFormValidator<T> for any T
builder.Services.AddTransient(typeof(YourApp.Validators.MudFormValidator<>));
```

---

## Step 7: Copy Razor components

Create a folder `Components/Pages/ToolSlots/` and add three files. Adjust `@using` directives to match your namespace.

### 7a. `Components/Pages/ToolSlots/Index.razor`

Copy from `AWBlazorApp/Components/Pages/ToolSlots/Index.razor`. Key adjustments:

- Replace `@using AWBlazorApp.*` with your project namespaces
- Replace `ApplicationDbContext` with your DbContext class name
- The `AppRoles` constants (`Employee`, `Manager`, `Admin`) reference a static class:
  ```csharp
  public static class AppRoles
  {
      public const string Admin = nameof(Admin);
      public const string Manager = nameof(Manager);
      public const string Employee = nameof(Employee);
  }
  ```
  If your project uses different role names, update the `AuthorizeView Roles` attributes accordingly.
- If you don't use `[Authorize]` or role-based auth, you can remove the `<AuthorizeView>` wrappers and the `@attribute [Authorize]` directive.

### 7b. `Components/Pages/ToolSlots/History.razor`

Copy from `AWBlazorApp/Components/Pages/ToolSlots/History.razor`. Same namespace adjustments as above.

### 7c. `Components/Pages/ToolSlots/ToolSlotDialog.razor`

Copy from `AWBlazorApp/Components/Pages/ToolSlots/ToolSlotDialog.razor`. Same namespace adjustments.

---

## Step 8 (Optional): Copy API endpoints

If you want REST API access in addition to the Blazor UI:

### 8a. `Models/Common.cs` (shared response types)

```csharp
namespace YourApp.Models;

public sealed record IdResponse(object Id);

public sealed record PagedResult<T>(IReadOnlyList<T> Results, int Total, int Skip, int Take);
```

### 8b. `Endpoints/ToolSlotConfigurationEndpoints.cs`

Copy from `AWBlazorApp/Endpoints/ToolSlotConfigurationEndpoints.cs`. Adjust namespaces and the authorization policy name (`"ApiOrCookie"` is specific to AWBlazor — replace with your project's policy or use `.RequireAuthorization()` for the default).

### 8c. Register in `Program.cs`

```csharp
app.MapToolSlotConfigurationEndpoints();
```

---

## Step 9: Generate the EF migration

If you're letting EF manage the `ToolSlotAuditLogs` table (recommended):

```pwsh
cd YourApp
dotnet ef migrations add AddToolSlotAuditLogs
dotnet ef database update
```

If you already created the table manually (Step 1b), either skip the migration or stamp it as applied:

```pwsh
# Create the migration but don't apply it — then manually insert the history row
dotnet ef migrations add AddToolSlotAuditLogs
# In SQL:
# INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
# VALUES ('20260413000000_AddToolSlotAuditLogs', '10.0.0');
```

---

## Step 10: Add navigation

Add a link to the Tool Slots page in your `NavMenu.razor` or equivalent:

```razor
<MudNavLink Href="tool-slots" Icon="@Icons.Material.Filled.Build">
    Tool slots
</MudNavLink>
```

---

## Summary of files to create

| # | File | Purpose |
|---|------|---------|
| 1 | `Data/Entities/ToolSlotConfiguration.cs` | Entity (externally-managed table) |
| 2 | `Data/Entities/ToolSlotAuditLog.cs` | Audit log entity (EF-managed) |
| 3 | `Models/ToolSlotConfigurationDtos.cs` | DTOs + mapping extensions |
| 4 | `Models/ToolSlotAuditLogDtos.cs` | Audit log DTO + mapping |
| 5 | `Services/ToolSlotAuditService.cs` | Audit diff/snapshot logic |
| 6 | `Validators/ToolSlotConfigurationValidators.cs` | FluentValidation rules |
| 7 | `Validators/MudFormValidator.cs` | MudForm-FluentValidation bridge |
| 8 | `Components/Pages/ToolSlots/Index.razor` | Main list page |
| 9 | `Components/Pages/ToolSlots/History.razor` | Audit history page |
| 10 | `Components/Pages/ToolSlots/ToolSlotDialog.razor` | Create/edit dialog |
| 11 | `Endpoints/ToolSlotConfigurationEndpoints.cs` | REST API (optional) |
| 12 | `Models/Common.cs` | Shared response types (if adding API) |

## Things to update in your existing code

- **DbContext**: Add `DbSet` properties + `OnModelCreating` config (Step 3)
- **Program.cs**: Register `AddValidatorsFromAssemblyContaining<Program>()` + `MudFormValidator<>` (Step 6c), optionally `MapToolSlotConfigurationEndpoints()` (Step 8c)
- **NavMenu**: Add navigation link (Step 10)
