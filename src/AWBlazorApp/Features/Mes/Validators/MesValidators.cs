using AWBlazorApp.Features.Mes.Runs.Dtos;
using AWBlazorApp.Features.Mes.Instructions.Dtos;
using AWBlazorApp.Features.Mes.Downtime.Domain; using AWBlazorApp.Features.Mes.Instructions.Domain; using AWBlazorApp.Features.Mes.Runs.Domain; 
using AWBlazorApp.Features.Mes.Dtos;
using FluentValidation;

namespace AWBlazorApp.Features.Mes.Validators;

public sealed class CreateProductionRunValidator : AbstractValidator<CreateProductionRunRequest>
{
    public CreateProductionRunValidator()
    {
        RuleFor(x => x.QuantityPlanned).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Notes).MaximumLength(500);
        // Production-kind runs require a WorkOrder; ad-hoc kinds (Engineering, Replacement,
        // Service, Other) may run without one.
        When(x => x.Kind == ProductionRunKind.Production, () =>
            RuleFor(x => x.WorkOrderId).NotNull().GreaterThan(0)
                .WithMessage("Production-kind runs require a WorkOrderId."));
    }
}

public sealed class UpdateProductionRunValidator : AbstractValidator<UpdateProductionRunRequest>
{
    public UpdateProductionRunValidator()
    {
        When(x => x.QuantityPlanned.HasValue, () => RuleFor(x => x.QuantityPlanned!.Value).GreaterThanOrEqualTo(0));
        When(x => x.Notes is not null, () => RuleFor(x => x.Notes!).MaximumLength(500));
    }
}

public sealed class CompleteProductionRunValidator : AbstractValidator<CompleteProductionRunRequest>
{
    public CompleteProductionRunValidator()
    {
        RuleFor(x => x.QuantityProduced).GreaterThanOrEqualTo(0);
        RuleFor(x => x.QuantityScrapped).GreaterThanOrEqualTo(0);
        // If any material-issue field is set, require the four required ones.
        When(x => x.MaterialIssueQuantity.HasValue || x.MaterialIssueInventoryItemId.HasValue, () =>
        {
            RuleFor(x => x.MaterialIssueInventoryItemId).NotNull().GreaterThan(0);
            RuleFor(x => x.MaterialIssueQuantity).NotNull().GreaterThan(0);
            RuleFor(x => x.MaterialIssueUnitMeasureCode).NotEmpty().MaximumLength(3);
            RuleFor(x => x.MaterialIssueFromLocationId).NotNull().GreaterThan(0);
        });
    }
}

public sealed class CreateProductionRunOperationValidator : AbstractValidator<CreateProductionRunOperationRequest>
{
    public CreateProductionRunOperationValidator()
    {
        RuleFor(x => x.ProductionRunId).GreaterThan(0);
        RuleFor(x => x.SequenceNumber).GreaterThan(0);
        RuleFor(x => x.OperationDescription).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ActualHours).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateProductionRunOperationValidator : AbstractValidator<UpdateProductionRunOperationRequest>
{
    public UpdateProductionRunOperationValidator()
    {
        When(x => x.ActualHours.HasValue, () => RuleFor(x => x.ActualHours!.Value).GreaterThanOrEqualTo(0));
        When(x => x.OperationDescription is not null, () => RuleFor(x => x.OperationDescription!).MaximumLength(200));
    }
}

public sealed class CreateOperatorClockEventValidator : AbstractValidator<CreateOperatorClockEventRequest>
{
    public CreateOperatorClockEventValidator()
    {
        RuleFor(x => x.StationId).GreaterThan(0);
        RuleFor(x => x.BusinessEntityId).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public sealed class CreateDowntimeEventValidator : AbstractValidator<CreateDowntimeEventRequest>
{
    public CreateDowntimeEventValidator()
    {
        RuleFor(x => x.StationId).GreaterThan(0);
        RuleFor(x => x.DowntimeReasonId).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public sealed class CreateDowntimeReasonValidator : AbstractValidator<CreateDowntimeReasonRequest>
{
    public CreateDowntimeReasonValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(32)
            .Matches("^[A-Z0-9][A-Z0-9_]*$").WithMessage("Code must be uppercase alphanumeric / underscore.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class UpdateDowntimeReasonValidator : AbstractValidator<UpdateDowntimeReasonRequest>
{
    public UpdateDowntimeReasonValidator()
    {
        When(x => x.Name is not null, () => RuleFor(x => x.Name!).NotEmpty().MaximumLength(100));
        When(x => x.Description is not null, () => RuleFor(x => x.Description!).MaximumLength(500));
    }
}

public sealed class CreateWorkInstructionValidator : AbstractValidator<CreateWorkInstructionRequest>
{
    public CreateWorkInstructionValidator()
    {
        RuleFor(x => x.WorkOrderRoutingId).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}

public sealed class UpdateWorkInstructionValidator : AbstractValidator<UpdateWorkInstructionRequest>
{
    public UpdateWorkInstructionValidator()
    {
        When(x => x.Title is not null, () => RuleFor(x => x.Title!).NotEmpty().MaximumLength(200));
    }
}

public sealed class CreateWorkInstructionStepValidator : AbstractValidator<CreateWorkInstructionStepRequest>
{
    public CreateWorkInstructionStepValidator()
    {
        RuleFor(x => x.WorkInstructionRevisionId).GreaterThan(0);
        RuleFor(x => x.SequenceNumber).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AttachmentUrl).MaximumLength(500);
    }
}

public sealed class UpdateWorkInstructionStepValidator : AbstractValidator<UpdateWorkInstructionStepRequest>
{
    public UpdateWorkInstructionStepValidator()
    {
        When(x => x.SequenceNumber.HasValue, () => RuleFor(x => x.SequenceNumber!.Value).GreaterThan(0));
        When(x => x.Title is not null, () => RuleFor(x => x.Title!).NotEmpty().MaximumLength(200));
        When(x => x.AttachmentUrl is not null, () => RuleFor(x => x.AttachmentUrl!).MaximumLength(500));
    }
}
