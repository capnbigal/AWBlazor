using AWBlazorApp.Features.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace AWBlazorApp.Features.Admin.Application;

/// <summary>
/// Admin-initiated user operations that do not require the target user's current credentials.
/// Shared by the admin Users page (<c>/admin/users</c>) and the admin user endpoints
/// (<c>/api/users</c>) so both rotate passwords through one identical, audited code path.
/// </summary>
public static class AdminUserManagement
{
    /// <summary>
    /// Resets a user's password WITHOUT knowing the old one (an administrator action). Generates a
    /// password-reset token and applies the new password via <see cref="UserManager{TUser}"/>, so the
    /// password is rehashed with the app's configured hasher and the security stamp is rotated
    /// (invalidating the user's existing sign-in sessions). Returns the <see cref="IdentityResult"/>
    /// so callers can surface password-policy failures to the user instead of throwing.
    /// </summary>
    public static async Task<IdentityResult> ResetPasswordAsync(
        UserManager<ApplicationUser> userManager, ApplicationUser user, string newPassword)
    {
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        return await userManager.ResetPasswordAsync(user, token, newPassword);
    }
}
