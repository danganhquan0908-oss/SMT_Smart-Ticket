namespace BusGoSmartTicket.Models.DTOs.Payments;

public sealed record PaymentWebhookPayload(string Reference, string TransactionId, decimal Amount, string Status);
