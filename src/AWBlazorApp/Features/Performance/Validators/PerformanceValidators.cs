using AWBlazorApp.Features.Performance.Kpis.Dtos; using AWBlazorApp.Features.Performance.ProductionMetrics.Dtos; using AWBlazorApp.Features.Performance.Scorecards.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Performance.Validators;

public sealed class ComputeOeeValidator : AbstractValidator<ComputeOeeRequest>
{
    public ComputeOeeValidator()
    {
        RuleFor(x => x.StationId).GreaterThan(0);
        RuleFor(x => x.PeriodEnd).GreaterThan(x => x.PeriodStart);
        RuleFor(x => x.IdealCycleSeconds).GreaterThan(0);
    }
}

public sealed class ComputeProductionMetricValidator : AbstractValidator<ComputeProductionMetricRequest>
{
    public ComputeProductionMetricValidator()
    {
        RuleFor(x => x.StationId).GreaterThan(0);
    }
}

public sealed class ComputeMaintenanceMetricValidator : AbstractValidator<ComputeMaintenanceMetricRequest>
{
    public ComputeMaintenanceMetricValidator()
    {
        RuleFor(x => x.AssetId).GreaterThan(0);
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
    }
}

public sealed class CreateKpiDefinitionValidator : AbstractValidator<CreateKpiDefinitionRequest>
{
    public CreateKpiDefinitionValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(32).Matches("^[A-Z0-9][A-Z0-9_-]*$");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Unit).MaximumLength(32);
    }
}

public sealed class UpdateKpiDefinitionValidator : AbstractValidator<UpdateKpiDefinitionRequest>
{
    public UpdateKpiDefinitionValidator()
    {
        When(x => x.Name is not null, () => RuleFor(x => x.Name!).NotEmpty().MaximumLength(200));
        When(x => x.Description is not null, () => RuleFor(x => x.Description!).MaximumLength(2000));
        When(x => x.Unit is not null, () => RuleFor(x => x.Unit!).MaximumLength(32));
    }
}

public sealed class EvaluateKpiValidator : AbstractValidator<EvaluateKpiRequest>
{
    public EvaluateKpiValidator()
    {
        RuleFor(x => x.KpiDefinitionId).GreaterThan(0);
        RuleFor(x => x.PeriodEnd).GreaterThan(x => x.PeriodStart);
    }
}

public sealed class CreateScorecardDefinitionValidator : AbstractValidator<CreateScorecardDefinitionRequest>
{
    public CreateScorecardDefinitionValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(32).Matches("^[A-Z0-9][A-Z0-9_-]*$");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}

public sealed class UpdateScorecardDefinitionValidator : AbstractValidator<UpdateScorecardDefinitionRequest>
{
    public UpdateScorecardDefinitionValidator()
    {
        When(x => x.Name is not null, () => RuleFor(x => x.Name!).NotEmpty().MaximumLength(200));
        When(x => x.Description is not null, () => RuleFor(x => x.Description!).MaximumLength(2000));
    }
}

public sealed class CreateScorecardKpiValidator : AbstractValidator<CreateScorecardKpiRequest>
{
    public CreateScorecardKpiValidator()
    {
        RuleFor(x => x.ScorecardDefinitionId).GreaterThan(0);
        RuleFor(x => x.KpiDefinitionId).GreaterThan(0);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreatePerformanceReportValidator : AbstractValidator<CreatePerformanceReportRequest>
{
    public CreatePerformanceReportValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(32).Matches("^[A-Z0-9][A-Z0-9_-]*$");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        When(x => x.StationId.HasValue, () => RuleFor(x => x.StationId!.Value).GreaterThan(0));
        When(x => x.AssetId.HasValue, () => RuleFor(x => x.AssetId!.Value).GreaterThan(0));
    }
}

public sealed class UpdatePerformanceReportValidator : AbstractValidator<UpdatePerformanceReportRequest>
{
    public UpdatePerformanceReportValidator()
    {
        When(x => x.Name is not null, () => RuleFor(x => x.Name!).NotEmpty().MaximumLength(200));
        When(x => x.Description is not null, () => RuleFor(x => x.Description!).MaximumLength(2000));
        When(x => x.StationId.HasValue, () => RuleFor(x => x.StationId!.Value).GreaterThan(0));
        When(x => x.AssetId.HasValue, () => RuleFor(x => x.AssetId!.Value).GreaterThan(0));
    }
}
