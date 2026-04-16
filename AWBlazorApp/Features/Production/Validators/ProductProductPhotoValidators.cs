using AWBlazorApp.Features.Production.Models;
using FluentValidation;

namespace AWBlazorApp.Features.Production.Validators;

public sealed class CreateProductProductPhotoValidator : AbstractValidator<CreateProductProductPhotoRequest>
{
    public CreateProductProductPhotoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("ProductId is required.");
        RuleFor(x => x.ProductPhotoId).GreaterThan(0).WithMessage("ProductPhotoId is required.");
    }
}

public sealed class UpdateProductProductPhotoValidator : AbstractValidator<UpdateProductProductPhotoRequest>
{
    public UpdateProductProductPhotoValidator() { }
}
