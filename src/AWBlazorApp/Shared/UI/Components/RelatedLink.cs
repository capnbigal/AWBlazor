namespace AWBlazorApp.Shared.UI.Components;

/// <summary>
/// One entry in a <c>RelatedLinksCard</c> "related records" rail on a detail page.
/// </summary>
/// <param name="Label">Link text (e.g. "Customer #29485").</param>
/// <param name="Href">Target route (e.g. "aw/customers/29485").</param>
/// <param name="Icon">Material icon shown beside the link.</param>
/// <param name="Detail">Optional secondary caption (e.g. a count or status).</param>
public sealed record RelatedLink(string Label, string Href, string Icon, string? Detail = null);
