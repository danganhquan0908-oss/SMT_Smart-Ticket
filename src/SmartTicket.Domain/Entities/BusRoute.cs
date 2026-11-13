namespace SmartTicket.Domain.Entities;

public sealed class BusRoute
{
    private BusRoute() { }

    public BusRoute(Guid departureStationId, Guid arrivalStationId, decimal distanceKm, int estimatedHours)
    {
        if (departureStationId == arrivalStationId)
            throw new ArgumentException("Bến đi và bến đến phải khác nhau.");
        if (distanceKm <= 0 || estimatedHours <= 0)
            throw new ArgumentOutOfRangeException(nameof(distanceKm), "Khoảng cách và thời lượng phải lớn hơn 0.");

        Id = Guid.NewGuid();
        DepartureStationId = departureStationId;
        ArrivalStationId = arrivalStationId;
        DistanceKm = distanceKm;
        EstimatedHours = estimatedHours;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid DepartureStationId { get; private set; }
    public Station? DepartureStation { get; private set; }
    public Guid ArrivalStationId { get; private set; }
    public Station? ArrivalStation { get; private set; }
    public decimal DistanceKm { get; private set; }
    public int EstimatedHours { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public void Update(Guid departureStationId, Guid arrivalStationId, decimal distanceKm, int estimatedHours)
    {
        if (departureStationId == arrivalStationId)
            throw new ArgumentException("Bến đi và bến đến phải khác nhau.");
        if (distanceKm <= 0 || estimatedHours <= 0)
            throw new ArgumentOutOfRangeException(nameof(distanceKm), "Khoảng cách và thời lượng phải lớn hơn 0.");

        DepartureStationId = departureStationId;
        ArrivalStationId = arrivalStationId;
        DistanceKm = distanceKm;
        EstimatedHours = estimatedHours;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Deactivate() { IsActive = false; UpdatedAtUtc = DateTime.UtcNow; }
    public void Reactivate() { IsActive = true; UpdatedAtUtc = DateTime.UtcNow; }
}
