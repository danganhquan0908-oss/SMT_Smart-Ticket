namespace BusGoSmartTicket.Models.DTOs.Bookings;

public sealed record HoldSeatsRequest(string PassengerName, string PassengerPhone, string? PassengerEmail, IReadOnlyList<string> SeatCodes, Guid? CustomerUserId = null, decimal? ExpectedUnitPrice = null);
