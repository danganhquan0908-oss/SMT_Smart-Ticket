using BusGoSmartTicket.Hubs;
using BusGoSmartTicket.Services.Interfaces.Bookings;
using Microsoft.AspNetCore.SignalR;

namespace BusGoSmartTicket.Services.Bookings;

public sealed class SignalRSeatStatusNotifier(IHubContext<TripSeatsHub> hub) : ISeatStatusNotifier
{
    public Task NotifyAsync(Guid tripId, IReadOnlyList<string> seatCodes, string status, CancellationToken ct) =>
        hub.Clients.Group(TripSeatsHub.Group(tripId)).SendAsync("SeatsChanged", new
        {
            seatCodes,
            status
        }, ct);
}
