using AWBlazorApp.Features.Workforce.Dtos;
using FluentValidation;

namespace AWBlazorApp.Features.Workforce.Validators;

public sealed class CreateTrainingCourseValidator : AbstractValidator<CreateTrainingCourseRequest>
{
    public CreateTrainingCourseValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(32).Matches("^[A-Z0-9][A-Z0-9_-]*$");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        When(x => x.DurationMinutes.HasValue, () => RuleFor(x => x.DurationMinutes!.Value).GreaterThan(0));
        When(x => x.RecurrenceMonths.HasValue, () => RuleFor(x => x.RecurrenceMonths!.Value).GreaterThan(0));
    }
}

public sealed class UpdateTrainingCourseValidator : AbstractValidator<UpdateTrainingCourseRequest>
{
    public UpdateTrainingCourseValidator()
    {
        When(x => x.Name is not null, () => RuleFor(x => x.Name!).NotEmpty().MaximumLength(200));
        When(x => x.Description is not null, () => RuleFor(x => x.Description!).MaximumLength(2000));
    }
}

public sealed class CreateTrainingRecordValidator : AbstractValidator<CreateTrainingRecordRequest>
{
    public CreateTrainingRecordValidator()
    {
        RuleFor(x => x.TrainingCourseId).GreaterThan(0);
        RuleFor(x => x.BusinessEntityId).GreaterThan(0);
        RuleFor(x => x.Score).MaximumLength(200);
        RuleFor(x => x.EvidenceUrl).MaximumLength(500);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public sealed class CreateQualificationValidator : AbstractValidator<CreateQualificationRequest>
{
    public CreateQualificationValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(32).Matches("^[A-Z0-9][A-Z0-9_-]*$");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}

public sealed class UpdateQualificationValidator : AbstractValidator<UpdateQualificationRequest>
{
    public UpdateQualificationValidator()
    {
        When(x => x.Name is not null, () => RuleFor(x => x.Name!).NotEmpty().MaximumLength(200));
        When(x => x.Description is not null, () => RuleFor(x => x.Description!).MaximumLength(2000));
    }
}

public sealed class GrantEmployeeQualificationValidator : AbstractValidator<GrantEmployeeQualificationRequest>
{
    public GrantEmployeeQualificationValidator()
    {
        RuleFor(x => x.BusinessEntityId).GreaterThan(0);
        RuleFor(x => x.QualificationId).GreaterThan(0);
    }
}

public sealed class CreateStationQualificationValidator : AbstractValidator<CreateStationQualificationRequest>
{
    public CreateStationQualificationValidator()
    {
        RuleFor(x => x.StationId).GreaterThan(0);
        RuleFor(x => x.QualificationId).GreaterThan(0);
    }
}

public sealed class CreateAttendanceEventValidator : AbstractValidator<CreateAttendanceEventRequest>
{
    public CreateAttendanceEventValidator()
    {
        RuleFor(x => x.BusinessEntityId).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public sealed class CreateLeaveRequestValidator : AbstractValidator<CreateLeaveRequestRequest>
{
    public CreateLeaveRequestValidator()
    {
        RuleFor(x => x.BusinessEntityId).GreaterThan(0);
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be on or after start date.");
        RuleFor(x => x.Reason).MaximumLength(2000);
    }
}

public sealed class CreateShiftHandoverNoteValidator : AbstractValidator<CreateShiftHandoverNoteRequest>
{
    public CreateShiftHandoverNoteValidator()
    {
        RuleFor(x => x.StationId).GreaterThan(0);
        RuleFor(x => x.Note).NotEmpty().MaximumLength(2000);
    }
}

public sealed class CreateAnnouncementValidator : AbstractValidator<CreateAnnouncementRequest>
{
    public CreateAnnouncementValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Body).NotEmpty();
    }
}

public sealed class UpdateAnnouncementValidator : AbstractValidator<UpdateAnnouncementRequest>
{
    public UpdateAnnouncementValidator()
    {
        When(x => x.Title is not null, () => RuleFor(x => x.Title!).NotEmpty().MaximumLength(200));
    }
}
