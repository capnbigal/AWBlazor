using AWBlazorApp.Models.AdventureWorks;
using FluentValidation;

namespace AWBlazorApp.Validators.AdventureWorks;

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
