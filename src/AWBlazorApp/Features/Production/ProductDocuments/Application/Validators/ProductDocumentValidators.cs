using AWBlazorApp.Features.Production.Cultures.Dtos; using AWBlazorApp.Features.Production.Documents.Dtos; using AWBlazorApp.Features.Production.Illustrations.Dtos; using AWBlazorApp.Features.Production.Locations.Dtos; using AWBlazorApp.Features.Production.ProductCategories.Dtos; using AWBlazorApp.Features.Production.ProductCostHistories.Dtos; using AWBlazorApp.Features.Production.ProductDescriptions.Dtos; using AWBlazorApp.Features.Production.ProductDocuments.Dtos; using AWBlazorApp.Features.Production.ProductInventories.Dtos; using AWBlazorApp.Features.Production.ProductListPriceHistories.Dtos; using AWBlazorApp.Features.Production.ProductModels.Dtos; using AWBlazorApp.Features.Production.ProductModelIllustrations.Dtos; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Dtos; using AWBlazorApp.Features.Production.ProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductReviews.Dtos; using AWBlazorApp.Features.Production.Products.Dtos; using AWBlazorApp.Features.Production.ProductSubcategories.Dtos; using AWBlazorApp.Features.Production.ScrapReasons.Dtos; using AWBlazorApp.Features.Production.TransactionHistories.Dtos; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Dtos; using AWBlazorApp.Features.Production.UnitMeasures.Dtos; using AWBlazorApp.Features.Production.WorkOrders.Dtos; using AWBlazorApp.Features.Production.WorkOrderRoutings.Dtos; 
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Production.ProductDocuments.Application.Validators;

public sealed class CreateProductDocumentValidator : AbstractValidator<CreateProductDocumentRequest>
{
    public CreateProductDocumentValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("ProductId is required.");
        RuleFor(x => x.DocumentNode)
            .NotEmpty().WithMessage("DocumentNode (hierarchyid path) is required.")
            .Must(BeAValidHierarchyId).WithMessage("DocumentNode must be a valid hierarchyid path.");
    }

    private static bool BeAValidHierarchyId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        try { HierarchyId.Parse(value); return true; }
        catch { return false; }
    }
}

public sealed class UpdateProductDocumentValidator : AbstractValidator<UpdateProductDocumentRequest>
{
    public UpdateProductDocumentValidator() { }
}
