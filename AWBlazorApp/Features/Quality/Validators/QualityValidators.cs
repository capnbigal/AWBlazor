using AWBlazorApp.Features.Quality.Domain;
using AWBlazorApp.Features.Quality.Models;
using FluentValidation;

namespace AWBlazorApp.Features.Quality.Validators;

public sealed class CreateInspectionPlanValidator : AbstractValidator<CreateInspectionPlanRequest>
{
    public CreateInspectionPlanValidator()
    {
        RuleFor(x => x.PlanCode).NotEmpty().MaximumLength(32)
            .Matches("^[A-Z0-9][A-Z0-9_-]*$").WithMessage("PlanCode must be uppercase letters/digits/underscore/hyphen.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.SamplingRule).MaximumLength(200);
        // At least one of (ProductId, WorkOrderRoutingId, VendorBusinessEntityId) must be set so
        // the plan has a target. Per the user: ProductId is the common case; the other two are
        // optional but required at times (e.g., vendor-specific first-piece inspections).
        RuleFor(x => x).Must(x => x.ProductId.HasValue || x.WorkOrderRoutingId.HasValue || x.VendorBusinessEntityId.HasValue)
            .WithMessage("At least one of ProductId, WorkOrderRoutingId, or VendorBusinessEntityId is required.");
    }
}

public sealed class UpdateInspectionPlanValidator : AbstractValidator<UpdateInspectionPlanRequest>
{
    public UpdateInspectionPlanValidator()
    {
        When(x => x.Name is not null, () => RuleFor(x => x.Name!).NotEmpty().MaximumLength(200));
        When(x => x.Description is not null, () => RuleFor(x => x.Description!).MaximumLength(2000));
        When(x => x.SamplingRule is not null, () => RuleFor(x => x.SamplingRule!).MaximumLength(200));
    }
}

public sealed class CreateInspectionPlanCharacteristicValidator : AbstractValidator<CreateInspectionPlanCharacteristicRequest>
{
    public CreateInspectionPlanCharacteristicValidator()
    {
        RuleFor(x => x.InspectionPlanId).GreaterThan(0);
        RuleFor(x => x.SequenceNumber).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.UnitMeasureCode).MaximumLength(3);
        RuleFor(x => x.ExpectedValue).MaximumLength(100);
        // Numeric characteristics need at least one of Min/Max/Target. Attribute characteristics
        // need an ExpectedValue (otherwise any non-empty answer trivially passes).
        When(x => x.Kind == CharacteristicKind.Numeric, () =>
            RuleFor(x => x).Must(c => c.MinValue.HasValue || c.MaxValue.HasValue || c.TargetValue.HasValue)
                .WithMessage("Numeric characteristics must specify at least one of MinValue, MaxValue, or TargetValue."));
        When(x => x.Kind == CharacteristicKind.Attribute, () =>
            RuleFor(x => x.ExpectedValue).NotEmpty().WithMessage("Attribute characteristics need an ExpectedValue."));
    }
}

public sealed class UpdateInspectionPlanCharacteristicValidator : AbstractValidator<UpdateInspectionPlanCharacteristicRequest>
{
    public UpdateInspectionPlanCharacteristicValidator()
    {
        When(x => x.SequenceNumber.HasValue, () => RuleFor(x => x.SequenceNumber!.Value).GreaterThan(0));
        When(x => x.Name is not null, () => RuleFor(x => x.Name!).NotEmpty().MaximumLength(200));
        When(x => x.UnitMeasureCode is not null, () => RuleFor(x => x.UnitMeasureCode!).MaximumLength(3));
    }
}

public sealed class CreateInspectionValidator : AbstractValidator<CreateInspectionRequest>
{
    public CreateInspectionValidator()
    {
        RuleFor(x => x.InspectionPlanId).GreaterThan(0);
        RuleFor(x => x.SourceId).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitMeasureCode).MaximumLength(3);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public sealed class RecordInspectionResultValidator : AbstractValidator<RecordInspectionResultRequest>
{
    public RecordInspectionResultValidator()
    {
        RuleFor(x => x.InspectionPlanCharacteristicId).GreaterThan(0);
        RuleFor(x => x).Must(r => r.NumericResult.HasValue || !string.IsNullOrWhiteSpace(r.AttributeResult))
            .WithMessage("Either NumericResult or AttributeResult must be supplied.");
        RuleFor(x => x.Notes).MaximumLength(500);
        RuleFor(x => x.AttributeResult).MaximumLength(100);
    }
}

public sealed class CreateNonConformanceValidator : AbstractValidator<CreateNonConformanceRequest>
{
    public CreateNonConformanceValidator()
    {
        RuleFor(x => x.InventoryItemId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.UnitMeasureCode).MaximumLength(3);
    }
}

public sealed class UpdateNonConformanceValidator : AbstractValidator<UpdateNonConformanceRequest>
{
    public UpdateNonConformanceValidator()
    {
        When(x => x.Quantity.HasValue, () => RuleFor(x => x.Quantity!.Value).GreaterThan(0));
        When(x => x.Description is not null, () => RuleFor(x => x.Description!).NotEmpty().MaximumLength(2000));
    }
}

public sealed class DispositionNonConformanceValidator : AbstractValidator<DispositionNonConformanceRequest>
{
    public DispositionNonConformanceValidator()
    {
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public sealed class CreateNonConformanceActionValidator : AbstractValidator<CreateNonConformanceActionRequest>
{
    public CreateNonConformanceActionValidator()
    {
        RuleFor(x => x.NonConformanceId).GreaterThan(0);
        RuleFor(x => x.Action).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public sealed class CreateCapaCaseValidator : AbstractValidator<CreateCapaCaseRequest>
{
    public CreateCapaCaseValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}

public sealed class UpdateCapaCaseValidator : AbstractValidator<UpdateCapaCaseRequest>
{
    public UpdateCapaCaseValidator()
    {
        When(x => x.Title is not null, () => RuleFor(x => x.Title!).NotEmpty().MaximumLength(200));
        When(x => x.RootCause is not null, () => RuleFor(x => x.RootCause!).MaximumLength(2000));
        When(x => x.CorrectiveAction is not null, () => RuleFor(x => x.CorrectiveAction!).MaximumLength(2000));
        When(x => x.PreventiveAction is not null, () => RuleFor(x => x.PreventiveAction!).MaximumLength(2000));
        When(x => x.VerificationNotes is not null, () => RuleFor(x => x.VerificationNotes!).MaximumLength(2000));
    }
}
