using AWBlazorApp.Features.Production.Dtos;
using FluentValidation;

namespace AWBlazorApp.Features.Production.Validators;

public sealed class CreateScrapReasonValidator : AbstractValidator<CreateScrapReasonRequest>
{
    public CreateScrapReasonValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.").MaximumLength(50);
    }
}

public sealed class UpdateScrapReasonValidator : AbstractValidator<UpdateScrapReasonRequest>
{
    public UpdateScrapReasonValidator()
    {
        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name!).NotEmpty().WithMessage("Name cannot be blanked out.").MaximumLength(50);
        });
    }
}
