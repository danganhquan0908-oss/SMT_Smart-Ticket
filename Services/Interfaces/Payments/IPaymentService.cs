using BusGoSmartTicket.Models.DTOs.Payments;
namespace BusGoSmartTicket.Services.Interfaces.Payments;

public interface IPaymentService
{
    Task<PaymentCheckoutDto> CreateCheckoutAsync(string lookupToken, CancellationToken ct);
    Task<PaymentStateDto?> GetPaymentStateAsync(string lookupToken, CancellationToken ct);
    Task<WebhookProcessResult> ProcessWebhookAsync(PaymentWebhookPayload payload, string rawPayload, CancellationToken ct);
    Task<TicketLookupDto?> GetTicketsAsync(string lookupToken, CancellationToken ct);
}
