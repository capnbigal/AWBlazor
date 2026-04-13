using FluentValidation;
using FluentValidation.Internal;

namespace ElementaryApp.Validators;

/// <summary>
/// Adapter that lets a <see cref="MudBlazor.MudForm"/> reuse a FluentValidation
/// <see cref="IValidator{T}"/>. Bind to <c>Validation</c> on the form:
/// <code>
/// &lt;MudForm Validation="@(_validator.ValidateField)" Model="@request" /&gt;
/// </code>
/// </summary>
public sealed class MudFormValidator<T>(IValidator<T> validator)
{
    public Func<object, string, Task<IEnumerable<string>>> ValidateField =>
        async (model, propertyName) =>
        {
            var context = ValidationContext<T>.CreateWithOptions(
                (T)model,
                strategy => strategy.IncludeProperties(propertyName));

            var result = await validator.ValidateAsync(context);
            return result.IsValid
                ? []
                : result.Errors.Select(e => e.ErrorMessage);
        };

    public async Task<bool> ValidateAllAsync(T instance)
    {
        var result = await validator.ValidateAsync(instance);
        return result.IsValid;
    }
}
