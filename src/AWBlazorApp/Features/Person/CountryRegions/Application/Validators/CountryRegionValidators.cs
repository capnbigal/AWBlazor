using AWBlazorApp.Features.Person.Addresses.Dtos; using AWBlazorApp.Features.Person.AddressTypes.Dtos; using AWBlazorApp.Features.Person.BusinessEntities.Dtos; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Dtos; using AWBlazorApp.Features.Person.BusinessEntityContacts.Dtos; using AWBlazorApp.Features.Person.ContactTypes.Dtos; using AWBlazorApp.Features.Person.CountryRegions.Dtos; using AWBlazorApp.Features.Person.EmailAddresses.Dtos; using AWBlazorApp.Features.Person.Persons.Dtos; using AWBlazorApp.Features.Person.PersonPhones.Dtos; using AWBlazorApp.Features.Person.PhoneNumberTypes.Dtos; using AWBlazorApp.Features.Person.StateProvinces.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Person.CountryRegions.Application.Validators;

public sealed class CreateCountryRegionValidator : AbstractValidator<CreateCountryRegionRequest>
{
    public CreateCountryRegionValidator()
    {
        RuleFor(x => x.CountryRegionCode)
            .NotEmpty().WithMessage("Country/region code is required.")
            .MaximumLength(3).WithMessage("Country/region code must be at most 3 characters.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.").MaximumLength(50);
    }
}

public sealed class UpdateCountryRegionValidator : AbstractValidator<UpdateCountryRegionRequest>
{
    public UpdateCountryRegionValidator()
    {
        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name!).NotEmpty().WithMessage("Name cannot be blanked out.").MaximumLength(50);
        });
    }
}
