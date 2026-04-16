using AWBlazorApp.Features.Sales.Models;
using FluentValidation;

namespace AWBlazorApp.Features.Sales.Validators;

public sealed class CreateSpecialOfferProductValidator : AbstractValidator<CreateSpecialOfferProductRequest>
{
    public CreateSpecialOfferProductValidator()
    {
        RuleFor(x => x.SpecialOfferId).GreaterThan(0).WithMessage("SpecialOfferId is required.");
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("ProductId is required.");
    }
}

public sealed class UpdateSpecialOfferProductValidator : AbstractValidator<UpdateSpecialOfferProductRequest>
{
    public UpdateSpecialOfferProductValidator()
    {
        // No fields to validate — junction table.
    }
}
