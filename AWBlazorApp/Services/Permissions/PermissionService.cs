using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AWBlazorApp.Services.Permissions;

/// <summary>
/// Resolves effective permission level for a user on a given area.
/// Effective = Max(roleDefault, explicitGrant). Cached in IMemoryCache with 5-min TTL.
/// </summary>
public sealed class PermissionService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    IMemoryCache cache,
    UserManager<ApplicationUser> userManager) : IPermissionService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public async Task<PermissionLevel> GetEffectivePermissionAsync(string userId, PermissionArea area)
    {
        var all = await GetAllPermissionsAsync(userId);
        return all.GetValueOrDefault(area, PermissionLevel.None);
    }

    public async Task<bool> HasPermissionAsync(string userId, PermissionArea area, PermissionLevel required)
    {
        var effective = await GetEffectivePermissionAsync(userId, area);
        return effective >= required;
    }

    public async Task<Dictionary<PermissionArea, PermissionLevel>> GetAllPermissionsAsync(string userId)
    {
        var cacheKey = $"perms:{userId}";
        if (cache.TryGetValue(cacheKey, out Dictionary<PermissionArea, PermissionLevel>? cached) && cached is not null)
            return cached;

        // Determine role-based floor
        var user = await userManager.FindByIdAsync(userId);
        var roleFloor = PermissionLevel.None;
        if (user is not null)
        {
            var roles = await userManager.GetRolesAsync(user);
            if (roles.Contains(AppRoles.Admin))
                roleFloor = PermissionLevel.Admin;
            else if (roles.Contains(AppRoles.Manager))
                roleFloor = PermissionLevel.Write;
            else if (roles.Contains(AppRoles.Employee))
                roleFloor = PermissionLevel.Read;
        }

        // Load explicit grants
        await using var db = await dbFactory.CreateDbContextAsync();
        var grants = await db.UserAreaPermissions
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .ToDictionaryAsync(p => p.Area, p => p.PermissionLevel);

        // Build effective permissions: Max(roleFloor, explicitGrant) per area
        var result = new Dictionary<PermissionArea, PermissionLevel>();
        foreach (var area in Enum.GetValues<PermissionArea>())
        {
            var explicitLevel = grants.GetValueOrDefault(area, PermissionLevel.None);
            result[area] = (PermissionLevel)Math.Max((int)roleFloor, (int)explicitLevel);
        }

        cache.Set(cacheKey, result, CacheTtl);
        return result;
    }

    public void InvalidateCache(string userId)
    {
        cache.Remove($"perms:{userId}");
    }
}
