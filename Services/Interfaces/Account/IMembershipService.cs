using BusGoSmartTicket.Models.DTOs.Account;
namespace BusGoSmartTicket.Services.Interfaces.Account;

public interface IMembershipService
{
    Task<DiscountQuote> QuoteAsync(Guid? userId, decimal price, CancellationToken ct);
    Task<MembershipOverview> OverviewAsync(Guid userId, CancellationToken ct);
    Task<bool> MarkReadAsync(Guid userId, Guid notificationId, CancellationToken ct);
}
