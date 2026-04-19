using AWBlazorApp.Features.Enterprise.Dtos;
using FluentValidation;

namespace AWBlazorApp.Features.Enterprise.Validators;

public sealed class CreateProductLineValidator : AbstractValidator<CreateProductLineRequest>
{
    public CreateProductLineValidator()
    {
        RuleFor(x => x.OrganizationId).GreaterThan(0);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}

public sealed class UpdateProductLineValidator : AbstractValidator<UpdateProductLineRequest>
{
    public UpdateProductLineValidator()
    {
        When(x => x.Code is not null, () => RuleFor(x => x.Code!).NotEmpty().MaximumLength(32));
        When(x => x.Name is not null, () => RuleFor(x => x.Name!).NotEmpty().MaximumLength(200));
        When(x => x.Description is not null, () => RuleFor(x => x.Description!).MaximumLength(2000));
    }
}
