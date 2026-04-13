using AWBlazorApp.Models.AdventureWorks;
using FluentValidation;

namespace AWBlazorApp.Validators.AdventureWorks;

public sealed class CreateProductSubcategoryValidator : AbstractValidator<CreateProductSubcategoryRequest>
{
    public CreateProductSubcategoryValidator()
    {
        RuleFor(x => x.ProductCategoryId).GreaterThan(0).WithMessage("Product category id is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.").MaximumLength(50);
    }
}

public sealed class UpdateProductSubcategoryValidator : AbstractValidator<UpdateProductSubcategoryRequest>
{
    public UpdateProductSubcategoryValidator()
    {
        When(x => x.ProductCategoryId.HasValue, () =>
        {
            RuleFor(x => x.ProductCategoryId!.Value).GreaterThan(0).WithMessage("Product category id must be positive.");
        });
        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name!).NotEmpty().WithMessage("Name cannot be blanked out.").MaximumLength(50);
        });
    }
}
