using AWBlazorApp.Features.Person.Addresses.Dtos; using AWBlazorApp.Features.Person.AddressTypes.Dtos; using AWBlazorApp.Features.Person.BusinessEntities.Dtos; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Dtos; using AWBlazorApp.Features.Person.BusinessEntityContacts.Dtos; using AWBlazorApp.Features.Person.ContactTypes.Dtos; using AWBlazorApp.Features.Person.CountryRegions.Dtos; using AWBlazorApp.Features.Person.EmailAddresses.Dtos; using AWBlazorApp.Features.Person.Persons.Dtos; using AWBlazorApp.Features.Person.PersonPhones.Dtos; using AWBlazorApp.Features.Person.PhoneNumberTypes.Dtos; using AWBlazorApp.Features.Person.StateProvinces.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Person.BusinessEntityAddresses.Application.Validators;

public sealed class CreateBusinessEntityAddressValidator : AbstractValidator<CreateBusinessEntityAddressRequest>
{
    public CreateBusinessEntityAddressValidator()
    {
        RuleFor(x => x.BusinessEntityId).GreaterThan(0).WithMessage("BusinessEntityId is required.");
        RuleFor(x => x.AddressId).GreaterThan(0).WithMessage("AddressId is required.");
        RuleFor(x => x.AddressTypeId).GreaterThan(0).WithMessage("AddressTypeId is required.");
    }
}

public sealed class UpdateBusinessEntityAddressValidator : AbstractValidator<UpdateBusinessEntityAddressRequest>
{
    public UpdateBusinessEntityAddressValidator() { }
}
