using Cronos;
using AWBlazorApp.Models;
using FluentValidation;

namespace AWBlazorApp.Validators;

public sealed class CreateProcessValidator : AbstractValidator<CreateProcessRequest>
{
    public CreateProcessValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.DepartmentId).GreaterThan((short)0);
        RuleFor(x => x.CronSchedule)
            .NotEmpty()
            .When(x => x.IsRecurring)
            .WithMessage("CronSchedule is required for recurring processes.");
        RuleFor(x => x.CronSchedule)
            .Must(cron =>
            {
                try { CronExpression.Parse(cron!); return true; }
                catch { return false; }
            })
            .When(x => !string.IsNullOrWhiteSpace(x.CronSchedule))
            .WithMessage("CronSchedule is not a valid cron expression.");
    }
}

public sealed class UpdateProcessValidator : AbstractValidator<UpdateProcessRequest>
{
    public UpdateProcessValidator()
    {
        RuleFor(x => x.Name).MaximumLength(200).When(x => x.Name is not null);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
        RuleFor(x => x.DepartmentId).GreaterThan((short)0).When(x => x.DepartmentId.HasValue);
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
        RuleFor(x => x.CronSchedule)
            .Must(cron =>
            {
                try { CronExpression.Parse(cron!); return true; }
                catch { return false; }
            })
            .When(x => !string.IsNullOrWhiteSpace(x.CronSchedule))
            .WithMessage("CronSchedule is not a valid cron expression.");
    }
}

public sealed class CreateProcessStepValidator : AbstractValidator<CreateProcessStepRequest>
{
    public CreateProcessStepValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.SequenceNumber).GreaterThan(0);
    }
}

public sealed class UpdateProcessStepValidator : AbstractValidator<UpdateProcessStepRequest>
{
    public UpdateProcessStepValidator()
    {
        RuleFor(x => x.Name).MaximumLength(200).When(x => x.Name is not null);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
        RuleFor(x => x.SequenceNumber).GreaterThan(0).When(x => x.SequenceNumber.HasValue);
    }
}
