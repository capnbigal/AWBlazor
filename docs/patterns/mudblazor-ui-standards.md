# MudBlazor UI Standards

The opinionated rules for building UIs in this codebase. Where this document and the official MudBlazor docs disagree, this document wins (because it's the agreed in-app convention).

## 1. Page shell

Every page renders inside `MainLayout`. The shape:

```razor
@page "/feature"
@attribute [Authorize]

<PageTitle>Feature name</PageTitle>

<MudStack Spacing="3">
    <MudStack Row="true" AlignItems="AlignItems.Center">
        <MudText Typo="Typo.h4">Feature name</MudText>
        <MudSpacer />
        <!-- Page-level actions: New button, refresh, etc. -->
    </MudStack>

    <!-- Filter bar -->
    <MudPaper Class="pa-3" Elevation="1">
        <MudGrid Spacing="2">
            <MudItem xs="12" sm="3">
                <MudTextField @bind-Value="filterX" Label="Filter X" Variant="Variant.Outlined" />
            </MudItem>
            <!-- ... -->
        </MudGrid>
    </MudPaper>

    <!-- Main content (grid, dashboard, form, etc.) -->
</MudStack>
```

## 2. Density & sizing

| Component | Setting |
|---|---|
| `MudDataGrid` | `Dense="true" Hover="true" Striped="true"` |
| `MudTextField` | `Variant="Variant.Outlined"` always (matches MudForm convention) |
| `MudButton` | `Variant="Variant.Filled"` for primary, `Variant.Outlined` for secondary, `Variant.Text` for tertiary |
| `MudIconButton` | `Size="Size.Small"` in tables, `Size="Size.Medium"` in toolbars |
| `MudPaper` | `Class="pa-3"` for filter bars, `Class="pa-6"` for forms |

## 3. Color usage

| Color | Use for |
|---|---|
| `Color.Primary` | Primary actions, key navigation |
| `Color.Secondary` | Secondary actions, supporting info |
| `Color.Success` | Confirmation snackbars, "active" status |
| `Color.Info` | Informational alerts |
| `Color.Warning` | Soft warnings, expiring items |
| `Color.Error` | Destructive actions, validation errors |
| `Color.Default` | Neutral, system messages |

Never hardcode hex colors. Use `var(--mud-palette-...)` in custom CSS so dark mode works.

## 4. Forms

In **Static SSR** (Identity pages, etc.):
```razor
<EditForm Model="Input" method="post" OnValidSubmit="..." FormName="login">
    <DataAnnotationsValidator />
    <ValidationSummary class="form-validation-summary" role="alert" />
    <div class="form-field">
        <label for="login-email" class="form-label">Email</label>
        <InputText id="login-email" @bind-Value="Input.Email" type="email" class="form-control" />
        <ValidationMessage For="@(() => Input.Email)" class="form-error" />
    </div>
</EditForm>
```

In **Interactive Server** (dialogs, etc.):
```razor
<MudForm @ref="form" Model="@request" Validation="@validator.ValidateField" ValidationDelay="0">
    <MudTextField @bind-Value="request.Name" Label="Name" Variant="Variant.Outlined" />
</MudForm>
```

## 5. Confirmations

For any **destructive action** (delete, deactivate, mass-update, etc.):

```csharp
var confirm = await DialogService.ShowMessageBoxAsync(
    "Delete record?",
    $"Permanently delete record #{id}?",
    yesText: "Delete", cancelText: "Cancel");
if (confirm != true) return;
```

For **complex confirmations** (with details/inputs), use a custom dialog with:
- Red `Color.Error` button for the destructive action
- Cancel button on the left
- Required confirmation checkbox if the action is irreversible

## 6. Loading & empty states

**Loading**:
```razor
@if (loading)
{
    <MudProgressLinear Indeterminate="true" />
}
else if (items.Count == 0)
{
    <EmptyState Title="No data yet"
                Description="Get started by creating your first item."
                ActionText="Create" ActionHref="/feature/new" />
}
else
{
    <!-- normal content -->
}
```

**Skeletons** for dashboard cards:
```razor
@if (loading)
{
    <KpiCardSkeleton />
}
else
{
    <KpiCard Value="@total" Label="Total" />
}
```

## 7. Snackbars

| Severity | When |
|---|---|
| `Severity.Success` | After a successful create/update/delete |
| `Severity.Error` | Validation failures, server errors |
| `Severity.Warning` | Soft conflicts (e.g. "stale data, refreshing...") |
| `Severity.Info` | Status messages (e.g. "saved as draft") |

Keep snackbar text short — full sentence, no period needed unless multiple sentences.

## 8. Accessibility

### Required for every UI element

- **Icon-only button**: must have `aria-label` or be wrapped in `<MudTooltip>`
- **Form input**: must have an associated `<label>` (via `for`/`id`)
- **Custom interactive div**: must have `role="button"` + `tabindex="0"` + key handler
- **Status region**: `role="status"` + `aria-live="polite"`
- **Color**: never use color alone to convey meaning (add icon or text)

### Recommended

- Heading hierarchy: page title is `h1` (via `<PageTitle>`), section titles are `h4`/`h5`/`h6`
- Tab order matches visual order (don't use positive `tabindex` values)
- Keyboard shortcuts documented in tooltips

## 9. Mobile responsiveness

Use `MudGrid` with breakpoint props:
```razor
<MudGrid Spacing="2">
    <MudItem xs="12" sm="6" md="4">
        <MudTextField ... />
    </MudItem>
</MudGrid>
```

Avoid:
- Hardcoded `Style="width: 300px"` (use `Class="full-width"` or breakpoint props)
- `MudDataGrid` without `Dense="true"` on small screens (too cramped at standard density)
- Multiple text columns side-by-side without responsive collapse

Test at 320px / 768px / 1280px viewport widths.

## 10. Common component composition patterns

### List page

```
<PageTitle> + <MudText Typo="h4"> + Action button
└── <MudPaper> filter bar
└── <MudDataGrid> with ServerData
```

### CRUD dialog

```
<MudDialog>
├── <TitleContent>: <MudText Typo="h6">
├── <DialogContent>: <MudForm> with MudGrid + MudTextField + ...
└── <DialogActions>: Cancel button + Save button (Disabled while saving)
```

### Dashboard

```
<MudGrid Spacing="3">
├── <MudItem xs="12" sm="6" md="3"> <KpiCard /> (×4)
└── <MudItem xs="12"> <TimeSeriesChart />
```

### Empty state with action

```razor
<EmptyState Icon="@Icons.Material.Filled.Inbox"
            Title="No forecasts yet"
            Description="Forecasts let you project future values from historical AdventureWorks data."
            ActionText="Create forecast"
            ActionHref="/forecasts/new" />
```

## 11. Anti-patterns

- ❌ MudBlazor inputs in static SSR forms (use `<InputText>` etc. instead)
- ❌ Hardcoded colors that don't respect dark mode
- ❌ Icons with no label or tooltip
- ❌ Snackbars longer than 1 sentence
- ❌ More than 4 KPI cards on one row (use 2x2 grid instead)
- ❌ Modal dialogs that scroll the page underneath (use `FullWidth="true" MaxWidth="..."`)
- ❌ Click handlers on `<div>` without `role="button"` and keyboard support
- ❌ Putting business logic in `OnClick` handlers (extract to private methods)

## 12. Page title convention

```razor
<PageTitle>Feature name</PageTitle>
```

Keep concise. Browser tab will show `Feature name` (the layout prepends/appends app name if configured).
