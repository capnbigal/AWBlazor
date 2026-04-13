using AWBlazorApp.Models.AdventureWorks;
using FluentValidation;

namespace AWBlazorApp.Validators.AdventureWorks;

public sealed class CreateJobCandidateValidator : AbstractValidator<CreateJobCandidateRequest>
{
    public CreateJobCandidateValidator()
    {
        When(x => x.BusinessEntityId.HasValue, () =>
            RuleFor(x => x.BusinessEntityId!.Value).GreaterThan(0));
    }
}

public sealed class UpdateJobCandidateValidator : AbstractValidator<UpdateJobCandidateRequest>
{
    public UpdateJobCandidateValidator()
    {
        When(x => x.BusinessEntityId.HasValue, () =>
            RuleFor(x => x.BusinessEntityId!.Value).GreaterThan(0));
    }
}
