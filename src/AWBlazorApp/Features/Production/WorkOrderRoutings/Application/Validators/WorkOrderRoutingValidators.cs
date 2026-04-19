using AWBlazorApp.Features.Production.Cultures.Dtos; using AWBlazorApp.Features.Production.Documents.Dtos; using AWBlazorApp.Features.Production.Illustrations.Dtos; using AWBlazorApp.Features.Production.Locations.Dtos; using AWBlazorApp.Features.Production.ProductCategories.Dtos; using AWBlazorApp.Features.Production.ProductCostHistories.Dtos; using AWBlazorApp.Features.Production.ProductDescriptions.Dtos; using AWBlazorApp.Features.Production.ProductDocuments.Dtos; using AWBlazorApp.Features.Production.ProductInventories.Dtos; using AWBlazorApp.Features.Production.ProductListPriceHistories.Dtos; using AWBlazorApp.Features.Production.ProductModels.Dtos; using AWBlazorApp.Features.Production.ProductModelIllustrations.Dtos; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Dtos; using AWBlazorApp.Features.Production.ProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductReviews.Dtos; using AWBlazorApp.Features.Production.Products.Dtos; using AWBlazorApp.Features.Production.ProductSubcategories.Dtos; using AWBlazorApp.Features.Production.ScrapReasons.Dtos; using AWBlazorApp.Features.Production.TransactionHistories.Dtos; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Dtos; using AWBlazorApp.Features.Production.UnitMeasures.Dtos; using AWBlazorApp.Features.Production.WorkOrders.Dtos; using AWBlazorApp.Features.Production.WorkOrderRoutings.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Production.WorkOrderRoutings.Application.Validators;

public sealed class CreateWorkOrderRoutingValidator : AbstractValidator<CreateWorkOrderRoutingRequest>
{
    public CreateWorkOrderRoutingValidator()
    {
        RuleFor(x => x.WorkOrderId).GreaterThan(0).WithMessage("WorkOrderId is required.");
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("ProductId is required.");
        RuleFor(x => x.OperationSequence).GreaterThan((short)0).WithMessage("OperationSequence must be > 0.");
        RuleFor(x => x.LocationId).GreaterThan((short)0).WithMessage("LocationId is required.");
        RuleFor(x => x.PlannedCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ScheduledEndDate).GreaterThanOrEqualTo(x => x.ScheduledStartDate)
            .WithMessage("ScheduledEndDate cannot be earlier than ScheduledStartDate.");
        When(x => x.ActualResourceHrs.HasValue, () =>
            RuleFor(x => x.ActualResourceHrs!.Value).GreaterThanOrEqualTo(0));
        When(x => x.ActualCost.HasValue, () =>
            RuleFor(x => x.ActualCost!.Value).GreaterThanOrEqualTo(0));
    }
}

public sealed class UpdateWorkOrderRoutingValidator : AbstractValidator<UpdateWorkOrderRoutingRequest>
{
    public UpdateWorkOrderRoutingValidator()
    {
        When(x => x.LocationId.HasValue, () => RuleFor(x => x.LocationId!.Value).GreaterThan((short)0));
        When(x => x.PlannedCost.HasValue, () => RuleFor(x => x.PlannedCost!.Value).GreaterThanOrEqualTo(0));
        When(x => x.ActualResourceHrs.HasValue, () => RuleFor(x => x.ActualResourceHrs!.Value).GreaterThanOrEqualTo(0));
        When(x => x.ActualCost.HasValue, () => RuleFor(x => x.ActualCost!.Value).GreaterThanOrEqualTo(0));
    }
}
