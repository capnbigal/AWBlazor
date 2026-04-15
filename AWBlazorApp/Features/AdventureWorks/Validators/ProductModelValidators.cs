using AWBlazorApp.Features.AdventureWorks.Models;
using FluentValidation;

namespace AWBlazorApp.Features.AdventureWorks.Validators;

public sealed class CreateProductModelValidator : AbstractValidator<CreateProductModelRequest>
{
    public CreateProductModelValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
    }
}

public sealed class UpdateProductModelValidator : AbstractValidator<UpdateProductModelRequest>
{
    public UpdateProductModelValidator()
    {
        When(x => x.Name is not null, () => RuleFor(x => x.Name!).NotEmpty().MaximumLength(50));
    }
}
