using AWBlazorApp.Data;
using AWBlazorApp.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Admin));

        group.MapGet("/data", async Task<Ok<AdminDataResponse>> (ApplicationDbContext db, CancellationToken ct) =>
        {
            var forecasts = await db.ForecastDefinitions.CountAsync(f => f.DeletedDate == null, ct);
            var toolSlots = await db.ToolSlotConfigurations.CountAsync(ct);

            return TypedResults.Ok(new AdminDataResponse(
            [
                new PageStats("Forecasts", forecasts),
                new PageStats("ToolSlotConfigurations", toolSlots),
            ]));
        })
        .WithName("GetAdminData")
        .WithSummary("Dashboard counts for the admin landing page.");

        return app;
    }
}
