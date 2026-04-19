using AWBlazorApp.Features.Person.Addresses.Dtos; using AWBlazorApp.Features.Person.AddressTypes.Dtos; using AWBlazorApp.Features.Person.BusinessEntities.Dtos; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Dtos; using AWBlazorApp.Features.Person.BusinessEntityContacts.Dtos; using AWBlazorApp.Features.Person.ContactTypes.Dtos; using AWBlazorApp.Features.Person.CountryRegions.Dtos; using AWBlazorApp.Features.Person.EmailAddresses.Dtos; using AWBlazorApp.Features.Person.Persons.Dtos; using AWBlazorApp.Features.Person.PersonPhones.Dtos; using AWBlazorApp.Features.Person.PhoneNumberTypes.Dtos; using AWBlazorApp.Features.Person.StateProvinces.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Person.Addresses.Application.Validators;

public sealed class CreateAddressValidator : AbstractValidator<CreateAddressRequest>
{
    public CreateAddressValidator()
    {
        RuleFor(x => x.AddressLine1).NotEmpty().MaximumLength(60);
        When(x => x.AddressLine2 is not null, () =>
            RuleFor(x => x.AddressLine2!).MaximumLength(60));
        RuleFor(x => x.City).NotEmpty().MaximumLength(30);
        RuleFor(x => x.StateProvinceId).GreaterThan(0).WithMessage("StateProvinceId is required.");
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(15);
    }
}

public sealed class UpdateAddressValidator : AbstractValidator<UpdateAddressRequest>
{
    public UpdateAddressValidator()
    {
        When(x => x.AddressLine1 is not null, () => RuleFor(x => x.AddressLine1!).NotEmpty().MaximumLength(60));
        When(x => x.AddressLine2 is not null, () => RuleFor(x => x.AddressLine2!).MaximumLength(60));
        When(x => x.City is not null, () => RuleFor(x => x.City!).NotEmpty().MaximumLength(30));
        When(x => x.StateProvinceId.HasValue, () => RuleFor(x => x.StateProvinceId!.Value).GreaterThan(0));
        When(x => x.PostalCode is not null, () => RuleFor(x => x.PostalCode!).NotEmpty().MaximumLength(15));
    }
}
