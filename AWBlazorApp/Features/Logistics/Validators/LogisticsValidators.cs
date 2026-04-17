using AWBlazorApp.Features.Logistics.Models;
using FluentValidation;

namespace AWBlazorApp.Features.Logistics.Validators;

public sealed class CreateGoodsReceiptValidator : AbstractValidator<CreateGoodsReceiptRequest>
{
    public CreateGoodsReceiptValidator()
    {
        RuleFor(x => x.ReceivedLocationId).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
public sealed class UpdateGoodsReceiptValidator : AbstractValidator<UpdateGoodsReceiptRequest>
{
    public UpdateGoodsReceiptValidator()
    {
        When(x => x.Notes is not null, () => RuleFor(x => x.Notes!).MaximumLength(500));
    }
}

public sealed class CreateGoodsReceiptLineValidator : AbstractValidator<CreateGoodsReceiptLineRequest>
{
    public CreateGoodsReceiptLineValidator()
    {
        RuleFor(x => x.GoodsReceiptId).GreaterThan(0);
        RuleFor(x => x.InventoryItemId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitMeasureCode).MaximumLength(3);
    }
}
public sealed class UpdateGoodsReceiptLineValidator : AbstractValidator<UpdateGoodsReceiptLineRequest>
{
    public UpdateGoodsReceiptLineValidator()
    {
        When(x => x.Quantity.HasValue, () => RuleFor(x => x.Quantity!.Value).GreaterThan(0));
        When(x => x.UnitMeasureCode is not null, () => RuleFor(x => x.UnitMeasureCode!).MaximumLength(3));
    }
}

public sealed class CreateShipmentValidator : AbstractValidator<CreateShipmentRequest>
{
    public CreateShipmentValidator()
    {
        RuleFor(x => x.ShippedFromLocationId).GreaterThan(0);
        RuleFor(x => x.TrackingNumber).MaximumLength(128);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
public sealed class UpdateShipmentValidator : AbstractValidator<UpdateShipmentRequest>
{
    public UpdateShipmentValidator()
    {
        When(x => x.TrackingNumber is not null, () => RuleFor(x => x.TrackingNumber!).MaximumLength(128));
        When(x => x.Notes is not null, () => RuleFor(x => x.Notes!).MaximumLength(500));
    }
}

public sealed class CreateShipmentLineValidator : AbstractValidator<CreateShipmentLineRequest>
{
    public CreateShipmentLineValidator()
    {
        RuleFor(x => x.ShipmentId).GreaterThan(0);
        RuleFor(x => x.InventoryItemId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitMeasureCode).MaximumLength(3);
    }
}
public sealed class UpdateShipmentLineValidator : AbstractValidator<UpdateShipmentLineRequest>
{
    public UpdateShipmentLineValidator()
    {
        When(x => x.Quantity.HasValue, () => RuleFor(x => x.Quantity!.Value).GreaterThan(0));
    }
}

public sealed class CreateStockTransferValidator : AbstractValidator<CreateStockTransferRequest>
{
    public CreateStockTransferValidator()
    {
        RuleFor(x => x.FromLocationId).GreaterThan(0);
        RuleFor(x => x.ToLocationId).GreaterThan(0)
            .NotEqual(x => x.FromLocationId).WithMessage("From and To locations must differ.");
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
public sealed class UpdateStockTransferValidator : AbstractValidator<UpdateStockTransferRequest>
{
    public UpdateStockTransferValidator()
    {
        When(x => x.Notes is not null, () => RuleFor(x => x.Notes!).MaximumLength(500));
    }
}

public sealed class CreateStockTransferLineValidator : AbstractValidator<CreateStockTransferLineRequest>
{
    public CreateStockTransferLineValidator()
    {
        RuleFor(x => x.StockTransferId).GreaterThan(0);
        RuleFor(x => x.InventoryItemId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitMeasureCode).MaximumLength(3);
    }
}
public sealed class UpdateStockTransferLineValidator : AbstractValidator<UpdateStockTransferLineRequest>
{
    public UpdateStockTransferLineValidator()
    {
        When(x => x.Quantity.HasValue, () => RuleFor(x => x.Quantity!.Value).GreaterThan(0));
    }
}
