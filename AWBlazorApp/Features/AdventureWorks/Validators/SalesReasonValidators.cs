using AWBlazorApp.Features.AdventureWorks.Models;
using FluentValidation;

namespace AWBlazorApp.Features.AdventureWorks.Validators;

public sealed class CreateSalesReasonValidator : AbstractValidator<CreateSalesReasonRequest>
{
    public CreateSalesReasonValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.").MaximumLength(50);
        RuleFor(x => x.ReasonType).NotEmpty().WithMessage("Reason type is required.").MaximumLength(50);
    }
}

public sealed class UpdateSalesReasonValidator : AbstractValidator<UpdateSalesReasonRequest>
{
    public UpdateSalesReasonValidator()
    {
        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name!).NotEmpty().WithMessage("Name cannot be blanked out.").MaximumLength(50);
        });
        When(x => x.ReasonType is not null, () =>
        {
            RuleFor(x => x.ReasonType!).NotEmpty().WithMessage("Reason type cannot be blanked out.").MaximumLength(50);
        });
    }
}
