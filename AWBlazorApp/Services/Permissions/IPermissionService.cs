using AWBlazorApp.Data;

namespace AWBlazorApp.Services.Permissions;

public interface IPermissionService
{
    Task<PermissionLevel> GetEffectivePermissionAsync(string userId, PermissionArea area);
    Task<bool> HasPermissionAsync(string userId, PermissionArea area, PermissionLevel required);
    Task<Dictionary<PermissionArea, PermissionLevel>> GetAllPermissionsAsync(string userId);
    void InvalidateCache(string userId);
}
