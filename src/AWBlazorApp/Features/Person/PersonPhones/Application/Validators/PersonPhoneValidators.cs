using AWBlazorApp.Features.Person.Addresses.Dtos; using AWBlazorApp.Features.Person.AddressTypes.Dtos; using AWBlazorApp.Features.Person.BusinessEntities.Dtos; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Dtos; using AWBlazorApp.Features.Person.BusinessEntityContacts.Dtos; using AWBlazorApp.Features.Person.ContactTypes.Dtos; using AWBlazorApp.Features.Person.CountryRegions.Dtos; using AWBlazorApp.Features.Person.EmailAddresses.Dtos; using AWBlazorApp.Features.Person.Persons.Dtos; using AWBlazorApp.Features.Person.PersonPhones.Dtos; using AWBlazorApp.Features.Person.PhoneNumberTypes.Dtos; using AWBlazorApp.Features.Person.StateProvinces.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Person.PersonPhones.Application.Validators;

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
