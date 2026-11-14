namespace BusGoSmartTicket.Models.DTOs.Payments;

public sealed record TicketLookupDto(string BookingCode, string BookingStatus, IReadOnlyList<TicketDto> Tickets);
