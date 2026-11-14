namespace BusGoSmartTicket.Models.DTOs.Account;

public sealed record CustomerTicketDto(
    Guid TicketId,
    string TicketNumber,
    string SeatCode,
    decimal Price,
    DateTime IssuedAtUtc,
    string Status,
    string CheckInJwt);
