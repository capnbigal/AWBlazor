using AWBlazorApp.Features.AdventureWorks.Models;
using FluentValidation;

namespace AWBlazorApp.Features.AdventureWorks.Validators;

public sealed class CreateEmailAddressValidator : AbstractValidator<CreateEmailAddressRequest>
{
    public CreateEmailAddressValidator()
    {
        RuleFor(x => x.BusinessEntityId).GreaterThan(0).WithMessage("BusinessEntityId is required.");
        RuleFor(x => x.EmailAddressValue)
            .NotEmpty().WithMessage("EmailAddressValue is required.")
            .MaximumLength(50)
            .EmailAddress();
    }
}

public sealed class UpdateEmailAddressValidator : AbstractValidator<UpdateEmailAddressRequest>
{
    public UpdateEmailAddressValidator()
    {
        When(x => x.EmailAddressValue is not null, () =>
            RuleFor(x => x.EmailAddressValue!)
                .NotEmpty()
                .MaximumLength(50)
                .EmailAddress());
    }
}
