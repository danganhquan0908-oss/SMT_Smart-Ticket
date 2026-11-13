using System.ComponentModel.DataAnnotations.Schema;
namespace SmartTicket.Domain.Entities;

public sealed class Booking
{
    private Booking() { }
    public Booking(Guid tripId, string passengerName, string passengerPhone, string? passengerEmail, decimal totalAmount, Guid? customerUserId = null)
    { Id = Guid.NewGuid(); BookingCode = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(8)); TripId = tripId; PassengerName = passengerName.Trim(); PassengerPhone = passengerPhone.Trim(); PassengerEmail = passengerEmail?.Trim(); TotalAmount = totalAmount; Status = BookingStatus.Pending; CreatedAtUtc = DateTime.UtcNow; ExpiresAtUtc = CreatedAtUtc.AddMinutes(7); LookupToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+','-').Replace('/','_'); LookupTokenHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(LookupToken))); CustomerUserId = customerUserId; }
    public Guid Id { get; private set; }
    public Guid? CustomerUserId { get; private set; }
    public string BookingCode { get; private set; } = "";
    public Guid TripId { get; private set; }
    public Trip? Trip { get; private set; }
    public string PassengerName { get; private set; } = "";
    public string PassengerPhone { get; private set; } = "";
    public string? PassengerEmail { get; private set; }
    public decimal TotalAmount { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    [NotMapped] public string LookupToken { get; private set; } = "";
    public string LookupTokenHash { get; private set; } = "";
    public ICollection<BookingSeat> Seats { get; private set; } = new List<BookingSeat>();
    public void Expire() { if (Status == BookingStatus.Pending && ExpiresAtUtc <= DateTime.UtcNow) Status = BookingStatus.Expired; }
    public void AssignCustomer(Guid customerUserId) { CustomerUserId = customerUserId; }
    public void MarkPaid() { if (Status != BookingStatus.Pending) throw new InvalidOperationException("Chỉ booking đang chờ mới có thể thanh toán."); Status = BookingStatus.Paid; }
    public void MarkPaidAfterSeatRecovery() { if (Status != BookingStatus.Pending && Status != BookingStatus.Expired) throw new InvalidOperationException("Trạng thái booking không hợp lệ."); Status = BookingStatus.Paid; }
    public void MarkPaidOverdueUnassigned() { if (Status != BookingStatus.Pending && Status != BookingStatus.Expired) throw new InvalidOperationException("Trạng thái booking không hợp lệ cho thanh toán trễ."); Status = BookingStatus.PaidOverdueUnassigned; }
}
public enum BookingStatus { Pending, Paid, Expired, Cancelled, PaidOverdueUnassigned }

public sealed class BookingSeat
{
    private BookingSeat() { }
    public BookingSeat(Guid bookingId, Guid seatId, string seatCode, decimal price)
    { Id = Guid.NewGuid(); BookingId = bookingId; SeatId = seatId; SeatCode = seatCode; PriceAtPurchase = price; }
    public Guid Id { get; private set; }
    public Guid BookingId { get; private set; }
    public Booking? Booking { get; private set; }
    public Guid SeatId { get; private set; }
    public string SeatCode { get; private set; } = "";
    public decimal PriceAtPurchase { get; private set; }
}
