using AWBlazorApp.Models.AdventureWorks;
using FluentValidation;

namespace AWBlazorApp.Validators.AdventureWorks;

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
