namespace BusGoSmartTicket.Models.DTOs.Account;

public sealed record CustomerBookingSummaryDto(
    Guid BookingId,
    string BookingCode,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAtUtc,
    DateTime ExpiresAtUtc,
    string DepartureStation,
    string ArrivalStation,
    string OperatorName,
    string VehicleType,
    DateTime DepartureTimeUtc,
    IReadOnlyList<string> SeatCodes,
    IReadOnlyList<CustomerTicketDto> Tickets,
    string? LookupToken = null);
