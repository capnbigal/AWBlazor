using AWBlazorApp.Features.Scheduling.LineProductAssignments.Domain;
using AWBlazorApp.Features.Scheduling.LineProductAssignments.Dtos;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Shared.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Scheduling.LineProductAssignments.Api;

public static class LineProductAssignmentEndpoints
{
    public static IEndpointRouteBuilder MapLineProductAssignmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/scheduling/line-products")
            .WithTags("Scheduling.LineProducts")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListLineProductAssignments").WithSummary("List Scheduling.LineProductAssignment rows.");

        group.MapCrudWithInterceptor<LineProductAssignment, LineProductAssignmentDto, CreateLineProductAssignmentRequest, UpdateLineProductAssignmentRequest, int>(
            entityName: "LineProductAssignment",
            routePrefix: "/api/scheduling/line-products",
            entitySet: db => db.LineProductAssignments,
            idSelector: e => e.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e));

        return app;
    }

    private static async Task<Ok<PagedResult<LineProductAssignmentDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 100,
        [FromQuery] short? locationId = null, [FromQuery] bool activeOnly = false,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.LineProductAssignments.AsNoTracking();
        if (locationId.HasValue) query = query.Where(l => l.LocationId == locationId.Value);
        if (activeOnly) query = query.Where(l => l.IsActive);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.LocationId).ThenBy(x => x.ProductModelId)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<LineProductAssignmentDto>(rows, total, skip, take));
    }
}
