using System.Text.RegularExpressions;
using Markdig;
using Microsoft.AspNetCore.Components;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ElementaryApp.Services;

/// <summary>
/// One blog post parsed from a `_posts/YYYY-MM-DD_slug.md` file.
/// </summary>
public sealed record BlogPost
{
    public required string Slug { get; init; }
    public required DateOnly Date { get; init; }
    public required string Title { get; init; }
    public string? Summary { get; init; }
    public string? Author { get; init; }
    public string? Image { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
    public required string MarkdownBody { get; init; }
    public required MarkupString HtmlBody { get; init; }
}

/// <summary>
/// Loads markdown posts from <c>_posts/</c> on startup, parses YAML frontmatter, and renders
/// the body to HTML with Markdig. This is a deliberately lightweight replacement for the
/// 34KB ServiceStack-era <c>MarkdownPagesBase.cs</c> — just enough to power the blog index +
/// a single post page.
/// </summary>
public sealed class MarkdownBlogService
{
    private static readonly Regex FrontMatterRegex = new(
        @"^---\s*\r?\n(?<yaml>.*?)\r?\n---\s*\r?\n(?<body>.*)$",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex FilenameRegex = new(
        @"^(?<date>\d{4}-\d{2}-\d{2})[_-](?<slug>.+)\.md$",
        RegexOptions.Compiled);

    private readonly Dictionary<string, BlogPost> _bySlug;
    private readonly List<BlogPost> _ordered;

    public MarkdownBlogService(IWebHostEnvironment env, ILogger<MarkdownBlogService> logger)
    {
        _bySlug = new Dictionary<string, BlogPost>(StringComparer.OrdinalIgnoreCase);
        _ordered = new List<BlogPost>();

        var postsDir = Path.Combine(env.ContentRootPath, "_posts");
        if (!Directory.Exists(postsDir))
        {
            logger.LogInformation("No _posts directory at {Path} — blog will be empty.", postsDir);
            return;
        }

        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseSoftlineBreakAsHardlineBreak()
            .Build();

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        foreach (var file in Directory.EnumerateFiles(postsDir, "*.md"))
        {
            try
            {
                var name = Path.GetFileName(file);
                var match = FilenameRegex.Match(name);
                if (!match.Success)
                {
                    logger.LogWarning("Skipping {File}: filename does not match YYYY-MM-DD_slug.md", name);
                    continue;
                }

                var date = DateOnly.ParseExact(match.Groups["date"].Value, "yyyy-MM-dd");
                var slug = match.Groups["slug"].Value.ToLowerInvariant();

                var raw = File.ReadAllText(file);
                var fm = FrontMatterRegex.Match(raw);

                FrontMatter? meta = null;
                string body = raw;
                if (fm.Success)
                {
                    try
                    {
                        meta = deserializer.Deserialize<FrontMatter>(fm.Groups["yaml"].Value);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to parse YAML frontmatter for {File}", name);
                    }
                    body = fm.Groups["body"].Value;
                }

                var html = Markdown.ToHtml(body, pipeline);

                var post = new BlogPost
                {
                    Slug = slug,
                    Date = date,
                    Title = meta?.Title ?? slug,
                    Summary = meta?.Summary,
                    Author = meta?.Author,
                    Image = meta?.Image,
                    Tags = meta?.Tags ?? [],
                    MarkdownBody = body,
                    HtmlBody = new MarkupString(html),
                };

                _bySlug[slug] = post;
                _ordered.Add(post);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to load blog post from {File}", file);
            }
        }

        _ordered.Sort((a, b) => b.Date.CompareTo(a.Date));
        logger.LogInformation("Loaded {Count} blog posts from {Path}", _ordered.Count, postsDir);
    }

    public IReadOnlyList<BlogPost> All => _ordered;

    public BlogPost? FindBySlug(string slug) =>
        _bySlug.TryGetValue(slug, out var post) ? post : null;

    public IReadOnlyList<BlogPost> ByTag(string tag) =>
        _ordered.Where(p => p.Tags.Any(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase))).ToList();

    public IReadOnlyList<string> AllTags =>
        _ordered.SelectMany(p => p.Tags).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(t => t).ToList();

    private sealed class FrontMatter
    {
        public string? Title { get; set; }
        public string? Summary { get; set; }
        public string? Author { get; set; }
        public string? Image { get; set; }
        public List<string>? Tags { get; set; }
    }
}
