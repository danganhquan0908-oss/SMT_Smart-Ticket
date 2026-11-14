namespace BusGoSmartTicket.Models.Entities;

public sealed class BookingSeat
{
    private BookingSeat()
    {
    }
    public BookingSeat(Guid bookingId, Guid seatId, string seatCode, decimal price)
    {
        Id = Guid.NewGuid();
        BookingId = bookingId;
        SeatId = seatId;
        SeatCode = seatCode;
        PriceAtPurchase = price;
    }
    public Guid Id
    {
        get; private set;
    }
    public Guid BookingId
    {
        get; private set;
    }
    public Booking? Booking
    {
        get; private set;
    }
    public Guid SeatId
    {
        get; private set;
    }
    public string SeatCode { get; private set; } = "";
    public decimal PriceAtPurchase
    {
        get; private set;
    }
}
