namespace SmartTicket.Domain.Entities;

public sealed class BusOperator
{
    private BusOperator() { }

    public BusOperator(Guid ownerUserId, string companyName, string taxCode, string hotline, string licensePath)
    {
        Id = Guid.NewGuid(); OwnerUserId = ownerUserId; CompanyName = companyName.Trim();
        TaxCode = taxCode.Trim(); Hotline = hotline.Trim(); BusinessLicensePath = licensePath;
        Status = OperatorStatus.PendingVerification; CreatedAtUtc = DateTime.UtcNow;
    }
    public Guid Id { get; private set; }
    public Guid OwnerUserId { get; private set; }
    public string CompanyName { get; private set; } = string.Empty;
    public string TaxCode { get; private set; } = string.Empty;
    public string Hotline { get; private set; } = string.Empty;
    public string BusinessLicensePath { get; private set; } = string.Empty;
    public OperatorStatus Status { get; private set; }
    public string? ReviewNote { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ReviewedAtUtc { get; private set; }
    public string? PayoutBankBin { get; private set; }
    public string? PayoutAccountNumber { get; private set; }
    public string? PayoutAccountName { get; private set; }

    public void Approve() { Status = OperatorStatus.Active; ReviewNote = null; ReviewedAtUtc = DateTime.UtcNow; }
    public void Reject(string note) { Status = OperatorStatus.Rejected; ReviewNote = note.Trim(); ReviewedAtUtc = DateTime.UtcNow; }
    public void Suspend(string note) { Status = OperatorStatus.Suspended; ReviewNote = note.Trim(); }
    public void SetPayoutAccount(string bankBin, string accountNumber, string accountName)
    { PayoutBankBin = bankBin.Trim(); PayoutAccountNumber = accountNumber.Trim(); PayoutAccountName = accountName.Trim(); }
}

public enum OperatorStatus { PendingVerification, Active, Rejected, Suspended }
public enum OperatorMemberRole { Owner, Dispatcher }
