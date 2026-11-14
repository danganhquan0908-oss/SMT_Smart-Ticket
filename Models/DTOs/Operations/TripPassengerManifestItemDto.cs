namespace BusGoSmartTicket.Models.DTOs.Operations;

public sealed record TripPassengerManifestItemDto(
    Guid TicketId,
    string SeatCode,
    string PassengerName,
    string PassengerPhone,
    string Status,
    decimal Price,
    DateTime? CheckedInAtUtc,
    string? CheckInToken
);
