using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Services.Permissions;
using AWBlazorApp.Data.Entities.Auth;
using AWBlazorApp.Data.Entities;
using AWBlazorApp.Services;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Endpoints.Admin;

public static class PermissionEndpoints
{
    public static IEndpointRouteBuilder MapPermissionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/permissions")
            .WithTags("Permissions")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Admin));

        // List all explicit permission grants
        group.MapGet("/", async (IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var grants = await db.UserAreaPermissions.AsNoTracking()
                .OrderBy(p => p.UserId).ThenBy(p => p.Area)
                .Select(p => new
                {
                    p.Id,
                    p.UserId,
                    UserEmail = p.User != null ? p.User.Email : null,
                    Area = p.Area.ToString(),
                    PermissionLevel = p.PermissionLevel.ToString(),
                })
                .ToListAsync();
            return Results.Ok(grants);
        });

        // Get permissions for a specific user
        group.MapGet("/{userId}", async (string userId, IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var grants = await db.UserAreaPermissions.AsNoTracking()
                .Where(p => p.UserId == userId)
                .Select(p => new { Area = p.Area.ToString(), PermissionLevel = p.PermissionLevel.ToString() })
                .ToListAsync();
            return Results.Ok(grants);
        });

        // Set permission for a user+area (upsert)
        group.MapPut("/{userId}/{area}", async (
            string userId, PermissionArea area, SetPermissionRequest request,
            IDbContextFactory<ApplicationDbContext> dbFactory, IPermissionService permissionService) =>
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var existing = await db.UserAreaPermissions
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Area == area);

            if (existing is not null)
            {
                existing.PermissionLevel = request.Level;
            }
            else
            {
                db.UserAreaPermissions.Add(new UserAreaPermission
                {
                    UserId = userId,
                    Area = area,
                    PermissionLevel = request.Level,
                });
            }

            await db.SaveChangesAsync();
            permissionService.InvalidateCache(userId);
            return Results.Ok();
        });

        // Remove explicit grant (revert to role default)
        group.MapDelete("/{userId}/{area}", async (
            string userId, PermissionArea area,
            IDbContextFactory<ApplicationDbContext> dbFactory, IPermissionService permissionService) =>
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var existing = await db.UserAreaPermissions
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Area == area);

            if (existing is not null)
            {
                db.UserAreaPermissions.Remove(existing);
                await db.SaveChangesAsync();
            }

            permissionService.InvalidateCache(userId);
            return Results.NoContent();
        });

        return app;
    }

    public sealed record SetPermissionRequest(PermissionLevel Level);
}
