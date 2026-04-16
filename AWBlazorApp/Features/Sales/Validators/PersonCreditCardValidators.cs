using AWBlazorApp.Features.Sales.Models;
using FluentValidation;

namespace AWBlazorApp.Features.Sales.Validators;

public sealed class CreatePersonCreditCardValidator : AbstractValidator<CreatePersonCreditCardRequest>
{
    public CreatePersonCreditCardValidator()
    {
        RuleFor(x => x.BusinessEntityId).GreaterThan(0).WithMessage("BusinessEntityId is required.");
        RuleFor(x => x.CreditCardId).GreaterThan(0).WithMessage("CreditCardId is required.");
    }
}

public sealed class UpdatePersonCreditCardValidator : AbstractValidator<UpdatePersonCreditCardRequest>
{
    public UpdatePersonCreditCardValidator()
    {
        // No fields to validate — junction table.
    }
}
