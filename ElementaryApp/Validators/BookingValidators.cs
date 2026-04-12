using ElementaryApp.Models;
using FluentValidation;

namespace ElementaryApp.Validators;

public sealed class CreateBookingValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(256);

        RuleFor(x => x.RoomNumber)
            .GreaterThan(0).WithMessage("RoomNumber must be greater than 0.");

        RuleFor(x => x.Cost)
            .GreaterThan(0m).WithMessage("Cost must be greater than 0.");

        RuleFor(x => x.BookingStartDate)
            .NotEqual(default(DateTime)).WithMessage("BookingStartDate is required.");

        RuleFor(x => x.BookingEndDate)
            .GreaterThanOrEqualTo(x => x.BookingStartDate)
            .When(x => x.BookingEndDate.HasValue)
            .WithMessage("BookingEndDate must be on or after BookingStartDate.");
    }
}

public sealed class UpdateBookingValidator : AbstractValidator<UpdateBookingRequest>
{
    public UpdateBookingValidator()
    {
        RuleFor(x => x.Name!)
            .MaximumLength(256)
            .When(x => x.Name is not null);

        RuleFor(x => x.RoomNumber!.Value)
            .GreaterThan(0).WithMessage("RoomNumber must be greater than 0.")
            .When(x => x.RoomNumber.HasValue);

        RuleFor(x => x.Cost!.Value)
            .GreaterThan(0m).WithMessage("Cost must be greater than 0.")
            .When(x => x.Cost.HasValue);

        RuleFor(x => x.BookingEndDate)
            .GreaterThanOrEqualTo(x => x.BookingStartDate)
            .When(x => x.BookingStartDate.HasValue && x.BookingEndDate.HasValue)
            .WithMessage("BookingEndDate must be on or after BookingStartDate.");
    }
}
