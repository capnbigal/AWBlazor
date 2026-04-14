using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;

namespace AWBlazorApp.Components.Account;

[DebuggerStepThrough]
[DebuggerNonUserCode]
internal sealed class IdentityRedirectManager(NavigationManager navigationManager)
{
    public const string StatusCookieName = "Identity.StatusMessage";

    private static readonly CookieBuilder StatusCookieBuilder = new()
    {
        SameSite = SameSiteMode.Strict,
        HttpOnly = true,
        IsEssential = true,
        MaxAge = TimeSpan.FromSeconds(5),
    };

    // Blazor implements navigation by THROWING NavigationException — the framework catches it
    // (in the static-SSR endpoint or the interactive renderer's event dispatch) and converts
    // it to either an HTTP 302 or a client-side navigation. The throw is intentional control
    // flow, not an error. [DebuggerNonUserCode] tells Just My Code to treat these methods as
    // framework code so the debugger doesn't break on the first-chance NavigationException
    // every time a successful redirect runs.
    //
    // NOTE: [DebuggerNonUserCode] only works when Just My Code is enabled (Tools → Options →
    // Debugging → General → Enable Just My Code). If JMC is off, the debugger will still
    // break on the throw — you can suppress it via Debug → Windows → Exception Settings,
    // search for "NavigationException", and uncheck the box.
    [DebuggerHidden]
    [DebuggerNonUserCode]
    [DoesNotReturn]
    public void RedirectTo(string? uri)
    {
        uri ??= "";

        // Prevent open redirects.
        if (!Uri.IsWellFormedUriString(uri, UriKind.Relative))
        {
            uri = navigationManager.ToBaseRelativePath(uri);
        }

        // During static rendering, NavigateTo throws a NavigationException which is handled by the framework as a redirect.
        // So as long as this is called from a statically rendered Identity component, the InvalidOperationException is never thrown.
        navigationManager.NavigateTo(uri);
        throw new InvalidOperationException($"{nameof(IdentityRedirectManager)} can only be used during static rendering.");
    }

    [DebuggerHidden]
    [DebuggerNonUserCode]
    [DoesNotReturn]
    public void RedirectTo(string uri, Dictionary<string, object?> queryParameters)
    {
        var uriWithoutQuery = navigationManager.ToAbsoluteUri(uri).GetLeftPart(UriPartial.Path);
        var newUri = navigationManager.GetUriWithQueryParameters(uriWithoutQuery, queryParameters);
        RedirectTo(newUri);
    }

    [DebuggerHidden]
    [DebuggerNonUserCode]
    [DoesNotReturn]
    public void RedirectToWithStatus(string uri, string message, HttpContext context)
    {
        context.Response.Cookies.Append(StatusCookieName, message, StatusCookieBuilder.Build(context));
        RedirectTo(uri);
    }

    private string CurrentPath => navigationManager.ToAbsoluteUri(navigationManager.Uri).GetLeftPart(UriPartial.Path);

    [DebuggerHidden]
    [DebuggerNonUserCode]
    [DoesNotReturn]
    public void RedirectToCurrentPage() => RedirectTo(CurrentPath);

    [DebuggerHidden]
    [DebuggerNonUserCode]
    [DoesNotReturn]
    public void RedirectToCurrentPageWithStatus(string message, HttpContext context)
        => RedirectToWithStatus(CurrentPath, message, context);
}
