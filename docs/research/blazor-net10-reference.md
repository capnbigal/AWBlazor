# Blazor Web App on .NET 10 — Quick Reference

Sourced from Microsoft Learn (`aspnetcore-10.0` moniker), November 2025 revisions. This is a developer quick-reference, not a tutorial.

---

## 1. Render modes

A Blazor Web App supports four render modes. Every routable page inherits a mode from its parent (default is Static Server).

| Mode | Location | Interactive | Purpose |
|---|---|---|---|
| **Static Server** (default) | Server | No | SSR HTML, form POSTs, SEO, request/response flows (e.g. cookie writes) |
| **Interactive Server** | Server (SignalR) | Yes | Server-hosted interactive UI over a circuit |
| **Interactive WebAssembly** | Client (wasm) | Yes | CSR, offline-capable, no SignalR |
| **Interactive Auto** | Server then client | Yes | Start on server, hand off to wasm once the bundle is cached |

Prerendering is **on by default** for all interactive modes.

### Decision tree

```
Does the page write to HttpContext (set cookies, issue headers, redirect)?
├─ YES → Static Server  (apply [ExcludeFromInteractiveRouting])
└─ NO  → Does it need real-time interactivity (buttons, live data, dialogs)?
        ├─ NO  → Static Server
        └─ YES → Is cold-start latency critical?
                ├─ NO  → Interactive Server
                └─ YES → Do clients download wasm acceptably?
                        ├─ Offline/PWA needed → Interactive WebAssembly
                        └─ Mix both           → Interactive Auto
```

### Applying a render mode

```razor
@* Per component definition *@
@page "/counter"
@rendermode InteractiveServer

@* Per instance *@
<Dialog @rendermode="InteractiveServer" />

@* App-wide (in App.razor) *@
<HeadOutlet @rendermode="InteractiveServer" />
<Routes     @rendermode="InteractiveServer" />
```

### Propagation rules

- Modes propagate **down** the tree.
- A child **cannot switch to a different interactive mode** (e.g. Server child inside a WebAssembly parent throws at runtime).
- Parameters across a Static→interactive boundary **must be JSON-serializable** — `RenderFragment`/`ChildContent` is not allowed.
- The root `App` component cannot itself be made interactive.

### Detecting mode at runtime (.NET 9+/10)

```razor
@if (!RendererInfo.IsInteractive) { <p>Loading…</p> }

@if (AssignedRenderMode is null)
{
    <form action="/movies">...</form>  @* static fallback *@
}
else
{
    <input @bind="filter" /><button @onclick="Filter">Search</button>
}
```

---

## 2. Static SSR pages inside an interactive app

Pages that set cookies, redirect, or otherwise depend on the request/response cycle (Identity is the canonical example) must run as Static SSR even when the rest of the app is interactive.

```razor
@attribute [ExcludeFromInteractiveRouting]
```

```razor
@* App.razor *@
<HeadOutlet @rendermode="@PageRenderMode" />
<Routes     @rendermode="@PageRenderMode" />

@code {
    [CascadingParameter] private HttpContext HttpContext { get; set; } = default!;
    private IComponentRenderMode? PageRenderMode =>
        HttpContext.AcceptsInteractiveRouting() ? InteractiveServer : null;
}
```

Navigation to an `[ExcludeFromInteractiveRouting]` page forces a **full page reload** so the root component re-renders under the new mode — that is what allows the server to write cookies.

---

## 3. Forms

### EditForm (preferred)

```razor
<EditForm Model="Model" OnValidSubmit="Submit" FormName="starship">
    <DataAnnotationsValidator />
    <ValidationSummary />
    <InputText @bind-Value="Model!.Id" />
    <button type="submit">Submit</button>
</EditForm>

@code {
    [SupplyParameterFromForm] private Starship? Model { get; set; }
    protected override void OnInitialized() => Model ??= new();
}
```

### Rules that bite

