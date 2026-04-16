using AWBlazorApp.Features.Person.Models;
using FluentValidation;

namespace AWBlazorApp.Features.Person.Validators;

public sealed class CreateBusinessEntityValidator : AbstractValidator<CreateBusinessEntityRequest>
{
    public CreateBusinessEntityValidator()
    {
        // No fields — BusinessEntity has no editable data of its own.
    }
}

public sealed class UpdateBusinessEntityValidator : AbstractValidator<UpdateBusinessEntityRequest>
{
    public UpdateBusinessEntityValidator()
    {
        // No fields — BusinessEntity has no editable data of its own.
    }
}
