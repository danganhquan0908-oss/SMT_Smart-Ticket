namespace SmartTicket.Application.Payments;

public sealed record PaymentCheckoutDto(Guid BookingId, string BookingCode, decimal Amount, DateTime ExpiresAtUtc, string QrPayload, string LookupToken);
public sealed record PaymentStateDto(string Status, DateTime? PaidAtUtc, string? TicketLookupUrl);
public sealed record TicketDto(string TicketNumber, string PassengerName, string Trip, string SeatCode, DateTime DepartureTimeUtc, decimal Price, string CheckInJwt);
public sealed record TicketLookupDto(string BookingCode, string BookingStatus, IReadOnlyList<TicketDto> Tickets);
public sealed record PaymentWebhookPayload(string Reference, string TransactionId, decimal Amount, string Status);
public sealed record WebhookProcessResult(bool Processed, string Result, Guid? TripId = null, IReadOnlyList<string>? SeatCodes = null);
public interface IPaymentService
{
    Task<PaymentCheckoutDto> CreateCheckoutAsync(string lookupToken, CancellationToken ct);
    Task<PaymentStateDto?> GetPaymentStateAsync(string lookupToken, CancellationToken ct);
    Task<WebhookProcessResult> ProcessWebhookAsync(PaymentWebhookPayload payload, string rawPayload, CancellationToken ct);
    Task<TicketLookupDto?> GetTicketsAsync(string lookupToken, CancellationToken ct);
}
public sealed class PaymentNotFoundException(string message) : Exception(message);
public sealed class PaymentConflictException(string message) : Exception(message);
