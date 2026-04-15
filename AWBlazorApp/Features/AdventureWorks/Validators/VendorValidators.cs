using AWBlazorApp.Features.AdventureWorks.Models;
using FluentValidation;

namespace AWBlazorApp.Features.AdventureWorks.Validators;

public sealed class CreateVendorValidator : AbstractValidator<CreateVendorRequest>
{
    public CreateVendorValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("BusinessEntityId is required (must already exist in Person.BusinessEntity).");
        RuleFor(x => x.AccountNumber).NotEmpty().WithMessage("Account number is required.").MaximumLength(15);
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.").MaximumLength(50);
        RuleFor(x => x.CreditRating).InclusiveBetween((byte)1, (byte)5).WithMessage("Credit rating must be between 1 and 5.");
    }
}

public sealed class UpdateVendorValidator : AbstractValidator<UpdateVendorRequest>
{
    public UpdateVendorValidator()
    {
        When(x => x.AccountNumber is not null, () =>
            RuleFor(x => x.AccountNumber!).NotEmpty().WithMessage("Account number cannot be blanked out.").MaximumLength(15));
        When(x => x.Name is not null, () =>
            RuleFor(x => x.Name!).NotEmpty().WithMessage("Name cannot be blanked out.").MaximumLength(50));
        When(x => x.CreditRating.HasValue, () =>
            RuleFor(x => x.CreditRating!.Value).InclusiveBetween((byte)1, (byte)5).WithMessage("Credit rating must be between 1 and 5."));
    }
}
