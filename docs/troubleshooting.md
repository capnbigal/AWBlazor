# Troubleshooting

Common issues and their solutions for AWBlazorApp.

---

## 1. SQL Server connection failure

**Symptom:** App fails to start with `SqlException: A network-related or instance-specific error occurred` or similar connection errors.

**Cause:** The SQL Server instance `ELITE` is unreachable, or the current Windows user does not have access.

**Fix:**
- Verify the SQL Server instance `ELITE` is running and reachable from your machine.
- Confirm Windows authentication is enabled on the SQL Server instance.
- Confirm your Windows account has access to the `AdventureWorks2022` database (production) or `AdventureWorks2022_dev` (development/test).
- If your SQL Server instance has a different name, update `ConnectionStrings:DefaultConnection` in `appsettings.json` and `appsettings.Development.json`.
- Ensure `TrustServerCertificate=True` is in the connection string if the server uses a self-signed certificate.

---

## 2. Hangfire DistributedLock error

**Symptom:** App throws `Hangfire.SqlServer.SqlServerDistributedLock` exceptions or fails to acquire a distributed lock on startup.

**Cause:** Hangfire tables do not exist in the database, or the Hangfire server is trying to start when it should not be.

**Fix:**
- Hangfire auto-creates its tables on first run. If the database user lacks DDL permissions, create the tables manually using the Hangfire SQL Server schema script.
- In test environments, Hangfire is disabled via `Features:Hangfire = false` in the in-memory configuration override. Verify this setting is present if you see lock errors during tests.
- To disable Hangfire in any environment, set `Features:Hangfire` to `false` in appsettings or environment variables.

---

## 3. MudBlazor SSR form binding -- fields always empty on POST

**Symptom:** Form submissions on Identity pages (login, register, etc.) fail with validation errors like "The Email field is required" even though you filled in the form. The `[SupplyParameterFromForm]` model properties are always null/empty.

**Cause:** `MudTextField`, `MudCheckBox`, `MudSelect`, and other MudBlazor input components do not emit the HTML `name` attribute needed by Blazor's static SSR form binder.

**Fix:**
- On any page marked with `[ExcludeFromInteractiveRouting]` (Identity/Account pages), use Blazor built-in components: `<InputText>`, `<InputCheckbox>`, `<InputSelect>`.
- These derive the correct `name` attribute from the `@bind-Value` expression automatically.
- Style them with the shared classes in `wwwroot/css/account-forms.css` (`form-field`, `form-control`, `form-label`, `form-error`, `form-validation-summary`).
- Layout-only MudBlazor components (`MudPaper`, `MudContainer`, `MudText`, `MudButton`, `MudStack`, etc.) are safe in SSR forms -- only the input-emitting components are the problem.

---

## 4. Antiforgery token errors on logout

**Symptom:** Clicking "Log out" throws an antiforgery validation exception or returns a 400 error.

**Cause:** The logout endpoint requires an antiforgery token but the request does not include one, or the token has expired.

**Fix:**
- The logout endpoint should have `DisableAntiforgery()` applied if it is a GET-based signout, or the logout form must include an `<AntiforgeryToken />` component.
- Verify the logout mapping in `Program.cs` or `IdentityEndpointMappingExtensions.cs` has `.DisableAntiforgery()` if using a simple link/redirect flow.

---

## 5. NavigationException in debugger

**Symptom:** Visual Studio breaks on a `NavigationException` during redirects (e.g., after login or on `NavigationManager.NavigateTo`).

**Cause:** This is normal Blazor control flow. Blazor uses exceptions internally to trigger navigation during static SSR rendering. It is not a bug.

**Fix:**
- In Visual Studio, go to **Debug > Windows > Exception Settings** and uncheck `Microsoft.AspNetCore.Components.NavigationException` (or its parent category).
- Alternatively, configure the debugger to "Continue when unhandled" for this exception type.
- The exception is caught by the Blazor framework and never reaches the user.

---

## 6. Empty MudDataGrid -- no rows displayed

**Symptom:** A `MudDataGrid` renders with column headers but zero rows, even though the database has data.

**Cause:** Several possible issues with the `ServerData` callback or database connectivity.

**Fix:**
- Verify the `ServerData` callback is wired up correctly and returns a `GridData<T>` with both `Items` and `TotalItems`.
- Check that you are using `IDbContextFactory<ApplicationDbContext>` (not a scoped DbContext) and creating a fresh context per call.
- Confirm the database is reachable and the table has data (`SELECT COUNT(*) FROM [TableName]`).
- Check the browser console and app logs for exceptions -- the `ServerData` callback may be throwing silently.
- Ensure the `@bind-Value` or `Property` on each `<PropertyColumn>` matches the DTO property name exactly (case-sensitive).

---

## 7. Dark mode not persisting across sessions

**Symptom:** Toggling dark mode works for the current page but reverts on refresh or navigation.

**Cause:** The dark mode preference is stored in a cookie. The toggle requires a page navigation to set the cookie on the response.

**Fix:**
- This is by design. The toggle sets a cookie and forces a navigation so the server can read the preference on the next request and render the correct MudBlazor theme.
- If dark mode appears to not persist, check that cookies are not being blocked by browser settings or extensions.
- Verify the cookie middleware is registered in `Program.cs` before the Blazor middleware.

---

## 8. QR code not showing on 2FA setup page

**Symptom:** The two-factor authentication setup page shows the manual entry key and `otpauth://` URI but no QR code image.

**Cause:** QR code rendering is not currently implemented. The app provides the manual setup key and URI only.

**Fix:**
- This is a known limitation. See Phase 7b in `docs/phase-plan.md` for the planned QR code rendering feature.
- Users can copy the `otpauth://` URI and paste it into their authenticator app, or manually enter the setup key.
- To add QR code rendering, install `QRCoder` (or a similar package), generate a QR image from the `otpauth://` URI, and render it as a base64 `<img>` tag on the 2FA setup page.
