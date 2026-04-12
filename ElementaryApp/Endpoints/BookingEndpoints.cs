using ElementaryApp.Data;
using ElementaryApp.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElementaryApp.Endpoints;

public static class BookingEndpoints
{
    public static IEndpointRouteBuilder MapBookingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bookings")
            .WithTags("Bookings")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListBookingsAsync)
            .WithName("ListBookings")
            .WithSummary("List bookings, excluding soft-deleted records.");

        group.MapGet("/{id:int}", GetBookingAsync)
            .WithName("GetBooking")
            .WithSummary("Get a single booking by id.");

        group.MapPost("/", CreateBookingAsync)
            .WithName("CreateBooking")
            .WithSummary("Create a new booking. Requires Employee role.")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));

        group.MapPatch("/{id:int}", UpdateBookingAsync)
            .WithName("UpdateBooking")
            .WithSummary("Update a booking. Requires Employee role.")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));

        group.MapDelete("/{id:int}", SoftDeleteBookingAsync)
            .WithName("DeleteBooking")
            .WithSummary("Soft-delete a booking by setting DeletedDate. Requires Manager role.")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        return app;
    }

    private static async Task<Ok<PagedResult<BookingDto>>> ListBookingsAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        [FromQuery] bool includeDeleted = false,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);

        var query = db.Bookings.AsNoTracking();
        if (!includeDeleted)
        {
            query = query.Where(b => b.DeletedDate == null);
        }

        var total = await query.CountAsync(ct);
        var rows = await query
            .OrderByDescending(b => b.CreatedDate)
            .Skip(skip)
            .Take(take)
            .Select(b => b.ToDto())
            .ToListAsync(ct);

        return TypedResults.Ok(new PagedResult<BookingDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<BookingDto>, NotFound>> GetBookingAsync(
        int id, ApplicationDbContext db, CancellationToken ct)
    {
        var booking = await db.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, ct);
        return booking is null ? TypedResults.NotFound() : TypedResults.Ok(booking.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateBookingAsync(
        CreateBookingRequest request,
        IValidator<CreateBookingRequest> validator,
        ApplicationDbContext db,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var entity = request.ToEntity();
        db.Bookings.Add(entity);
        await db.SaveChangesAsync(ct);

        return TypedResults.Created($"/api/bookings/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateBookingAsync(
        int id,
        UpdateBookingRequest request,
        IValidator<UpdateBookingRequest> validator,
        ApplicationDbContext db,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var entity = await db.Bookings.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        request.ApplyTo(entity);
        await db.SaveChangesAsync(ct);

        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> SoftDeleteBookingAsync(
        int id, ApplicationDbContext db, HttpContext http, CancellationToken ct)
    {
        var entity = await db.Bookings.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();

        entity.DeletedDate = DateTime.UtcNow;
        entity.DeletedBy = http.User.Identity?.Name;
        await db.SaveChangesAsync(ct);

        return TypedResults.NoContent();
    }
}
