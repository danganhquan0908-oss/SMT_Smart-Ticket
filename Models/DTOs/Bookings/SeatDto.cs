namespace BusGoSmartTicket.Models.DTOs.Bookings;

public sealed record SeatDto(Guid Id, string Code, int Deck, int Row, int Column, string Status, DateTime? LockedUntilUtc);
