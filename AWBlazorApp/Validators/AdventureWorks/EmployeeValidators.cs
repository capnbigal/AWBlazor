using AWBlazorApp.Models.AdventureWorks;
using FluentValidation;

namespace AWBlazorApp.Validators.AdventureWorks;

public sealed class CreateEmployeeValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("BusinessEntityId is required (must already exist in Person.BusinessEntity).");
        RuleFor(x => x.NationalIDNumber).NotEmpty().WithMessage("NationalIDNumber is required.").MaximumLength(15);
        RuleFor(x => x.LoginID).NotEmpty().WithMessage("LoginID is required.").MaximumLength(256);
        RuleFor(x => x.JobTitle).NotEmpty().WithMessage("Job title is required.").MaximumLength(50);
        RuleFor(x => x.BirthDate).NotEmpty();
        RuleFor(x => x.MaritalStatus).NotEmpty().Must(v => v is "S" or "M" or "s" or "m")
            .WithMessage("MaritalStatus must be S (Single) or M (Married).");
        RuleFor(x => x.Gender).NotEmpty().Must(v => v is "F" or "M" or "f" or "m")
            .WithMessage("Gender must be F (Female) or M (Male).");
        RuleFor(x => x.HireDate).NotEmpty();
    }
}

public sealed class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeRequest>
{
    public UpdateEmployeeValidator()
    {
        When(x => x.NationalIDNumber is not null, () =>
            RuleFor(x => x.NationalIDNumber!).NotEmpty().MaximumLength(15));
        When(x => x.LoginID is not null, () =>
            RuleFor(x => x.LoginID!).NotEmpty().MaximumLength(256));
        When(x => x.JobTitle is not null, () =>
            RuleFor(x => x.JobTitle!).NotEmpty().MaximumLength(50));
        When(x => x.MaritalStatus is not null, () =>
            RuleFor(x => x.MaritalStatus!).Must(v => v is "S" or "M" or "s" or "m")
                .WithMessage("MaritalStatus must be S or M."));
        When(x => x.Gender is not null, () =>
            RuleFor(x => x.Gender!).Must(v => v is "F" or "M" or "f" or "m")
                .WithMessage("Gender must be F or M."));
    }
}
