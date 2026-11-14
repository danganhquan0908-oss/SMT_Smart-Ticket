namespace BusGoSmartTicket.Models.Entities;

public sealed class Payment
{
    private Payment()
    {
    }
    public Payment(Guid bookingId, string reference, decimal amount)
    {
        Id = Guid.NewGuid();
        BookingId = bookingId;
        Reference = reference;
        Amount = amount;
        Status = PaymentStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }
    public Guid Id
    {
        get; private set;
    }
    public Guid BookingId
    {
        get; private set;
    }
    public Booking? Booking
    {
        get; private set;
    }
    public string Reference { get; private set; } = "";
    public string? ProviderTransactionId
    {
        get; private set;
    }
    public decimal Amount
    {
        get; private set;
    }
    public PaymentStatus Status
    {
        get; private set;
    }
    public string? RawWebhookPayload
    {
        get; private set;
    }
    public DateTime CreatedAtUtc
    {
        get; private set;
    }
    public DateTime? CompletedAtUtc
    {
        get; private set;
    }
    public void MarkSucceeded(string transactionId, string rawPayload)
    {
        Status = PaymentStatus.Succeeded;
        ProviderTransactionId = transactionId;
        RawWebhookPayload = rawPayload;
        CompletedAtUtc = DateTime.UtcNow;
    }
    public void MarkOverdue(string transactionId, string rawPayload)
    {
        Status = PaymentStatus.OverdueUnassigned;
        ProviderTransactionId = transactionId;
        RawWebhookPayload = rawPayload;
        CompletedAtUtc = DateTime.UtcNow;
    }
    public void MarkFailed(string transactionId, string rawPayload)
    {
        Status = PaymentStatus.Failed;
        ProviderTransactionId = transactionId;
        RawWebhookPayload = rawPayload;
        CompletedAtUtc = DateTime.UtcNow;
    }
}
