using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.App.Middleware;

/// <summary>
/// Converts unhandled exceptions thrown by <c>/api/*</c> endpoints into RFC-7807
/// <c>application/problem+json</c> responses. Requests outside <c>/api</c> (Blazor pages, SignalR,
/// static assets) are left untouched — the exception is re-thrown so the existing
/// <c>UseExceptionHandler("/Error")</c> page (production) or the developer exception page
/// (development) handles them exactly as before.
/// </summary>
/// <remarks>
/// Placed just outside the endpoint/authorization middleware so it wraps endpoint execution.
/// Mappings: <see cref="DbUpdateConcurrencyException"/>/<see cref="DbUpdateException"/> → 409 (a
/// duplicate, a concurrency clash, or a foreign-key violation), <see cref="FluentValidation.ValidationException"/>
/// → 400, <see cref="BadHttpRequestException"/> → 400, everything else → 500. Internal details are
/// logged, never leaked to the caller.
/// </remarks>
public sealed class ApiProblemDetailsMiddleware(RequestDelegate next, ILogger<ApiProblemDetailsMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, IProblemDetailsService problemDetails)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex) when (context.Request.Path.StartsWithSegments("/api"))
        {
            if (context.Response.HasStarted)
            {
                logger.LogError(ex, "Unhandled API exception after the response had started for {Method} {Path}",
                    context.Request.Method, context.Request.Path);
                throw;
            }

            var (status, title) = ex switch
            {
                DbUpdateConcurrencyException => (StatusCodes.Status409Conflict,
                    "The record was modified by another user. Reload and try again."),
                // Only a genuine constraint violation (unique/PK/FK) is a 409. A DbUpdateException can
                // also wrap a transient fault (deadlock, timeout, dropped connection) — those are
                // server errors and must fall through to 500, not be mislabelled as conflicts.
                DbUpdateException due when IsConstraintViolation(due) => (StatusCodes.Status409Conflict,
                    "The operation conflicts with existing data (a duplicate value, or a referenced record)."),
                FluentValidation.ValidationException => (StatusCodes.Status400BadRequest,
                    "One or more validation errors occurred."),
                BadHttpRequestException => (StatusCodes.Status400BadRequest,
                    "The request was malformed."),
                _ => (StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred."),
            };

            if (status >= 500)
                logger.LogError(ex, "Unhandled API exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            else
                logger.LogWarning(ex, "API request failed ({Status}) for {Method} {Path}", status, context.Request.Method, context.Request.Path);

            context.Response.Clear();
            context.Response.StatusCode = status;

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Type = $"https://httpstatuses.io/{status}",
            };

            // TryWriteAsync returns false when the client's Accept header is incompatible with the
            // problem-details writer; in that case still return a JSON body rather than a bare status.
            var wrote = await problemDetails.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = problem,
            });
            if (!wrote)
            {
                await context.Response.WriteAsJsonAsync(problem, (System.Text.Json.JsonSerializerOptions?)null, "application/problem+json");
            }
        }
    }

    // 2627 = unique constraint, 2601 = duplicate key in a unique index, 547 = FK/check constraint.
    private static bool IsConstraintViolation(DbUpdateException ex) =>
        ex.GetBaseException() is SqlException { Number: 2627 or 2601 or 547 };
}
