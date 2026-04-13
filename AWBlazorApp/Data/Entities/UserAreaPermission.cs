namespace AWBlazorApp.Data.Entities;

/// <summary>
/// Explicit per-area permission grant for a user. One row per (UserId, Area).
/// Grants only elevate above the role-based default — they never restrict.
/// </summary>
public class UserAreaPermission
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public PermissionArea Area { get; set; }

    public PermissionLevel PermissionLevel { get; set; }

    public ApplicationUser? User { get; set; }
}
