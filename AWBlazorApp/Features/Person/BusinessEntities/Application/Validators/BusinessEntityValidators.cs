using AWBlazorApp.Features.Person.Addresses.Dtos; using AWBlazorApp.Features.Person.AddressTypes.Dtos; using AWBlazorApp.Features.Person.BusinessEntities.Dtos; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Dtos; using AWBlazorApp.Features.Person.BusinessEntityContacts.Dtos; using AWBlazorApp.Features.Person.ContactTypes.Dtos; using AWBlazorApp.Features.Person.CountryRegions.Dtos; using AWBlazorApp.Features.Person.EmailAddresses.Dtos; using AWBlazorApp.Features.Person.Persons.Dtos; using AWBlazorApp.Features.Person.PersonPhones.Dtos; using AWBlazorApp.Features.Person.PhoneNumberTypes.Dtos; using AWBlazorApp.Features.Person.StateProvinces.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Person.BusinessEntities.Application.Validators;

public sealed class CreateBusinessEntityValidator : AbstractValidator<CreateBusinessEntityRequest>
{
    public CreateBusinessEntityValidator()
    {
        // No fields — BusinessEntity has no editable data of its own.
    }
}

public sealed class UpdateBusinessEntityValidator : AbstractValidator<UpdateBusinessEntityRequest>
{
    public UpdateBusinessEntityValidator()
    {
        // No fields — BusinessEntity has no editable data of its own.
    }
}
