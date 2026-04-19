using AWBlazorApp.Features.Person.Addresses.Dtos; using AWBlazorApp.Features.Person.AddressTypes.Dtos; using AWBlazorApp.Features.Person.BusinessEntities.Dtos; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Dtos; using AWBlazorApp.Features.Person.BusinessEntityContacts.Dtos; using AWBlazorApp.Features.Person.ContactTypes.Dtos; using AWBlazorApp.Features.Person.CountryRegions.Dtos; using AWBlazorApp.Features.Person.EmailAddresses.Dtos; using AWBlazorApp.Features.Person.Persons.Dtos; using AWBlazorApp.Features.Person.PersonPhones.Dtos; using AWBlazorApp.Features.Person.PhoneNumberTypes.Dtos; using AWBlazorApp.Features.Person.StateProvinces.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Person.EmailAddresses.Application.Validators;

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
