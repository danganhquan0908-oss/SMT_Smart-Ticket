namespace BusGoSmartTicket.Models.DTOs.Payments;

public sealed record PaymentStateDto(string Status, DateTime? PaidAtUtc, string? TicketLookupUrl);
