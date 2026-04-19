using AWBlazorApp.Features.Purchasing.Dtos;
using FluentValidation;

namespace AWBlazorApp.Features.Purchasing.Validators;

public sealed class CreatePurchaseOrderHeaderValidator : AbstractValidator<CreatePurchaseOrderHeaderRequest>
{
    public CreatePurchaseOrderHeaderValidator()
    {
        RuleFor(x => x.Status).InclusiveBetween((byte)1, (byte)4).WithMessage("Status must be 1–4 (Pending/Approved/Rejected/Complete).");
        RuleFor(x => x.EmployeeId).GreaterThan(0).WithMessage("EmployeeId is required.");
        RuleFor(x => x.VendorId).GreaterThan(0).WithMessage("VendorId is required.");
        RuleFor(x => x.ShipMethodId).GreaterThan(0).WithMessage("ShipMethodId is required.");
        RuleFor(x => x.OrderDate).NotEmpty();
        RuleFor(x => x.SubTotal).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TaxAmt).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Freight).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdatePurchaseOrderHeaderValidator : AbstractValidator<UpdatePurchaseOrderHeaderRequest>
{
    public UpdatePurchaseOrderHeaderValidator()
    {
        When(x => x.Status.HasValue, () =>
            RuleFor(x => x.Status!.Value).InclusiveBetween((byte)1, (byte)4).WithMessage("Status must be 1–4."));
        When(x => x.EmployeeId.HasValue, () => RuleFor(x => x.EmployeeId!.Value).GreaterThan(0));
        When(x => x.VendorId.HasValue, () => RuleFor(x => x.VendorId!.Value).GreaterThan(0));
        When(x => x.ShipMethodId.HasValue, () => RuleFor(x => x.ShipMethodId!.Value).GreaterThan(0));
        When(x => x.SubTotal.HasValue, () => RuleFor(x => x.SubTotal!.Value).GreaterThanOrEqualTo(0));
        When(x => x.TaxAmt.HasValue, () => RuleFor(x => x.TaxAmt!.Value).GreaterThanOrEqualTo(0));
        When(x => x.Freight.HasValue, () => RuleFor(x => x.Freight!.Value).GreaterThanOrEqualTo(0));
    }
}
