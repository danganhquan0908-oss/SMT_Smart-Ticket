namespace SmartTicket.Application.Account;

public sealed record CustomerProfileDto(Guid Id, string FullName, string Email, string? PhoneNumber);

public sealed record UpdateProfileCommand(string FullName, string? PhoneNumber);

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword, string ConfirmPassword);

public sealed record CustomerTicketDto(
    Guid TicketId,
    string TicketNumber,
    string SeatCode,
    decimal Price,
    DateTime IssuedAtUtc,
    string Status,
    string CheckInJwt);

public sealed record CustomerBookingSummaryDto(
    Guid BookingId,
    string BookingCode,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAtUtc,
    DateTime ExpiresAtUtc,
    string DepartureStation,
    string ArrivalStation,
    string OperatorName,
    string VehicleType,
    DateTime DepartureTimeUtc,
    IReadOnlyList<string> SeatCodes,
    IReadOnlyList<CustomerTicketDto> Tickets,
    string? LookupToken = null);

public sealed record CustomerPaymentDetailsDto(
    Guid BookingId,
    string BookingCode,
    decimal Amount,
    string Reference,
    string BankBin,
    string BankName,
    string AccountNumber,
    string AccountName,
    string? QrPayload,
    string? QrImage,
    string VietQrUrl,
    int SecondsRemaining,
    string Status);

public sealed record CustomerPaymentStateDto(
    string Status,
    bool IsPaid,
    bool IsExpired,
    DateTime? CompletedAtUtc);

public interface ICustomerAccountService
{
    Task<CustomerProfileDto?> GetProfileAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerBookingSummaryDto>> GetBookingsAsync(Guid userId, CancellationToken ct = default);
    Task<CustomerBookingSummaryDto?> GetBookingDetailsAsync(Guid userId, Guid bookingId, CancellationToken ct = default);
    Task<CustomerPaymentDetailsDto?> GetPaymentDetailsAsync(Guid userId, Guid bookingId, CancellationToken ct = default);
    Task<CustomerPaymentStateDto?> GetPaymentStateAsync(Guid userId, Guid bookingId, CancellationToken ct = default);
}
