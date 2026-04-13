using System.Text.RegularExpressions;

namespace AWBlazorApp.Tests;

/// <summary>
/// Helper for testing Blazor static-SSR forms. Performs the standard browser flow:
///   1. GET the page (which sets the antiforgery cookie + renders a hidden token)
///   2. Parse the token out of the response HTML
///   3. POST the form back with the token + the form fields + the form's <c>_handler</c> name
///
/// The same <see cref="HttpClient"/> instance is reused across the GET and POST so the
/// antiforgery cookie persists.
/// </summary>
public static class FormPostHelper
{
    private static readonly Regex AntiforgeryTokenRegex = new(
        @"<input[^>]+name=""__RequestVerificationToken""[^>]+value=""([^""]+)""",
        RegexOptions.Compiled);

    /// <summary>
    /// GETs the page, extracts the antiforgery token, and POSTs the form back with the token,
    /// the <c>_handler</c> form name, and the supplied fields. The HTTP client must allow
    /// cookies to persist (the default <see cref="HttpClient"/> from
    /// <c>WebApplicationFactory.CreateClient</c> does).
    /// </summary>
    /// <param name="client">An HTTP client whose handler stores cookies between requests.</param>
    /// <param name="path">Page path (e.g. <c>"/Account/Login"</c>).</param>
    /// <param name="formName">The Blazor <c>FormName</c> the page declared on its EditForm.</param>
    /// <param name="fields">Form field name → value pairs.</param>
    public static async Task<HttpResponseMessage> PostFormAsync(
        HttpClient client,
        string path,
        string formName,
        IDictionary<string, string> fields,
        CancellationToken cancellationToken = default)
    {
        var getResponse = await client.GetAsync(path, cancellationToken);
        if (!getResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"GET {path} returned {(int)getResponse.StatusCode} {getResponse.StatusCode}; expected 2xx so we could grab the antiforgery token.");
        }

        var html = await getResponse.Content.ReadAsStringAsync(cancellationToken);
        var match = AntiforgeryTokenRegex.Match(html);
        if (!match.Success)
        {
            throw new InvalidOperationException(
                $"GET {path} response did not contain a __RequestVerificationToken hidden input. " +
                "The page may not be rendering an <AntiforgeryToken /> or <EditForm method=\"post\">.");
        }

        var token = match.Groups[1].Value;

        var formData = new Dictionary<string, string>(fields, StringComparer.Ordinal)
        {
            ["__RequestVerificationToken"] = token,
            ["_handler"] = formName,
        };

        var content = new FormUrlEncodedContent(formData);
        return await client.PostAsync(path, content, cancellationToken);
    }
}
