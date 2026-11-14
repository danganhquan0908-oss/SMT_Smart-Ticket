using BusGoSmartTicket.Models.DTOs.Account;
namespace BusGoSmartTicket.Services.Interfaces.Account;

public interface ICustomerAccountService
{
    Task<CustomerProfileDto?> GetProfileAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerBookingSummaryDto>> GetBookingsAsync(Guid userId, CancellationToken ct = default);
    Task<CustomerBookingSummaryDto?> GetBookingDetailsAsync(Guid userId, Guid bookingId, CancellationToken ct = default);
}
