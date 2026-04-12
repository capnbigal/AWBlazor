using ElementaryApp.Data;
using ElementaryApp.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElementaryApp.Endpoints;

public static class CouponEndpoints
{
    public static IEndpointRouteBuilder MapCouponEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/coupons")
            .WithTags("Coupons")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListCouponsAsync)
            .WithName("ListCoupons")
            .WithSummary("List all coupons.");

        group.MapGet("/{id}", GetCouponAsync)
            .WithName("GetCoupon")
            .WithSummary("Get a single coupon by id.");

        group.MapPost("/", CreateCouponAsync)
            .WithName("CreateCoupon")
            .WithSummary("Create a new coupon. Requires Employee role.")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));

        group.MapPatch("/{id}", UpdateCouponAsync)
            .WithName("UpdateCoupon")
            .WithSummary("Update a coupon. Requires Employee role.")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));

        group.MapDelete("/{id}", DeleteCouponAsync)
            .WithName("DeleteCoupon")
            .WithSummary("Delete a coupon. Requires Manager role.")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        return app;
    }

    private static async Task<Ok<PagedResult<CouponDto>>> ListCouponsAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);

        var query = db.Coupons.AsNoTracking();
        var total = await query.CountAsync(ct);
        var rows = await query
            .OrderBy(c => c.Id)
            .Skip(skip)
            .Take(take)
            .Select(c => c.ToDto())
            .ToListAsync(ct);

        return TypedResults.Ok(new PagedResult<CouponDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<CouponDto>, NotFound>> GetCouponAsync(
        string id, ApplicationDbContext db, CancellationToken ct)
    {
        var coupon = await db.Coupons.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
        return coupon is null ? TypedResults.NotFound() : TypedResults.Ok(coupon.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, Conflict, ValidationProblem>> CreateCouponAsync(
        CreateCouponRequest request,
        IValidator<CreateCouponRequest> validator,
        ApplicationDbContext db,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        if (await db.Coupons.AnyAsync(c => c.Id == request.Id, ct))
        {
            return TypedResults.Conflict();
        }

        var entity = request.ToEntity();
        db.Coupons.Add(entity);
        await db.SaveChangesAsync(ct);

        return TypedResults.Created($"/api/coupons/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateCouponAsync(
        string id,
        UpdateCouponRequest request,
        IValidator<UpdateCouponRequest> validator,
        ApplicationDbContext db,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var entity = await db.Coupons.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        request.ApplyTo(entity);
        await db.SaveChangesAsync(ct);

        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteCouponAsync(
        string id, ApplicationDbContext db, CancellationToken ct)
    {
        var entity = await db.Coupons.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        db.Coupons.Remove(entity);
        await db.SaveChangesAsync(ct);

        return TypedResults.NoContent();
    }
}
