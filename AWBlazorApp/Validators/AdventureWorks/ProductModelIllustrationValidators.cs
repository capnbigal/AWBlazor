using AWBlazorApp.Models.AdventureWorks;
using FluentValidation;

namespace AWBlazorApp.Validators.AdventureWorks;

public sealed class CreateProductModelIllustrationValidator : AbstractValidator<CreateProductModelIllustrationRequest>
{
    public CreateProductModelIllustrationValidator()
    {
        RuleFor(x => x.ProductModelId).GreaterThan(0).WithMessage("ProductModelId is required.");
        RuleFor(x => x.IllustrationId).GreaterThan(0).WithMessage("IllustrationId is required.");
    }
}

public sealed class UpdateProductModelIllustrationValidator : AbstractValidator<UpdateProductModelIllustrationRequest>
{
    public UpdateProductModelIllustrationValidator()
    {
        // No fields to validate — junction table.
    }
}
