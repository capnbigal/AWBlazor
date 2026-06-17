using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Features.Admin.Application;
using AWBlazorApp.Infrastructure.Authentication;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Shared.Api;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        // Accept either an Identity cookie OR an X-Api-Key (the standard "ApiOrCookie" scheme set
        // used across the API), then require the Admin role. A bare RequireRole here would fall back
        // to the default (cookie) scheme only, making the whole group unusable via API key.
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization(new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(
                    Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme,
                    ApiKeyAuthenticationOptions.Scheme)
                .RequireAuthenticatedUser()
                .RequireRole(AppRoles.Admin)
                .Build());

        group.MapGet("/", ListUsersAsync)
            .WithName("ListUsers")
            .WithSummary("List Identity users. Admin only.");

        group.MapGet("/{id}", GetUserAsync)
            .WithName("GetUser")
            .WithSummary("Get a single Identity user by id. Admin only.");

        group.MapPost("/{id}/reset-password", ResetUserPasswordAsync)
            .WithName("ResetUserPassword")
            .WithSummary("Reset a user's password (admin). Does not require the current password; rotates the security stamp and invalidates the user's sessions.");

        return app;
    }

    private static async Task<Ok<PagedResult<UserDto>>> ListUsersAsync(
        UserManager<ApplicationUser> userManager,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);

        var query = userManager.Users.AsNoTracking().OrderBy(u => u.UserName);
        var total = await query.CountAsync(ct);
        var rows = await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return TypedResults.Ok(new PagedResult<UserDto>(
            rows.Select(u => u.ToDto()).ToList(),
            total, skip, take));
    }

    private static async Task<Results<Ok<UserDto>, NotFound>> GetUserAsync(
        string id, UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.FindByIdAsync(id);
        return user is null ? TypedResults.NotFound() : TypedResults.Ok(user.ToDto());
    }

    private static async Task<Results<Ok, NotFound, ValidationProblem>> ResetUserPasswordAsync(
        string id,
        ResetUserPasswordRequest request,
        UserManager<ApplicationUser> userManager)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["newPassword"] = ["A new password is required."],
            });
        }

        var user = await userManager.FindByIdAsync(id);
        if (user is null) return TypedResults.NotFound();

        var result = await AdminUserManagement.ResetPasswordAsync(userManager, user, request.NewPassword);
        if (!result.Succeeded)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["newPassword"] = result.Errors.Select(e => e.Description).ToArray(),
            });
        }

        return TypedResults.Ok();
    }
}

/// <summary>Request body for the admin password-reset endpoint (<c>POST /api/users/{id}/reset-password</c>).</summary>
public sealed record ResetUserPasswordRequest(string NewPassword);
