using AWBlazorApp.Features.Purchasing.ProductVendors.Dtos; using AWBlazorApp.Features.Purchasing.PurchaseOrderDetails.Dtos; using AWBlazorApp.Features.Purchasing.PurchaseOrderHeaders.Dtos; using AWBlazorApp.Features.Purchasing.ShipMethods.Dtos; using AWBlazorApp.Features.Purchasing.Vendors.Dtos; 
using FluentValidation;

namespace AWBlazorApp.Features.Purchasing.PurchaseOrderDetails.Application.Validators;

public sealed class CreatePurchaseOrderDetailValidator : AbstractValidator<CreatePurchaseOrderDetailRequest>
{
    public CreatePurchaseOrderDetailValidator()
    {
        RuleFor(x => x.PurchaseOrderId).GreaterThan(0).WithMessage("PurchaseOrderId is required.");
        RuleFor(x => x.DueDate).NotEmpty();
        RuleFor(x => x.OrderQty).GreaterThan((short)0).WithMessage("OrderQty must be greater than zero.");
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("ProductId is required.");
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).WithMessage("UnitPrice cannot be negative.");
        RuleFor(x => x.ReceivedQty).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RejectedQty).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdatePurchaseOrderDetailValidator : AbstractValidator<UpdatePurchaseOrderDetailRequest>
{
    public UpdatePurchaseOrderDetailValidator()
    {
        When(x => x.OrderQty.HasValue, () => RuleFor(x => x.OrderQty!.Value).GreaterThan((short)0));
        When(x => x.ProductId.HasValue, () => RuleFor(x => x.ProductId!.Value).GreaterThan(0));
        When(x => x.UnitPrice.HasValue, () => RuleFor(x => x.UnitPrice!.Value).GreaterThanOrEqualTo(0));
        When(x => x.ReceivedQty.HasValue, () => RuleFor(x => x.ReceivedQty!.Value).GreaterThanOrEqualTo(0));
        When(x => x.RejectedQty.HasValue, () => RuleFor(x => x.RejectedQty!.Value).GreaterThanOrEqualTo(0));
    }
}
