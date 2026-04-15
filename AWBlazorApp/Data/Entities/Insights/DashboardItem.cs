namespace AWBlazorApp.Data.Entities.Insights;

/// <summary>
/// A saved-query reference pinned to a user's personal dashboard. Each row has a DisplayOrder
/// (sortable position) and a Width (grid column span at md and above: 12/6/4/3).
/// </summary>
public class DashboardItem
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public int SavedQueryId { get; set; }

    public int DisplayOrder { get; set; }

    /// <summary>MudGrid column span at md+ breakpoints. Valid: 3, 4, 6, 12.</summary>
    public int Width { get; set; } = 6;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
