using AWBlazorApp.Features.Production.Models;
using FluentValidation;

namespace AWBlazorApp.Features.Production.Validators;

public sealed class CreateProductCategoryValidator : AbstractValidator<CreateProductCategoryRequest>
{
    public CreateProductCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.").MaximumLength(50);
    }
}

public sealed class UpdateProductCategoryValidator : AbstractValidator<UpdateProductCategoryRequest>
{
    public UpdateProductCategoryValidator()
    {
        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name!).NotEmpty().WithMessage("Name cannot be blanked out.").MaximumLength(50);
        });
    }
}
