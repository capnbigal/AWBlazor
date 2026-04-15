using FluentValidation;

namespace AWBlazorApp.Shared.Endpoints;

internal static class ValidationExtensions
{
    /// <summary>
    /// Runs the registered <see cref="IValidator{T}"/> for the supplied request and returns
    /// a 400 ValidationProblem result on failure. Returns <c>null</c> on success.
    /// </summary>
    public static async Task<IResult?> ValidateAsync<T>(this IValidator<T>? validator, T request)
    {
        if (validator is null) return null;

        var result = await validator.ValidateAsync(request);
        if (result.IsValid) return null;

        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return Results.ValidationProblem(errors);
    }
}
