using AWBlazorApp.Features.Maintenance.Dtos;
using FluentValidation;

namespace AWBlazorApp.Features.Maintenance.Validators;

public sealed class CreateAssetMaintenanceProfileValidator : AbstractValidator<CreateAssetMaintenanceProfileRequest>
{
    public CreateAssetMaintenanceProfileValidator()
    {
        RuleFor(x => x.AssetId).GreaterThan(0);
        When(x => x.TargetMtbfHours.HasValue, () => RuleFor(x => x.TargetMtbfHours!.Value).GreaterThan(0));
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public sealed class UpdateAssetMaintenanceProfileValidator : AbstractValidator<UpdateAssetMaintenanceProfileRequest>
{
    public UpdateAssetMaintenanceProfileValidator()
    {
        When(x => x.TargetMtbfHours.HasValue, () => RuleFor(x => x.TargetMtbfHours!.Value).GreaterThan(0));
        When(x => x.Notes is not null, () => RuleFor(x => x.Notes!).MaximumLength(2000));
    }
}

public sealed class CreatePmScheduleValidator : AbstractValidator<CreatePmScheduleRequest>
{
    public CreatePmScheduleValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(32).Matches("^[A-Z0-9][A-Z0-9_-]*$");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.AssetId).GreaterThan(0);
        RuleFor(x => x.IntervalValue).GreaterThan(0);
        RuleFor(x => x.EstimatedMinutes).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdatePmScheduleValidator : AbstractValidator<UpdatePmScheduleRequest>
{
    public UpdatePmScheduleValidator()
    {
        When(x => x.Name is not null, () => RuleFor(x => x.Name!).NotEmpty().MaximumLength(200));
        When(x => x.Description is not null, () => RuleFor(x => x.Description!).MaximumLength(2000));
        When(x => x.IntervalValue.HasValue, () => RuleFor(x => x.IntervalValue!.Value).GreaterThan(0));
        When(x => x.EstimatedMinutes.HasValue, () => RuleFor(x => x.EstimatedMinutes!.Value).GreaterThanOrEqualTo(0));
    }
}

public sealed class CreatePmScheduleTaskValidator : AbstractValidator<CreatePmScheduleTaskRequest>
{
    public CreatePmScheduleTaskValidator()
    {
        RuleFor(x => x.PmScheduleId).GreaterThan(0);
        RuleFor(x => x.SequenceNumber).GreaterThan(0);
        RuleFor(x => x.TaskName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Instructions).MaximumLength(2000);
        When(x => x.EstimatedMinutes.HasValue, () => RuleFor(x => x.EstimatedMinutes!.Value).GreaterThan(0));
    }
}

public sealed class CreateMaintenanceWorkOrderValidator : AbstractValidator<CreateMaintenanceWorkOrderRequest>
{
    public CreateMaintenanceWorkOrderValidator()
    {
        RuleFor(x => x.WorkOrderNumber).NotEmpty().MaximumLength(32).Matches("^[A-Z0-9][A-Z0-9_-]*$");
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.AssetId).GreaterThan(0);
    }
}

public sealed class UpdateMaintenanceWorkOrderValidator : AbstractValidator<UpdateMaintenanceWorkOrderRequest>
{
    public UpdateMaintenanceWorkOrderValidator()
    {
        When(x => x.Title is not null, () => RuleFor(x => x.Title!).NotEmpty().MaximumLength(200));
        When(x => x.Description is not null, () => RuleFor(x => x.Description!).MaximumLength(4000));
    }
}

public sealed class CreateWorkOrderTaskValidator : AbstractValidator<CreateWorkOrderTaskRequest>
{
    public CreateWorkOrderTaskValidator()
    {
        RuleFor(x => x.MaintenanceWorkOrderId).GreaterThan(0);
        RuleFor(x => x.SequenceNumber).GreaterThan(0);
        RuleFor(x => x.TaskName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Instructions).MaximumLength(2000);
    }
}

public sealed class CreateSparePartValidator : AbstractValidator<CreateSparePartRequest>
{
    public CreateSparePartValidator()
    {
        RuleFor(x => x.PartNumber).NotEmpty().MaximumLength(32).Matches("^[A-Z0-9][A-Z0-9_-]*$");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.UnitMeasureCode).NotEmpty().MaximumLength(3);
        When(x => x.StandardCost.HasValue, () => RuleFor(x => x.StandardCost!.Value).GreaterThanOrEqualTo(0));
    }
}

public sealed class UpdateSparePartValidator : AbstractValidator<UpdateSparePartRequest>
{
    public UpdateSparePartValidator()
    {
        When(x => x.Name is not null, () => RuleFor(x => x.Name!).NotEmpty().MaximumLength(200));
        When(x => x.UnitMeasureCode is not null, () => RuleFor(x => x.UnitMeasureCode!).MaximumLength(3));
        When(x => x.StandardCost.HasValue, () => RuleFor(x => x.StandardCost!.Value).GreaterThanOrEqualTo(0));
    }
}

public sealed class CreateWorkOrderPartUsageValidator : AbstractValidator<CreateWorkOrderPartUsageRequest>
{
    public CreateWorkOrderPartUsageValidator()
    {
        RuleFor(x => x.MaintenanceWorkOrderId).GreaterThan(0);
        RuleFor(x => x.SparePartId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        When(x => x.UnitCost.HasValue, () => RuleFor(x => x.UnitCost!.Value).GreaterThanOrEqualTo(0));
    }
}

public sealed class CreateMeterReadingValidator : AbstractValidator<CreateMeterReadingRequest>
{
    public CreateMeterReadingValidator()
    {
        RuleFor(x => x.AssetId).GreaterThan(0);
        RuleFor(x => x.Value).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public sealed class CreateMaintenanceLogValidator : AbstractValidator<CreateMaintenanceLogRequest>
{
    public CreateMaintenanceLogValidator()
    {
        RuleFor(x => x.AssetId).GreaterThan(0);
        RuleFor(x => x.Note).NotEmpty().MaximumLength(2000);
    }
}
