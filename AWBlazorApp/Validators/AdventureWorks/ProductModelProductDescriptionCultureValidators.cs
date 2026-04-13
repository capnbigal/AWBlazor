using AWBlazorApp.Models.AdventureWorks;
using FluentValidation;

namespace AWBlazorApp.Validators.AdventureWorks;

public sealed class CreateProductModelProductDescriptionCultureValidator : AbstractValidator<CreateProductModelProductDescriptionCultureRequest>
{
    public CreateProductModelProductDescriptionCultureValidator()
    {
        RuleFor(x => x.ProductModelId).GreaterThan(0).WithMessage("ProductModelId is required.");
        RuleFor(x => x.ProductDescriptionId).GreaterThan(0).WithMessage("ProductDescriptionId is required.");
        RuleFor(x => x.CultureId).NotEmpty().MaximumLength(6).WithMessage("CultureId is required (max 6 chars).");
    }
}

public sealed class UpdateProductModelProductDescriptionCultureValidator : AbstractValidator<UpdateProductModelProductDescriptionCultureRequest>
{
    public UpdateProductModelProductDescriptionCultureValidator()
    {
        // No fields to validate — junction table.
    }
}
