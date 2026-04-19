using AWBlazorApp.Features.Production.Dtos; using AWBlazorApp.Features.Production.Cultures.Dtos; using AWBlazorApp.Features.Production.Documents.Dtos; using AWBlazorApp.Features.Production.Illustrations.Dtos; using AWBlazorApp.Features.Production.Locations.Dtos; using AWBlazorApp.Features.Production.ProductCategories.Dtos; using AWBlazorApp.Features.Production.ProductCostHistories.Dtos; using AWBlazorApp.Features.Production.ProductDescriptions.Dtos; using AWBlazorApp.Features.Production.ProductDocuments.Dtos; using AWBlazorApp.Features.Production.ProductInventories.Dtos; using AWBlazorApp.Features.Production.ProductListPriceHistories.Dtos; using AWBlazorApp.Features.Production.ProductModels.Dtos; using AWBlazorApp.Features.Production.ProductModelIllustrations.Dtos; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Dtos; using AWBlazorApp.Features.Production.ProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductReviews.Dtos; using AWBlazorApp.Features.Production.Products.Dtos; using AWBlazorApp.Features.Production.ProductSubcategories.Dtos; using AWBlazorApp.Features.Production.ScrapReasons.Dtos; using AWBlazorApp.Features.Production.TransactionHistories.Dtos; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Dtos; using AWBlazorApp.Features.Production.UnitMeasures.Dtos; using AWBlazorApp.Features.Production.WorkOrders.Dtos; using AWBlazorApp.Features.Production.WorkOrderRoutings.Dtos; 
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Production.Documents.Application.Validators;

public sealed class CreateDocumentValidator : AbstractValidator<CreateDocumentRequest>
{
    public CreateDocumentValidator()
    {
        RuleFor(x => x.DocumentNode)
            .NotEmpty().WithMessage("DocumentNode (hierarchyid path) is required, e.g. \"/1/\" or \"/1/2/\".")
            .Must(BeAValidHierarchyId).WithMessage("DocumentNode must be a valid hierarchyid path (e.g. \"/\", \"/1/\", \"/1/2/\").");
        RuleFor(x => x.Title).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Owner).GreaterThan(0).WithMessage("Owner (EmployeeID) is required.");
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(400);
        RuleFor(x => x.FileExtension).NotEmpty().MaximumLength(8);
        RuleFor(x => x.Revision).NotEmpty().MaximumLength(5);
        RuleFor(x => x.ChangeNumber).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Status).InclusiveBetween((byte)1, (byte)3)
            .WithMessage("Status must be 1 (Pending), 2 (Approved), or 3 (Obsolete).");
    }

    private static bool BeAValidHierarchyId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        try { HierarchyId.Parse(value); return true; }
        catch { return false; }
    }
}

public sealed class UpdateDocumentValidator : AbstractValidator<UpdateDocumentRequest>
{
    public UpdateDocumentValidator()
    {
        When(x => x.Title is not null, () => RuleFor(x => x.Title!).NotEmpty().MaximumLength(50));
        When(x => x.Owner.HasValue, () => RuleFor(x => x.Owner!.Value).GreaterThan(0));
        When(x => x.FileName is not null, () => RuleFor(x => x.FileName!).NotEmpty().MaximumLength(400));
        When(x => x.FileExtension is not null, () => RuleFor(x => x.FileExtension!).NotEmpty().MaximumLength(8));
        When(x => x.Revision is not null, () => RuleFor(x => x.Revision!).NotEmpty().MaximumLength(5));
        When(x => x.ChangeNumber.HasValue, () => RuleFor(x => x.ChangeNumber!.Value).GreaterThanOrEqualTo(0));
        When(x => x.Status.HasValue, () =>
            RuleFor(x => x.Status!.Value).InclusiveBetween((byte)1, (byte)3));
    }
}
