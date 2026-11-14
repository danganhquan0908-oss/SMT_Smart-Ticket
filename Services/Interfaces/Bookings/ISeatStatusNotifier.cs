namespace BusGoSmartTicket.Services.Interfaces.Bookings;

public interface ISeatStatusNotifier
{
    Task NotifyAsync(Guid tripId, IReadOnlyList<string> seatCodes, string status, CancellationToken ct);
}
