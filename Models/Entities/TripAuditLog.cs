namespace BusGoSmartTicket.Models.Entities;

public sealed class TripAuditLog
{
    private TripAuditLog()
    {
    }
    public TripAuditLog(Guid tripId, Guid modifiedByUserId, decimal oldPrice, decimal newPrice, string action)
    {
        Id = Guid.NewGuid();
        TripId = tripId;
        ModifiedByUserId = modifiedByUserId;
        OldPrice = oldPrice;
        NewPrice = newPrice;
        Action = action;
        ChangedAtUtc = DateTime.UtcNow;
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
    public Guid ModifiedByUserId
    {
        get; private set;
    }
    public decimal OldPrice
    {
        get; private set;
    }
    public decimal NewPrice
    {
        get; private set;
    }
    public string Action { get; private set; } = string.Empty;
    public DateTime ChangedAtUtc
    {
        get; private set;
    }
}
