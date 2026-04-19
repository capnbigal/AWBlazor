using AWBlazorApp.Features.Engineering.Dtos;
using AWBlazorApp.Features.Engineering.Boms.Dtos; using AWBlazorApp.Features.Engineering.Ecos.Dtos; using AWBlazorApp.Features.Engineering.Routings.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Engineering.Validators;

public sealed class CreateManufacturingRoutingValidator : AbstractValidator<CreateManufacturingRoutingRequest>
{
    public CreateManufacturingRoutingValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(32).Matches("^[A-Z0-9][A-Z0-9_-]*$");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.RevisionNumber).GreaterThan(0);
    }
}

public sealed class UpdateManufacturingRoutingValidator : AbstractValidator<UpdateManufacturingRoutingRequest>
{
    public UpdateManufacturingRoutingValidator()
    {
        When(x => x.Name is not null, () => RuleFor(x => x.Name!).NotEmpty().MaximumLength(200));
        When(x => x.Description is not null, () => RuleFor(x => x.Description!).MaximumLength(2000));
        When(x => x.RevisionNumber.HasValue, () => RuleFor(x => x.RevisionNumber!.Value).GreaterThan(0));
    }
}

public sealed class CreateRoutingStepValidator : AbstractValidator<CreateRoutingStepRequest>
{
    public CreateRoutingStepValidator()
    {
        RuleFor(x => x.ManufacturingRoutingId).GreaterThan(0);
        RuleFor(x => x.SequenceNumber).GreaterThan(0);
        RuleFor(x => x.OperationName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StandardMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Instructions).MaximumLength(2000);
    }
}

public sealed class CreateBomHeaderValidator : AbstractValidator<CreateBomHeaderRequest>
{
    public CreateBomHeaderValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(32).Matches("^[A-Z0-9][A-Z0-9_-]*$");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.RevisionNumber).GreaterThan(0);
    }
}

public sealed class UpdateBomHeaderValidator : AbstractValidator<UpdateBomHeaderRequest>
{
    public UpdateBomHeaderValidator()
    {
        When(x => x.Name is not null, () => RuleFor(x => x.Name!).NotEmpty().MaximumLength(200));
        When(x => x.Description is not null, () => RuleFor(x => x.Description!).MaximumLength(2000));
        When(x => x.RevisionNumber.HasValue, () => RuleFor(x => x.RevisionNumber!.Value).GreaterThan(0));
    }
}

public sealed class CreateBomLineValidator : AbstractValidator<CreateBomLineRequest>
{
    public CreateBomLineValidator()
    {
        RuleFor(x => x.BomHeaderId).GreaterThan(0);
        RuleFor(x => x.ComponentProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitMeasureCode).NotEmpty().MaximumLength(3);
        RuleFor(x => x.ScrapPercentage).InclusiveBetween(0, 1);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public sealed class CreateEngineeringChangeOrderValidator : AbstractValidator<CreateEngineeringChangeOrderRequest>
{
    public CreateEngineeringChangeOrderValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(32).Matches("^[A-Z0-9][A-Z0-9_-]*$");
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
    }
}

public sealed class UpdateEngineeringChangeOrderValidator : AbstractValidator<UpdateEngineeringChangeOrderRequest>
{
    public UpdateEngineeringChangeOrderValidator()
    {
        When(x => x.Title is not null, () => RuleFor(x => x.Title!).NotEmpty().MaximumLength(200));
        When(x => x.Description is not null, () => RuleFor(x => x.Description!).MaximumLength(4000));
    }
}

public sealed class CreateEcoAffectedItemValidator : AbstractValidator<CreateEcoAffectedItemRequest>
{
    public CreateEcoAffectedItemValidator()
    {
        RuleFor(x => x.TargetId).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public sealed class CreateEngineeringDocumentValidator : AbstractValidator<CreateEngineeringDocumentRequest>
{
    public CreateEngineeringDocumentValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(32).Matches("^[A-Z0-9][A-Z0-9_-]*$");
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RevisionNumber).GreaterThan(0);
        RuleFor(x => x.Url).MaximumLength(1000);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}

public sealed class UpdateEngineeringDocumentValidator : AbstractValidator<UpdateEngineeringDocumentRequest>
{
    public UpdateEngineeringDocumentValidator()
    {
        When(x => x.Title is not null, () => RuleFor(x => x.Title!).NotEmpty().MaximumLength(200));
        When(x => x.RevisionNumber.HasValue, () => RuleFor(x => x.RevisionNumber!.Value).GreaterThan(0));
        When(x => x.Url is not null, () => RuleFor(x => x.Url!).MaximumLength(1000));
        When(x => x.Description is not null, () => RuleFor(x => x.Description!).MaximumLength(2000));
    }
}

public sealed class CreateDeviationRequestValidator : AbstractValidator<CreateDeviationRequestRequest>
{
    public CreateDeviationRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(32).Matches("^[A-Z0-9][A-Z0-9_-]*$");
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.ProposedDisposition).MaximumLength(2000);
        When(x => x.AuthorizedQuantity.HasValue, () =>
            RuleFor(x => x.AuthorizedQuantity!.Value).GreaterThan(0));
        When(x => x.UnitMeasureCode is not null, () => RuleFor(x => x.UnitMeasureCode!).MaximumLength(3));
        When(x => x.ValidFrom.HasValue && x.ValidTo.HasValue, () =>
            RuleFor(x => x.ValidTo!.Value).GreaterThanOrEqualTo(x => x.ValidFrom!.Value)
                .WithMessage("ValidTo must be on or after ValidFrom."));
    }
}