- **`FormName` is required** and must be unique per form on a page. Use per-row FormNames for dynamic lists plus a hidden `<input type="hidden" name="X.Id" value="@id" />`.
- `[SupplyParameterFromForm]` properties get nulled by the binder on every render. Always re-hydrate them in `OnInitialized`: `Model ??= new();`.
- Antiforgery middleware must come **after** `UseAuthentication`/`UseAuthorization` and after `UseRouting`.
- Only Blazor's built-in `<Input*>` components reliably participate in SSR form binding — they derive `name` from the `@bind-Value` expression. Third-party components that render raw `<input>` without emitting a `name` attribute will silently fail model-binding on POST. **MudBlazor inputs DO NOT WORK in SSR forms.**
- **Client-side validation requires an active circuit.** Static-SSR forms are validated only after POST.

### Overposting / mass-assignment

Use a dedicated view-model/DTO per form. Never bind directly to an EF entity containing fields users shouldn't set.

---

## 4. Authentication & authorization

### Services

```csharp
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();   // serialize auth state to wasm
```

### Page-level

```razor
@page "/admin"
@attribute [Authorize(Roles = "Admin, Superuser")]
```

Only use `[Authorize]` on `@page` components. For sections within a page use `AuthorizeView`.

### `AuthorizeView`

```razor
<AuthorizeView Roles="Admin">
    <Authorized>Hi, @context.User.Identity!.Name</Authorized>
    <NotAuthorized>Go away</NotAuthorized>
    <Authorizing>Checking…</Authorizing>
</AuthorizeView>
```

`AuthorizeView` controls **UI only** — it is not a security boundary. Server-side policies and minimal-API authorization are what actually enforce access.

### HttpContext — the #1 server-side gotcha

`IHttpContextAccessor.HttpContext` is available **only during the initial HTTP request**. In SignalR-circuit event handlers it will be `null`. Patterns:

- **Static SSR page** → `[CascadingParameter] HttpContext` works; use it for cookie reads, redirects, etc.
- **Interactive Server** → capture what you need in `OnInitialized`, then never touch `HttpContext` again; for tokens/claims rely on `AuthenticationStateProvider`.

### Antiforgery

`AddRazorComponents()` registers antiforgery. `UseAntiforgery()` goes **after** `UseRouting`, `UseAuthentication`, `UseAuthorization`. To read a token for an out-of-band POST:

```razor
@inject AntiforgeryStateProvider Antiforgery
var token = await Antiforgery.GetAntiforgeryToken();
```

---

## 5. State management

### Options ranked

| Need | Use |
|---|---|
| Preserve prerendered state across the SSR→interactive handoff | `[PersistentState]` (new in .NET 10) or `PersistentComponentState` |
| Per-user, per-tab state within one circuit | Scoped DI services (e.g. `AppState`) |
| Per-user state across tabs | `ProtectedLocalStorage` |
| Ephemeral per-tab state | `ProtectedSessionStorage` |
| Pass data down a tree without props | `<CascadingValue>` / `[CascadingParameter]` |

### `[PersistentState]` (.NET 10)

```razor
@code {
    [PersistentState] public int? CurrentCount { get; set; }

    protected override void OnInitialized()
    {
        CurrentCount ??= Random.Shared.Next(100); // only on true first render
    }
}
```

Serialized with `System.Text.Json` into the prerendered HTML, data-protected for Interactive Server, visible in the browser for WebAssembly/Auto (so **do not put secrets in it**).

### Persisting scoped service state (.NET 10)

```csharp
// CounterTracker.cs
public class CounterTracker
{
    [PersistentState] public int CurrentCount { get; set; }
}

// Program.cs
builder.Services.AddScoped<CounterTracker>();
builder.Services.AddRazorComponents()
    .RegisterPersistentService<CounterTracker>(RenderMode.InteractiveAuto);
```

Only **scoped** services are supported.

### Enhanced-navigation handling (.NET 10)

`PersistentComponentState` now survives enhanced navigation. By default state loads once per component to avoid clobbering user edits; opt-in updates:

