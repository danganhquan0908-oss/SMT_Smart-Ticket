using BusGoSmartTicket.Models.DTOs.Payments;

namespace BusGoSmartTicket.Models.ViewModels;

public sealed class TicketLookupViewModel
{
    public required TicketLookupDto Booking
    {
        get; init;
    }
    public IReadOnlyDictionary<string, string> QrImages { get; init; } = new Dictionary<string, string>();
}
