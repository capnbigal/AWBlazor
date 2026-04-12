using ElementaryApp.Data.Entities;

namespace ElementaryApp.Models;

public sealed record BookingDto(
    int Id,
    string Name,
    RoomType RoomType,
    int RoomNumber,
    DateTime BookingStartDate,
    DateTime? BookingEndDate,
    decimal Cost,
    string? CouponId,
    string? Notes,
    bool? Cancelled,
    string? CreatedBy,
    DateTime CreatedDate,
    string? ModifiedBy,
    DateTime ModifiedDate);

public sealed record CreateBookingRequest
{
    public string Name { get; set; } = string.Empty;
    public RoomType RoomType { get; set; }
    public int RoomNumber { get; set; }
    public decimal Cost { get; set; }
    public DateTime BookingStartDate { get; set; }
    public DateTime? BookingEndDate { get; set; }
    public string? Notes { get; set; }
    public string? CouponId { get; set; }
}

public sealed record UpdateBookingRequest
{
    public string? Name { get; set; }
    public RoomType? RoomType { get; set; }
    public int? RoomNumber { get; set; }
    public decimal? Cost { get; set; }
    public DateTime? BookingStartDate { get; set; }
    public DateTime? BookingEndDate { get; set; }
    public string? Notes { get; set; }
    public string? CouponId { get; set; }
    public bool? Cancelled { get; set; }
}

public static class BookingMappings
{
    public static BookingDto ToDto(this Booking b) => new(
        b.Id, b.Name, b.RoomType, b.RoomNumber, b.BookingStartDate, b.BookingEndDate,
        b.Cost, b.CouponId, b.Notes, b.Cancelled,
        b.CreatedBy, b.CreatedDate, b.ModifiedBy, b.ModifiedDate);

    public static Booking ToEntity(this CreateBookingRequest r) => new()
    {
        Name = r.Name,
        RoomType = r.RoomType,
        RoomNumber = r.RoomNumber,
        Cost = r.Cost,
        BookingStartDate = r.BookingStartDate,
        BookingEndDate = r.BookingEndDate,
        Notes = r.Notes,
        CouponId = r.CouponId,
    };

    public static void ApplyTo(this UpdateBookingRequest r, Booking b)
    {
        if (r.Name is not null) b.Name = r.Name;
        if (r.RoomType.HasValue) b.RoomType = r.RoomType.Value;
        if (r.RoomNumber.HasValue) b.RoomNumber = r.RoomNumber.Value;
        if (r.Cost.HasValue) b.Cost = r.Cost.Value;
        if (r.BookingStartDate.HasValue) b.BookingStartDate = r.BookingStartDate.Value;
        if (r.BookingEndDate.HasValue) b.BookingEndDate = r.BookingEndDate;
        if (r.Notes is not null) b.Notes = r.Notes;
        if (r.CouponId is not null) b.CouponId = r.CouponId;
        if (r.Cancelled.HasValue) b.Cancelled = r.Cancelled;
    }
}
