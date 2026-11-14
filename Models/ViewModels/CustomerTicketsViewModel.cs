using BusGoSmartTicket.Models.DTOs.Account;

namespace BusGoSmartTicket.Models.ViewModels;

public sealed class CustomerTicketsViewModel
{
    public IReadOnlyList<CustomerBookingSummaryDto> Upcoming { get; init; } = [];
    public IReadOnlyList<CustomerBookingSummaryDto> Pending { get; init; } = [];
    public IReadOnlyList<CustomerBookingSummaryDto> History { get; init; } = [];
}
