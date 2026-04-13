using AWBlazorApp.Models.AdventureWorks;
using FluentValidation;

namespace AWBlazorApp.Validators.AdventureWorks;

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
