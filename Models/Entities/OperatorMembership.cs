namespace BusGoSmartTicket.Models.Entities;

public sealed class OperatorMembership
{
    private OperatorMembership()
    {
    }
    public OperatorMembership(Guid operatorId, Guid userId, OperatorMemberRole role)
    {
        Id = Guid.NewGuid();
        OperatorId = operatorId;
        UserId = userId;
        Role = role;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }
    public Guid Id
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
    public Guid UserId
    {
        get; private set;
    }
    public OperatorMemberRole Role
    {
        get; private set;
    }
    public bool IsActive
    {
        get; private set;
    }
    public DateTime CreatedAtUtc
    {
        get; private set;
    }
    public void Deactivate() => IsActive = false;
}
