using AWBlazorApp.Features.Production.Models;
using FluentValidation;

namespace AWBlazorApp.Features.Production.Validators;

public sealed class CreateProductDescriptionValidator : AbstractValidator<CreateProductDescriptionRequest>
{
    public CreateProductDescriptionValidator()
    {
        RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required.").MaximumLength(400);
    }
}

public sealed class UpdateProductDescriptionValidator : AbstractValidator<UpdateProductDescriptionRequest>
{
    public UpdateProductDescriptionValidator()
    {
        When(x => x.Description is not null, () =>
        {
            RuleFor(x => x.Description!).NotEmpty().WithMessage("Description cannot be blanked out.").MaximumLength(400);
        });
    }
}
