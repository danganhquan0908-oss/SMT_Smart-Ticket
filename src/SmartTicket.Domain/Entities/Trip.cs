namespace SmartTicket.Domain.Entities;

public sealed class Trip
{
    private Trip() { }
    public Trip(Guid operatorId, Guid routeId, Guid vehicleId, Guid driverId, DateTime departureTimeUtc, DateTime arrivalTimeUtc, decimal ticketPrice)
    {
        if (departureTimeUtc >= arrivalTimeUtc) throw new ArgumentException("Giờ đến phải sau giờ khởi hành.");
        if (ticketPrice <= 0) throw new ArgumentOutOfRangeException(nameof(ticketPrice));
        Id = Guid.NewGuid(); OperatorId = operatorId; RouteId = routeId; VehicleId = vehicleId; DriverId = driverId;
        DepartureTimeUtc = departureTimeUtc; ArrivalTimeUtc = arrivalTimeUtc; TicketPrice = ticketPrice;
        Status = TripStatus.Scheduled; Version = 1;
    }
    public Guid Id { get; private set; }
    public Guid OperatorId { get; private set; }
    public BusOperator? Operator { get; private set; }
    public Guid RouteId { get; private set; }
    public BusRoute? Route { get; private set; }
    public Guid VehicleId { get; private set; }
    public Vehicle? Vehicle { get; private set; }
    public Guid DriverId { get; private set; }
    public Driver? Driver { get; private set; }
    public DateTime DepartureTimeUtc { get; private set; }
    public DateTime ArrivalTimeUtc { get; private set; }
    public decimal TicketPrice { get; private set; }
    public TripStatus Status { get; private set; }
    public long Version { get; private set; }
    public ICollection<TripTicket> Tickets { get; private set; } = new List<TripTicket>();
    public ICollection<Seat> Seats { get; private set; } = new List<Seat>();
    public void UpdateSchedule(Guid routeId, Guid vehicleId, Guid driverId, DateTime departureUtc, DateTime arrivalUtc, bool hasSoldTickets)
    {
        if (hasSoldTickets && (departureUtc != DepartureTimeUtc || arrivalUtc != ArrivalTimeUtc))
            throw new InvalidOperationException("Không thể đổi giờ chạy khi chuyến đã phát sinh vé đã bán.");
        if (departureUtc >= arrivalUtc) throw new ArgumentException("Giờ đến phải sau giờ khởi hành.");
        RouteId = routeId; VehicleId = vehicleId; DriverId = driverId; DepartureTimeUtc = departureUtc; ArrivalTimeUtc = arrivalUtc; Version++;
    }
    public void ChangePrice(decimal newPrice)
    { if (newPrice <= 0) throw new ArgumentOutOfRangeException(nameof(newPrice)); TicketPrice = newPrice; Version++; }
    public void Start() { if (Status != TripStatus.Scheduled) throw new InvalidOperationException("Chỉ chuyến đã lên lịch mới có thể xuất bến."); Status = TripStatus.Running; Version++; }
    public void Complete() { if (Status != TripStatus.Running) throw new InvalidOperationException("Chỉ chuyến đang chạy mới có thể hoàn tất."); Status = TripStatus.Completed; Version++; }
}

public enum TripStatus { Scheduled, Running, Completed, Cancelled }
