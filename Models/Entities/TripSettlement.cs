namespace BusGoSmartTicket.Models.Entities;

public sealed class TripSettlement
{
    private TripSettlement()
    {
    }
    public TripSettlement(Guid tripId, Guid operatorId, decimal grossAmount, decimal commissionAmount)
    {
        Id = Guid.NewGuid();
        TripId = tripId;
        OperatorId = operatorId;
        GrossAmount = grossAmount;
        CommissionAmount = commissionAmount;
        NetAmount = grossAmount - commissionAmount;
        Status = SettlementStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }
    public Guid Id
    {
        get; private set;
    }
    public Guid TripId
    {
        get; private set;
    }
    public Trip? Trip
    {
        get; private set;
    }
    public Guid OperatorId
    {
        get; private set;
    }
    public BusOperator? Operator
    {
        get; private set;
    }
    public decimal GrossAmount
    {
        get; private set;
    }
    public decimal CommissionAmount
    {
        get; private set;
    }
    public decimal NetAmount
    {
        get; private set;
    }
    public SettlementStatus Status
    {
        get; private set;
    }
    public string? ProviderReference
    {
        get; private set;
    }
    public string? FailureReason
    {
        get; private set;
    }
    public DateTime CreatedAtUtc
    {
        get; private set;
    }
    public DateTime? ProcessingAtUtc
    {
        get; private set;
    }
    public DateTime? PaidAtUtc
    {
        get; private set;
    }
    public void MarkProcessing()
    {
        if (Status == SettlementStatus.Pending || Status == SettlementStatus.Failed || (Status == SettlementStatus.Processing && ProcessingAtUtc < DateTime.UtcNow.AddMinutes(-5)))
        {
            Status = SettlementStatus.Processing;
            ProcessingAtUtc = DateTime.UtcNow;
        }
    }
    public void MarkPaid(string providerReference)
    {
        Status = SettlementStatus.Paid;
        ProviderReference = providerReference;
        PaidAtUtc = DateTime.UtcNow;
        FailureReason = null;
    }
    public void MarkFailed(string reason)
    {
        Status = SettlementStatus.Failed;
        FailureReason = reason;
    }
}
