namespace BusGoSmartTicket.Models.DTOs.Bookings;

public sealed record BookingHoldResult(Guid BookingId, string BookingCode, DateTime ExpiresAtUtc, decimal TotalAmount, IReadOnlyList<string> SeatCodes, string LookupToken);
