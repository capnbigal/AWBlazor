using AWBlazorApp.Features.Enterprise.Dtos;
using FluentValidation;

namespace AWBlazorApp.Features.Enterprise.Validators;

public sealed class CreateOrganizationValidator : AbstractValidator<CreateOrganizationRequest>
{
    public CreateOrganizationValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(32)
            .Matches("^[A-Z0-9][A-Z0-9_-]*$").WithMessage("Code must be uppercase letters/digits/underscore/hyphen.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ExternalRef).MaximumLength(128);
    }
}

public sealed class UpdateOrganizationValidator : AbstractValidator<UpdateOrganizationRequest>
{
    public UpdateOrganizationValidator()
    {
        When(x => x.Code is not null, () =>
            RuleFor(x => x.Code!).NotEmpty().MaximumLength(32)
                .Matches("^[A-Z0-9][A-Z0-9_-]*$").WithMessage("Code must be uppercase letters/digits/underscore/hyphen."));
        When(x => x.Name is not null, () =>
            RuleFor(x => x.Name!).NotEmpty().MaximumLength(200));
        When(x => x.ExternalRef is not null, () =>
            RuleFor(x => x.ExternalRef!).MaximumLength(128));
    }
}
