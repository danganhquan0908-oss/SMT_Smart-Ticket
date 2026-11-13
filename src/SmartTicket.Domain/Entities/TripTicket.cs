namespace SmartTicket.Domain.Entities;

public sealed class TripTicket
{
    private TripTicket() { }
    public TripTicket(Guid tripId, Guid bookingId, Guid seatId, string seatCode, decimal priceAtPurchase, string checkInToken)
    { Id = Guid.NewGuid(); TripId = tripId; BookingId = bookingId; SeatId = seatId; SeatCode = seatCode; PriceAtPurchase = priceAtPurchase; CheckInToken = checkInToken; Status = TicketLifecycleStatus.Sold; IssuedAtUtc = DateTime.UtcNow; }
    public Guid Id { get; private set; }
    public Guid TripId { get; private set; }
    public Trip? Trip { get; private set; }
    public Guid BookingId { get; private set; }
    public Guid SeatId { get; private set; }
    public string SeatCode { get; private set; } = "";
    public decimal PriceAtPurchase { get; private set; }
    public string CheckInToken { get; private set; } = "";
    public TicketLifecycleStatus Status { get; private set; }
    public DateTime IssuedAtUtc { get; private set; }
    public DateTime? CheckedInAtUtc { get; private set; }
    public Guid? CheckedInByUserId { get; private set; }
    public void CheckIn(Guid userId, DateTime atUtc)
    { if (Status != TicketLifecycleStatus.Sold) throw new InvalidOperationException("Vé đã được sử dụng hoặc đã bị hủy."); Status = TicketLifecycleStatus.CheckedIn; CheckedInByUserId = userId; CheckedInAtUtc = atUtc; }
}

public enum TicketLifecycleStatus { Pending, Sold, Cancelled, CheckedIn }
