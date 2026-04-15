using AWBlazorApp.Features.AdventureWorks.Models;
using FluentValidation;

namespace AWBlazorApp.Features.AdventureWorks.Validators;

public sealed class CreateBusinessEntityContactValidator : AbstractValidator<CreateBusinessEntityContactRequest>
{
    public CreateBusinessEntityContactValidator()
    {
        RuleFor(x => x.BusinessEntityId).GreaterThan(0).WithMessage("BusinessEntityId is required.");
        RuleFor(x => x.PersonId).GreaterThan(0).WithMessage("PersonId is required.");
        RuleFor(x => x.ContactTypeId).GreaterThan(0).WithMessage("ContactTypeId is required.");
    }
}

public sealed class UpdateBusinessEntityContactValidator : AbstractValidator<UpdateBusinessEntityContactRequest>
{
    public UpdateBusinessEntityContactValidator() { }
}
