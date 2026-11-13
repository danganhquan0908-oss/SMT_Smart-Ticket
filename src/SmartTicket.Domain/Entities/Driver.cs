namespace SmartTicket.Domain.Entities;

public sealed class Driver
{
    private Driver() { }
    public Driver(Guid operatorId, string fullName, string phone, string licenseNumber)
    { Id = Guid.NewGuid(); OperatorId = operatorId; FullName = fullName.Trim(); Phone = phone.Trim(); LicenseNumber = licenseNumber.Trim(); IsActive = true; CreatedAtUtc = DateTime.UtcNow; }
    public Guid Id { get; private set; }
    public Guid OperatorId { get; private set; }
    public BusOperator? Operator { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string LicenseNumber { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public void Update(string fullName, string phone, string licenseNumber)
    { FullName = fullName.Trim(); Phone = phone.Trim(); LicenseNumber = licenseNumber.Trim(); }
    public void Deactivate() => IsActive = false;
    public void Reactivate() => IsActive = true;
}
