using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Shared.Api;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Admin));

        group.MapGet("/", ListUsersAsync)
            .WithName("ListUsers")
            .WithSummary("List Identity users. Admin only.");

        group.MapGet("/{id}", GetUserAsync)
            .WithName("GetUser")
            .WithSummary("Get a single Identity user by id. Admin only.");

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
}
