# MudBlazor 9 — Quick Reference

Practical reference for the patterns used in this codebase. Authoritative source: <https://mudblazor.com/>. This document is grounded in the patterns actually shipping in AWBlazorApp + MudBlazor 9 documentation through May 2025.

---

## 1. Setup essentials

In `Program.cs`:

```csharp
builder.Services.AddMudServices();
```

In `App.razor` (`<head>`):

```html
<link rel="stylesheet" href="_content/MudBlazor/MudBlazor.min.css" />
```

In every layout root (Blazor Server: in `MainLayout.razor`):

```razor
<MudThemeProvider @rendermode="InteractiveServer" @bind-IsDarkMode="_isDarkMode" />
<MudPopoverProvider />
<MudDialogProvider />
<MudSnackbarProvider />
```

These four providers must be present for dialogs/snackbars/popovers to work.

---

## 2. MudDataGrid — the workhorse

### Server-side data

```razor
<MudDataGrid T="ProductDto"
             ServerData="LoadServerDataAsync"
             @ref="grid"
             Dense="true" Hover="true" Striped="true"
             SortMode="SortMode.Single">
    <Columns>
        <PropertyColumn Property="x => x.Name" Title="Name" />
        <PropertyColumn Property="x => x.Price" Title="Price" Format="C" />
        <TemplateColumn Title="Actions" Sortable="false">
            <CellTemplate>
                <MudIconButton Icon="@Icons.Material.Filled.Edit"
                               Size="Size.Small"
                               OnClick="@(() => OnEditAsync(context.Item))"
                               aria-label="Edit" />
            </CellTemplate>
        </TemplateColumn>
    </Columns>
    <PagerContent>
        <MudDataGridPager T="ProductDto" />
    </PagerContent>
</MudDataGrid>

@code {
    private MudDataGrid<ProductDto> grid = null!;

    private async Task<GridData<ProductDto>> LoadServerDataAsync(
        GridState<ProductDto> state, CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        var query = db.Products.AsNoTracking();

        // Apply sort from state.SortDefinitions.FirstOrDefault()
        // Apply filters from state.FilterDefinitions

        var total = await query.CountAsync(ct);
        var rows = await query
            .Skip(state.Page * state.PageSize)
            .Take(state.PageSize)
            .Select(x => x.ToDto())
            .ToListAsync(ct);

        return new GridData<ProductDto> { Items = rows, TotalItems = total };
    }
}
```

### Client-side data (small datasets only)

```razor
<MudDataGrid Items="@products" QuickFilter="@QuickFilter" />
```

### Hierarchy / expand rows

```razor
<HierarchyColumn T="ProductDto" />
<ChildRowContent>
    <MudCard>
        <ProductExpandedRow ProductId="@context.Item.Id" />
    </MudCard>
</ChildRowContent>
```

`ChildRowContent` is **lazy-rendered** — the inner component is only instantiated when the row is expanded. Put your data load in `OnInitializedAsync` of that component.

### Virtualization

For client-side data with thousands of rows: `Virtualize="true"`. Don't use with `ServerData` — pagination already handles it.

### Quick filter

```csharp
private Func<ProductDto, bool> QuickFilter => x =>
{
    if (string.IsNullOrWhiteSpace(_searchString)) return true;
    return x.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase);
};
```

### Reload after mutation

```csharp
await grid.ReloadServerData();
```

---

## 3. MudDialog patterns

### Show a dialog and await result

```csharp
var parameters = new DialogParameters<ProductDialog>
{
    { x => x.Model, dto },
};
var options = new DialogOptions
{
    MaxWidth = MaxWidth.Medium,
    FullWidth = true,
    CloseButton = true,
};
var dialog = await DialogService.ShowAsync<ProductDialog>("Edit product", parameters, options);
var result = await dialog.Result;

if (result is { Canceled: false })
{
    Snackbar.Add("Saved.", Severity.Success);
    await grid.ReloadServerData();
}
```