```csharp
[PersistentState(AllowUpdates = true)]
public WeatherForecast[]? Forecasts { get; set; }

[PersistentState(RestoreBehavior = RestoreBehavior.SkipInitialValue)]
public string NoPrerenderedData { get; set; }

[PersistentState(RestoreBehavior = RestoreBehavior.SkipLastSnapshot)]
public int CounterNotRestoredOnReconnect { get; set; }
```

### DbContext in interactive components

Inject `IDbContextFactory<T>`, not a scoped `DbContext` — the circuit's lifetime dwarfs a single logical operation, and `DbContext` is not thread-safe.

---

## 6. Performance

- **Prerendering** is on by default for interactive modes. Disable per-component with `new InteractiveServerRenderMode(prerender: false)` only when it causes more trouble than it's worth.
- **Streaming rendering** (`@attribute [StreamRendering]`) flushes HTML as async work completes — great for lists loaded from slow queries.
- **`RendererInfo.IsInteractive`** lets you disable UI that shouldn't fire until the circuit is up.
- **Virtualization** (`<Virtualize Items="@items" ItemSize="50">`) for long lists — render only what's visible.
- **Persistent state** avoids the "render once, then flash/replace with different values" problem.
- Prefer small per-component render-mode scopes over global interactivity when large swaths of the app are static.

---

## 7. What's new in .NET 10 for Blazor

1. **`[PersistentState]` declarative attribute** replaces the boilerplate around `RegisterOnPersisting`/`TryTakeFromJson`. Paired with `[PersistentState(AllowUpdates=true)]` and `RestoreBehavior.{SkipInitialValue,SkipLastSnapshot}`.
2. **`RegisterPersistentService<T>(RenderMode)`** — scoped services themselves can declare `[PersistentState]` properties that flow from prerender to interactive mode.
3. **Enhanced-navigation support for persistent state** — state survives enhanced navigations (a hard gap in .NET 8/9).
4. **`PersistentComponentState.RegisterOnRestoring`** — symmetric imperative hook to `RegisterOnPersisting`.
5. **`PersistentComponentStateSerializer<T>`** extensibility point for custom per-type serialization.

### Also unchanged but worth remembering

- `AcceptsInteractiveRouting()` + `[ExcludeFromInteractiveRouting]` is still the official pattern for cookie-writing Identity pages.
- Data Protection secures the persisted payload for Interactive Server only — WebAssembly/Auto modes expose it to the browser.

---

## 8. Cheat sheet — common pitfalls

| Symptom | Cause | Fix |
|---|---|---|
| Form "field required" on every POST | Third-party input component doesn't emit `name` | Use `<InputText>`/`<InputCheckbox>`/`<InputSelect>` in SSR forms |
| "Model parameter required" | Binder nulled `[SupplyParameterFromForm]` property | `Model ??= new();` in `OnInitialized` |
| Value flickers from X to Y on load | Prerender ran `OnInitializedAsync`; interactive ran it again | `[PersistentState]` the field |
| `HttpContext` null in event handler | Tried to use it on an interactive circuit | Capture in `OnInitialized` during SSR; or make the page `[ExcludeFromInteractiveRouting]` |
| "There is no registered service of type IWebAssemblyHostEnvironment" during prerender | Client-only service injected into a prerendered component | Register a server equivalent, make optional, or disable prerender |
| Enhanced navigation leaves DOM stale | Dynamically injected DOM replaced | `data-permanent` on the element |
| Cookies not set after "Login" click | Tried to run Identity login in an interactive component | Move to Static SSR page with `[ExcludeFromInteractiveRouting]` and full reload |
| `NavigationException` breaks debugger | By-design SSR redirect | `[DebuggerStepThrough]` on the calling pages, `[DebuggerHidden]` on RedirectManager |

---

### Sources

- `learn.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-10.0`
- `.../blazor/components/render-modes?view=aspnetcore-10.0`
- `.../blazor/components/prerender?view=aspnetcore-10.0`
- `.../blazor/state-management/prerendered-state-persistence?view=aspnetcore-10.0`
- `.../blazor/security/server/?view=aspnetcore-10.0`
- `.../blazor/security/?view=aspnetcore-10.0`
- `.../blazor/forms/?view=aspnetcore-10.0`
