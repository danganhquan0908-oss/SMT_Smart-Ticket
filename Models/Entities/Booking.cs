using System.ComponentModel.DataAnnotations.Schema;
namespace BusGoSmartTicket.Models.Entities;

public sealed class Booking
{
    private Booking()
    {
    }
    public Booking(Guid tripId, string passengerName, string passengerPhone, string? passengerEmail, decimal totalAmount, Guid? customerUserId = null)
    {
        Id = Guid.NewGuid();
        BookingCode = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(8));
        TripId = tripId;
        PassengerName = passengerName.Trim();
        PassengerPhone = passengerPhone.Trim();
        PassengerEmail = passengerEmail?.Trim();
        BaseAmount = totalAmount;
        TotalAmount = totalAmount;
        Status = BookingStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
        ExpiresAtUtc = CreatedAtUtc.AddMinutes(7);
        LookupToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        LookupTokenHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(LookupToken)));
        CustomerUserId = customerUserId;
    }
    public Guid Id
    {
        get; private set;
    }
    public Guid? CustomerUserId
    {
        get; private set;
    }
    public string BookingCode { get; private set; } = "";
    public Guid TripId
    {
        get; private set;
    }
    public Trip? Trip
    {
        get; private set;
    }
    public string PassengerName { get; private set; } = "";
    public string PassengerPhone { get; private set; } = "";
    public string? PassengerEmail
    {
        get; private set;
    }
    public decimal TotalAmount
    {
        get; private set;
    }
    public decimal BaseAmount
    {
        get; private set;
    }
    public decimal DiscountAmount
    {
        get; private set;
    }
    public Guid? DiscountCampaignId
    {
        get; private set;
    }
    public string? DiscountName
    {
        get; private set;
    }
    public void ApplyDiscount(Guid campaignId, string name, decimal netAmount)
    {
        if (Status != BookingStatus.Pending || netAmount < 0 || netAmount > BaseAmount)
            throw new InvalidOperationException("Ưu đãi không hợp lệ cho lượt đặt vé này.");
        TotalAmount = netAmount;
        DiscountAmount = BaseAmount - netAmount;
        DiscountCampaignId = campaignId;
        DiscountName = name;
    }
    public BookingStatus Status
    {
        get; private set;
    }
    public DateTime ExpiresAtUtc
    {
        get; private set;
    }
    public DateTime CreatedAtUtc
    {
        get; private set;
    }
    [NotMapped] public string LookupToken { get; private set; } = "";
    public string LookupTokenHash { get; private set; } = "";
    public ICollection<BookingSeat> Seats { get; private set; } = new List<BookingSeat>();
    public void Expire()
    {
        if (Status == BookingStatus.Pending && ExpiresAtUtc <= DateTime.UtcNow)
            Status = BookingStatus.Expired;
    }
    public void AssignCustomer(Guid customerUserId)
    {
        CustomerUserId = customerUserId;
    }
    public void MarkPaid()
    {
        if (Status != BookingStatus.Pending)
            throw new InvalidOperationException("Chỉ booking đang chờ mới có thể thanh toán.");
        Status = BookingStatus.Paid;
    }
    public void MarkPaidAfterSeatRecovery()
    {
        if (Status != BookingStatus.Pending && Status != BookingStatus.Expired)
            throw new InvalidOperationException("Trạng thái booking không hợp lệ.");
        Status = BookingStatus.Paid;
    }
    public void MarkPaidOverdueUnassigned()
    {
        if (Status != BookingStatus.Pending && Status != BookingStatus.Expired)
            throw new InvalidOperationException("Trạng thái booking không hợp lệ cho thanh toán trễ.");
        Status = BookingStatus.PaidOverdueUnassigned;
    }
}