### Inside the dialog

```razor
<MudDialog>
    <TitleContent>
        <MudText Typo="Typo.h6">@Title</MudText>
    </TitleContent>
    <DialogContent>
        <MudForm @ref="form" Model="@Model">
            ...
        </MudForm>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel">Cancel</MudButton>
        <MudButton Color="Color.Primary" Variant="Variant.Filled"
                   OnClick="SaveAsync" Disabled="@saving">
            @(saving ? "Saving..." : "Save")
        </MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public ProductDto? Model { get; set; }

    private void Cancel() => MudDialog.Cancel();
    private async Task SaveAsync()
    {
        // ... validate, save ...
        MudDialog.Close(DialogResult.Ok(entity.Id));
    }
}
```

### Confirmation dialog

```csharp
var confirm = await DialogService.ShowMessageBoxAsync(
    title: "Delete product?",
    markupMessage: new MarkupString($"Permanently delete <b>{name}</b>?"),
    yesText: "Delete",
    cancelText: "Cancel");
if (confirm == true) { /* delete */ }
```

---

## 4. MudForm + FluentValidation

```razor
<MudForm @ref="form" Model="@request" Validation="@validator.ValidateField" ValidationDelay="0">
    <MudTextField @bind-Value="request.Name" Label="Name" Variant="Variant.Outlined" />
    <MudNumericField @bind-Value="request.Price" Label="Price" Format="C" />
</MudForm>

@code {
    [Inject] MudFormValidator<CreateProductRequest> Validator { get; set; } = default!;
    private MudForm form = null!;
    private CreateProductRequest request = new();

    private async Task SaveAsync()
    {
        if (!await Validator.ValidateAllAsync(request))
        {
            Snackbar.Add("Please fix the errors.", Severity.Error);
            return;
        }
        // ... persist ...
    }
}
```

`MudFormValidator<T>` is a generic adapter that bridges `MudForm.Validation` with FluentValidation's `IValidator<T>`. See `Validators/MudFormValidator.cs` and the existing CRUD dialogs for the pattern.

---

## 5. MudSnackbar

```csharp
@inject ISnackbar Snackbar

Snackbar.Add("Created.", Severity.Success);
Snackbar.Add("Network error.", Severity.Error);
Snackbar.Add("Heads up.", Severity.Warning);
Snackbar.Add("FYI.", Severity.Info);
```

Configure default behavior in `Program.cs`:

```csharp
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 5000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
});
```

---

## 6. Theming + dark mode

```razor
<MudThemeProvider @bind-IsDarkMode="_isDarkMode" Theme="@_theme" />

@code {
    private bool _isDarkMode;
    private MudTheme _theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = Colors.Blue.Default,
            // ...
        },
        PaletteDark = new PaletteDark
        {
            Primary = Colors.Blue.Lighten1,
            // ...
        },
    };
}
```

### FOUC (flash of unstyled content) prevention

Read the dark-mode cookie before Blazor loads and apply `data-mud-theme` to `<html>` in an inline `<script>` tag. See `Components/App.razor` in this codebase for the working pattern.

```html
<!-- Inline in <head> -->
<script>
  window.getDarkModeCookie = function() {
    var m = document.cookie.match(/(?:^|;\s*)darkMode=([^;]*)/);
    return m !== null && m[1] === 'true';
  };
  if (window.getDarkModeCookie()) document.documentElement.setAttribute('data-mud-theme', 'dark');
</script>
<style>
  html[data-mud-theme="dark"] { background-color: #1e1e1e; }
  html[data-mud-theme="dark"] body { background-color: #1e1e1e; color: #fff; }
</style>
```

### Theme-aware colors in custom CSS

Use MudBlazor's CSS variables instead of hardcoded colors:

```css
.my-component {
    background-color: var(--mud-palette-surface);
    color: var(--mud-palette-text-primary);
    border: 1px solid var(--mud-palette-divider);
}
```

