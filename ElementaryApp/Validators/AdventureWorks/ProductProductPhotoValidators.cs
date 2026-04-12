using ElementaryApp.Models.AdventureWorks;
using FluentValidation;

namespace ElementaryApp.Validators.AdventureWorks;

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
