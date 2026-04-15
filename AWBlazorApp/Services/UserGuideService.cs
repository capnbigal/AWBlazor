using System.Text.RegularExpressions;
using AWBlazorApp.Data.Entities.UserGuide;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Data.Entities;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AWBlazorApp.Services;

/// <summary>
/// Loads user-guide articles from <c>_posts/*.md</c> at startup (singleton) and provides
/// DB-backed read-tracking via <see cref="IDbContextFactory{TContext}"/>.
/// </summary>
public sealed class UserGuideService
{
    private static readonly Regex FilePattern = new(@"^(?<date>\d{4}-\d{2}-\d{2})[_-](?<slug>.+)\.md$", RegexOptions.Compiled);
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseSoftlineBreakAsHardlineBreak()
        .Build();
    private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    private readonly Dictionary<string, GuideArticle> _bySlug = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<GuideArticle> _sorted = [];
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public UserGuideService(IWebHostEnvironment env, IDbContextFactory<ApplicationDbContext> dbFactory, ILogger<UserGuideService> logger)
    {
        _dbFactory = dbFactory;

        var postsDir = Path.Combine(env.ContentRootPath, "_posts");
        if (!Directory.Exists(postsDir))
        {
            logger.LogWarning("_posts directory not found at {Path}", postsDir);
            return;
        }

        foreach (var file in Directory.GetFiles(postsDir, "*.md"))
        {
            var match = FilePattern.Match(Path.GetFileName(file));
            if (!match.Success) continue;

            try
            {
                var date = DateOnly.Parse(match.Groups["date"].Value);
                var slug = match.Groups["slug"].Value;
                var raw = File.ReadAllText(file);
                var (frontMatter, body) = ParseFrontMatter(raw);

                var article = new GuideArticle
                {
                    Slug = slug,
                    Date = date,
                    Title = frontMatter?.Title ?? slug,
                    Summary = frontMatter?.Summary,
                    Author = frontMatter?.Author,
                    Image = frontMatter?.Image,
                    Tags = frontMatter?.Tags?.AsReadOnly() ?? (IReadOnlyList<string>)[],
                    Category = frontMatter?.Category ?? "how-to",
                    MarkdownBody = body,
                    HtmlBody = new MarkupString(Markdown.ToHtml(body, Pipeline)),
                };

                _bySlug[slug] = article;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to parse article from {File}", file);
            }
        }

        _sorted = _bySlug.Values.OrderByDescending(a => a.Date).ThenBy(a => a.Title).ToList();
        logger.LogInformation("UserGuideService loaded {Count} articles from {Path}", _sorted.Count, postsDir);
    }

    // --- Content (in-memory, from disk) ---

    public IReadOnlyList<GuideArticle> All => _sorted;
    public GuideArticle? FindBySlug(string slug) => _bySlug.GetValueOrDefault(slug);
    public IReadOnlyList<GuideArticle> ByTag(string tag) =>
        _sorted.Where(a => a.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)).ToList();
    public IReadOnlyList<GuideArticle> ByCategory(string category) =>
        _sorted.Where(a => string.Equals(a.Category, category, StringComparison.OrdinalIgnoreCase)).ToList();
    public IReadOnlyList<string> AllTags =>
        _sorted.SelectMany(a => a.Tags).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(t => t).ToList();
    public IReadOnlyList<string> AllCategories =>
        _sorted.Select(a => a.Category).Where(c => c is not null).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(c => c!).ToList()!;

    // --- Read tracking (DB via IDbContextFactory) ---

    public async Task<List<GuideArticleDto>> GetAllForUserAsync(string? userId)
    {
        if (userId is null)
            return _sorted.Select(a => new GuideArticleDto(a, false)).ToList();

        await using var db = await _dbFactory.CreateDbContextAsync();
        var readSlugs = await db.ArticleReads
            .Where(r => r.UserId == userId)
            .Select(r => r.ArticleSlug)
            .ToListAsync();
        var readSet = new HashSet<string>(readSlugs, StringComparer.OrdinalIgnoreCase);

        return _sorted.Select(a => new GuideArticleDto(a, readSet.Contains(a.Slug))).ToList();
    }

    public async Task<GuideArticleDto?> GetArticleForUserAsync(string slug, string? userId)
    {
        var article = FindBySlug(slug);
        if (article is null) return null;
        if (userId is null) return new GuideArticleDto(article, false);

        await using var db = await _dbFactory.CreateDbContextAsync();
        var isRead = await db.ArticleReads.AnyAsync(r => r.UserId == userId && r.ArticleSlug == slug);
        return new GuideArticleDto(article, isRead);
    }

    public async Task<int> GetUnreadCountForUserAsync(string userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var readCount = await db.ArticleReads
            .Where(r => r.UserId == userId && _bySlug.Keys.Contains(r.ArticleSlug))
            .CountAsync();
        return _sorted.Count - readCount;
    }

    public async Task MarkAsReadAsync(string slug, string userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var exists = await db.ArticleReads.AnyAsync(r => r.UserId == userId && r.ArticleSlug == slug);
        if (exists) return;

        db.ArticleReads.Add(new ArticleRead
        {
            UserId = userId,
            ArticleSlug = slug,
            ReadDate = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
    }

    // --- Admin stats ---

    public async Task<List<ArticleReadStats>> GetReadStatsAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var totalUsers = await db.Users.CountAsync();
        var counts = await db.ArticleReads
            .GroupBy(r => r.ArticleSlug)
            .Select(g => new { Slug = g.Key, Count = g.Count() })
            .ToListAsync();

        var countDict = counts.ToDictionary(c => c.Slug, c => c.Count, StringComparer.OrdinalIgnoreCase);

        return _sorted.Select(a => new ArticleReadStats(
            a.Slug,
            a.Title,
            a.Category ?? "",
            countDict.GetValueOrDefault(a.Slug, 0),
            totalUsers
        )).ToList();
    }

    // --- Frontmatter parsing ---

    private static (FrontMatter? fm, string body) ParseFrontMatter(string raw)
    {
        if (!raw.StartsWith("---"))
            return (null, raw);

        var endIndex = raw.IndexOf("---", 3, StringComparison.Ordinal);
        if (endIndex < 0)
            return (null, raw);

        var yaml = raw[3..endIndex].Trim();
        var body = raw[(endIndex + 3)..].Trim();

        try
        {
            var fm = YamlDeserializer.Deserialize<FrontMatter>(yaml);
            return (fm, body);
        }
        catch
        {
            return (null, raw);
        }
    }

    private sealed class FrontMatter
    {
        public string? Title { get; set; }
        public string? Summary { get; set; }
        public string? Author { get; set; }
        public string? Image { get; set; }
        public List<string>? Tags { get; set; }
        public string? Category { get; set; }
    }
}

public sealed record GuideArticle
{
    public required string Slug { get; init; }
    public required DateOnly Date { get; init; }
    public required string Title { get; init; }
    public string? Summary { get; init; }
    public string? Author { get; init; }
    public string? Image { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
    public string? Category { get; init; }
    public required string MarkdownBody { get; init; }
    public required MarkupString HtmlBody { get; init; }
}

public sealed record GuideArticleDto(GuideArticle Article, bool IsRead);

public sealed record ArticleReadStats(string Slug, string Title, string Category, int ReadCount, int TotalUsers);
