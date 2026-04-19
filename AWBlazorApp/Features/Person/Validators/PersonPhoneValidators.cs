using AWBlazorApp.Features.Person.Dtos;
using FluentValidation;

namespace AWBlazorApp.Features.Person.Validators;

public sealed class CreatePersonPhoneValidator : AbstractValidator<CreatePersonPhoneRequest>
{
    public CreatePersonPhoneValidator()
    {
        RuleFor(x => x.BusinessEntityId).GreaterThan(0).WithMessage("BusinessEntityId is required.");
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(25);
        RuleFor(x => x.PhoneNumberTypeId).GreaterThan(0).WithMessage("PhoneNumberTypeId is required.");
    }
}

public sealed class UpdatePersonPhoneValidator : AbstractValidator<UpdatePersonPhoneRequest>
{
    public UpdatePersonPhoneValidator()
    {
        // No fields to validate — touching the row just updates ModifiedDate.
    }
}
