using AWBlazorApp.Features.Person.Addresses.Dtos; using AWBlazorApp.Features.Person.AddressTypes.Dtos; using AWBlazorApp.Features.Person.BusinessEntities.Dtos; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Dtos; using AWBlazorApp.Features.Person.BusinessEntityContacts.Dtos; using AWBlazorApp.Features.Person.ContactTypes.Dtos; using AWBlazorApp.Features.Person.CountryRegions.Dtos; using AWBlazorApp.Features.Person.EmailAddresses.Dtos; using AWBlazorApp.Features.Person.Persons.Dtos; using AWBlazorApp.Features.Person.PersonPhones.Dtos; using AWBlazorApp.Features.Person.PhoneNumberTypes.Dtos; using AWBlazorApp.Features.Person.StateProvinces.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Person.BusinessEntityContacts.Application.Validators;

public sealed class CreateBusinessEntityContactValidator : AbstractValidator<CreateBusinessEntityContactRequest>
{
    public CreateBusinessEntityContactValidator()
    {
        RuleFor(x => x.BusinessEntityId).GreaterThan(0).WithMessage("BusinessEntityId is required.");
        RuleFor(x => x.PersonId).GreaterThan(0).WithMessage("PersonId is required.");
        RuleFor(x => x.ContactTypeId).GreaterThan(0).WithMessage("ContactTypeId is required.");
    }
}

public sealed class UpdateBusinessEntityContactValidator : AbstractValidator<UpdateBusinessEntityContactRequest>
{
    public UpdateBusinessEntityContactValidator() { }
}
