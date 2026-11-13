namespace SmartTicket.Domain.Entities;

public sealed class StationMembership
{
    private StationMembership() { }
    public StationMembership(Guid stationId, Guid userId)
    { Id = Guid.NewGuid(); StationId = stationId; UserId = userId; IsActive = true; CreatedAtUtc = DateTime.UtcNow; }
    public Guid Id { get; private set; }
    public Guid StationId { get; private set; }
    public Station? Station { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public void Deactivate() => IsActive = false;
    public void Reactivate() => IsActive = true;
}
