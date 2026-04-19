using AWBlazorApp.Features.Production.Dtos;
using FluentValidation;

namespace AWBlazorApp.Features.Production.Validators;

public sealed class CreateIllustrationValidator : AbstractValidator<CreateIllustrationRequest>
{
    public CreateIllustrationValidator() { }
}

public sealed class UpdateIllustrationValidator : AbstractValidator<UpdateIllustrationRequest>
{
    public UpdateIllustrationValidator() { }
}
