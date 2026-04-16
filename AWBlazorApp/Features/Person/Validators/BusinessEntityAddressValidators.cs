using AWBlazorApp.Features.Person.Models;
using FluentValidation;

namespace AWBlazorApp.Features.Person.Validators;

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
