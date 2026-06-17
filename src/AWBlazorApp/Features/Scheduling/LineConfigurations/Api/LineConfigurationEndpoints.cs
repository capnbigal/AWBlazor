using AWBlazorApp.Features.Scheduling.LineConfigurations.Domain;
using AWBlazorApp.Features.Scheduling.LineConfigurations.Dtos;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Shared.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Scheduling.LineConfigurations.Api;

public static class LineConfigurationEndpoints
{
    public static IEndpointRouteBuilder MapLineConfigurationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/scheduling/lines")
            .WithTags("Scheduling.Lines")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListLineConfigurations").WithSummary("List Scheduling.LineConfiguration rows.");

        group.MapCrudWithInterceptor<LineConfiguration, LineConfigurationDto, CreateLineConfigurationRequest, UpdateLineConfigurationRequest, int>(
            entityName: "LineConfiguration",
            routePrefix: "/api/scheduling/lines",
            entitySet: db => db.LineConfigurations,
            idSelector: e => e.Id,
            getId: e => e.Id,
            toDto: e => e.ToDto(),
            toEntity: r => r.ToEntity(),
            applyUpdate: (r, e) => r.ApplyTo(e));

        return app;
    }

    private static async Task<Ok<PagedResult<LineConfigurationDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] bool activeOnly = false, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.LineConfigurations.AsNoTracking();
        if (activeOnly) query = query.Where(l => l.IsActive);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.LocationId).Skip(skip).Take(take)
            .Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<LineConfigurationDto>(rows, total, skip, take));
    }
}
