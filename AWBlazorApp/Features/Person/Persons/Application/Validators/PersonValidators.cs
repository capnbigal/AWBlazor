using AWBlazorApp.Features.Person.Addresses.Dtos; using AWBlazorApp.Features.Person.AddressTypes.Dtos; using AWBlazorApp.Features.Person.BusinessEntities.Dtos; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Dtos; using AWBlazorApp.Features.Person.BusinessEntityContacts.Dtos; using AWBlazorApp.Features.Person.ContactTypes.Dtos; using AWBlazorApp.Features.Person.CountryRegions.Dtos; using AWBlazorApp.Features.Person.EmailAddresses.Dtos; using AWBlazorApp.Features.Person.Persons.Dtos; using AWBlazorApp.Features.Person.PersonPhones.Dtos; using AWBlazorApp.Features.Person.PhoneNumberTypes.Dtos; using AWBlazorApp.Features.Person.StateProvinces.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Person.Persons.Application.Validators;

public sealed class CreatePersonValidator : AbstractValidator<CreatePersonRequest>
{
    /// <summary>
    /// SQL CHECK constraint on <c>Person.Person.PersonType</c> — only these 6 codes are valid.
    /// SC=Store Contact, IN=Individual customer, SP=Sales Person, EM=Employee,
    /// VC=Vendor Contact, GC=General Contact.
    /// </summary>
    private static readonly string[] AllowedPersonTypes = ["SC", "IN", "SP", "EM", "VC", "GC"];

    public CreatePersonValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("BusinessEntityId is required (must already exist in Person.BusinessEntity).");
        RuleFor(x => x.PersonType)
            .NotEmpty()
            .Must(t => t is not null && AllowedPersonTypes.Contains(t))
            .WithMessage("PersonType must be one of SC, IN, SP, EM, VC, GC.");
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
        When(x => x.MiddleName is not null, () => RuleFor(x => x.MiddleName!).MaximumLength(50));
        When(x => x.Title is not null, () => RuleFor(x => x.Title!).MaximumLength(8));
        When(x => x.Suffix is not null, () => RuleFor(x => x.Suffix!).MaximumLength(10));
        RuleFor(x => x.EmailPromotion).InclusiveBetween(0, 2)
            .WithMessage("EmailPromotion must be 0, 1, or 2.");
    }
}

public sealed class UpdatePersonValidator : AbstractValidator<UpdatePersonRequest>
{
    private static readonly string[] AllowedPersonTypes = ["SC", "IN", "SP", "EM", "VC", "GC"];

    public UpdatePersonValidator()
    {
        When(x => x.PersonType is not null, () =>
            RuleFor(x => x.PersonType!)
                .Must(t => AllowedPersonTypes.Contains(t))
                .WithMessage("PersonType must be one of SC, IN, SP, EM, VC, GC."));
        When(x => x.FirstName is not null, () => RuleFor(x => x.FirstName!).NotEmpty().MaximumLength(50));
        When(x => x.LastName is not null, () => RuleFor(x => x.LastName!).NotEmpty().MaximumLength(50));
        When(x => x.MiddleName is not null, () => RuleFor(x => x.MiddleName!).MaximumLength(50));
        When(x => x.Title is not null, () => RuleFor(x => x.Title!).MaximumLength(8));
        When(x => x.Suffix is not null, () => RuleFor(x => x.Suffix!).MaximumLength(10));
        When(x => x.EmailPromotion.HasValue, () =>
            RuleFor(x => x.EmailPromotion!.Value).InclusiveBetween(0, 2));
    }
}
