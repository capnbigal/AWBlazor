using AWBlazorApp.Features.Person.Addresses.Dtos; using AWBlazorApp.Features.Person.AddressTypes.Dtos; using AWBlazorApp.Features.Person.BusinessEntities.Dtos; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Dtos; using AWBlazorApp.Features.Person.BusinessEntityContacts.Dtos; using AWBlazorApp.Features.Person.ContactTypes.Dtos; using AWBlazorApp.Features.Person.CountryRegions.Dtos; using AWBlazorApp.Features.Person.EmailAddresses.Dtos; using AWBlazorApp.Features.Person.Persons.Dtos; using AWBlazorApp.Features.Person.PersonPhones.Dtos; using AWBlazorApp.Features.Person.PhoneNumberTypes.Dtos; using AWBlazorApp.Features.Person.StateProvinces.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Person.StateProvinces.Application.Validators;

public sealed class CreateStateProvinceValidator : AbstractValidator<CreateStateProvinceRequest>
{
    public CreateStateProvinceValidator()
    {
        RuleFor(x => x.StateProvinceCode).NotEmpty().MaximumLength(3);
        RuleFor(x => x.CountryRegionCode).NotEmpty().MaximumLength(3);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.TerritoryId).GreaterThan(0).WithMessage("Territory id is required.");
    }
}

public sealed class UpdateStateProvinceValidator : AbstractValidator<UpdateStateProvinceRequest>
{
    public UpdateStateProvinceValidator()
    {
        When(x => x.StateProvinceCode is not null, () =>
            RuleFor(x => x.StateProvinceCode!).NotEmpty().MaximumLength(3));
        When(x => x.CountryRegionCode is not null, () =>
            RuleFor(x => x.CountryRegionCode!).NotEmpty().MaximumLength(3));
        When(x => x.Name is not null, () =>
            RuleFor(x => x.Name!).NotEmpty().MaximumLength(50));
        When(x => x.TerritoryId.HasValue, () =>
            RuleFor(x => x.TerritoryId!.Value).GreaterThan(0));
    }
}
