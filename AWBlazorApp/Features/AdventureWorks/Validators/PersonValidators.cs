using AWBlazorApp.Features.AdventureWorks.Models;
using FluentValidation;

namespace AWBlazorApp.Features.AdventureWorks.Validators;

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
