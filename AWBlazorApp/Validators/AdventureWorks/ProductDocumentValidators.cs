using AWBlazorApp.Models.AdventureWorks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Validators.AdventureWorks;

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
