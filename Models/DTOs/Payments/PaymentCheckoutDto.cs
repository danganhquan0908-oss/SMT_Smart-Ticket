namespace BusGoSmartTicket.Models.DTOs.Payments;

public sealed record PaymentCheckoutDto(Guid BookingId, string BookingCode, decimal Amount, DateTime ExpiresAtUtc, string QrPayload, string LookupToken);
