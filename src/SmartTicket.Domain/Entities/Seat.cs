namespace SmartTicket.Domain.Entities;

public sealed class Seat
{
    private Seat() { }
    public Seat(Guid tripId, string seatCode, int deck, int row, int column)
    { Id = Guid.NewGuid(); TripId = tripId; SeatCode = seatCode; Deck = deck; Row = row; Column = column; Status = SeatStatus.Available; }
    public Guid Id { get; private set; }
    public Guid TripId { get; private set; }
    public Trip? Trip { get; private set; }
    public string SeatCode { get; private set; } = "";
    public int Deck { get; private set; }
    public int Row { get; private set; }
    public int Column { get; private set; }
    public SeatStatus Status { get; private set; }
    public DateTime? LockedUntilUtc { get; private set; }
    public Guid? CurrentBookingId { get; private set; }
    public bool TryHold(Guid bookingId, DateTime nowUtc, TimeSpan duration)
    {
        if (Status == SeatStatus.Booked || Status == SeatStatus.Blocked || (Status == SeatStatus.Holding && LockedUntilUtc > nowUtc)) return false;
        Status = SeatStatus.Holding; CurrentBookingId = bookingId; LockedUntilUtc = nowUtc.Add(duration); return true;
    }
    public void ReleaseHold(Guid bookingId)
    { if (Status == SeatStatus.Holding && CurrentBookingId == bookingId) { Status = SeatStatus.Available; CurrentBookingId = null; LockedUntilUtc = null; } }
    public void ConfirmBooking(Guid bookingId)
    { if (Status != SeatStatus.Holding || CurrentBookingId != bookingId || LockedUntilUtc <= DateTime.UtcNow) throw new InvalidOperationException("Ghế không còn được giữ bởi booking này."); Status = SeatStatus.Booked; LockedUntilUtc = null; }
}

public enum SeatStatus { Available, Holding, Booked, Blocked }