---

## 7. Common pitfalls (battle-tested)

| Symptom | Cause | Fix |
|---|---|---|
| Form POST fails with "field required" on every input | MudBlazor inputs in static-SSR `EditForm method="post"` | Use `<InputText>`/`<InputCheckbox>`/`<InputSelect>` in SSR forms — only Mud non-input components (MudPaper, MudButton, MudText) are safe |
| `MudDataGrid` doesn't refresh after save | Forgot to call `ReloadServerData()` | `await grid.ReloadServerData();` after mutation |
| `MudDataGrid` slow with many columns | Too many `PropertyColumn`s with non-trivial getters | Switch to `TemplateColumn` only where needed; minimize work in cell expressions |
| Dialog flashes briefly then closes | `MudDialog.Close()` called before `await` finished | Wrap save logic in try/finally; only close on success |
| Snackbars stack and don't disappear | Default config has no max | Set `MaxDisplayedSnackbars` and `PreventDuplicates` |
| Dark mode flashes on every nav | Server re-renders `<html data-theme>` on each request | Drive theme purely client-side; server should not set theme attributes |
| `@bind-IsDarkMode` doesn't persist | No cookie/storage backing | Use `JS.InvokeVoidAsync("setDarkModeCookie", isDark)` from your toggle handler |
| Tooltip never shows on icon button | Missing `aria-label` (and no Text) | `<MudTooltip Text="...">` wrapper or `aria-label` attribute |

---

## 8. Analyzer warnings

MudBlazor 9 ships analyzers (NuGet `MudBlazor.Analyzers`). Common warnings:

| Code | Meaning | Fix |
|---|---|---|
| `MUD0001` | Required parameter missing | Provide the parameter or remove the component |
| `MUD0002` | "Illegal Attribute on MudX using pattern 'LowerCase'" | False positive on PascalCase MudBlazor params (e.g. `XAxisLabels` on `MudChart`) — safe to ignore or `<NoWarn>MUD0002</NoWarn>` |
| `MUD0003` | Render mode mismatch | Check parent/child render modes |

---

## 9. Performance tips for enterprise apps

- **Server-side data for any grid backed by a DB table** — even if the table has 100 rows today, it'll have 100k tomorrow.
- **`Dense="true"` + `Hover="true"`** is the most readable density for long lists.
- **Disable unused features**: `Sortable="false"`, `Filterable="false"`, `Resizable="false"` per column when you don't need them — each one costs render time.
- **`PropertyColumn` over `TemplateColumn`** when both work — PropertyColumn knows the type and can use cached expression trees.
- **Avoid heavy markup in `CellTemplate`** — keep it to icons + a small text. Move complex layouts into a separate component referenced by name.
- **Don't render dialogs that are closed** — `DialogService.ShowAsync` lazy-creates them.
- **Use `@key` on items in loops** to help Blazor's diff algorithm.

---

## 10. Recommended component patterns for this codebase

- **Page = list grid + filter bar + create button** (see `Components/Pages/AdventureWorks/Addresses/Index.razor`)
- **Dialog = MudForm + Save/Cancel + try/finally to control `saving` flag** (see `AddressDialog.razor`)
- **History page = MudDataGrid filtered by entity Id** (see `Addresses/History.razor`)
- **Shared empty state = `<EmptyState Title="..." Description="..." ActionText="..." ActionHref="..." />`** (see `Components/Shared/EmptyState.razor`)
- **Loading skeleton = `<KpiCardSkeleton />` for KPI cards** (see `Components/Shared/KpiCardSkeleton.razor`)

---

### Sources

- <https://mudblazor.com/getting-started/installation>
- <https://mudblazor.com/components/datagrid>
- <https://mudblazor.com/components/dialog>
- <https://mudblazor.com/components/form>
- <https://mudblazor.com/customization/theming>
- This codebase's `CLAUDE.md` Section 2 (the SSR form gotcha)
