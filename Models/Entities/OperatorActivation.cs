namespace BusGoSmartTicket.Models.Entities;

public sealed class OperatorActivation
{
    private OperatorActivation()
    {
    }
    public OperatorActivation(Guid userId, Guid operatorId, string tokenHash, DateTime expiresAtUtc)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        OperatorId = operatorId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = DateTime.UtcNow;
    }
    public Guid Id
    {
        get; private set;
    }
    public Guid UserId
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
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc
    {
        get; private set;
    }
    public DateTime CreatedAtUtc
    {
        get; private set;
    }
    public DateTime? UsedAtUtc
    {
        get; private set;
    }
    public void Consume() => UsedAtUtc = DateTime.UtcNow;
    public bool IsValid(DateTime nowUtc, string tokenHash) => UsedAtUtc is null && ExpiresAtUtc > nowUtc && TokenHash == tokenHash;
}
