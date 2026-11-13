using Microsoft.AspNetCore.SignalR;
using SmartTicket.Application.Bookings;

namespace SmartTicket.Web.Hubs;

public sealed class SignalRSeatStatusNotifier(IHubContext<TripSeatsHub> hub) : ISeatStatusNotifier
{
    public Task NotifyAsync(Guid tripId, IReadOnlyList<string> seatCodes, string status, CancellationToken ct) =>
        hub.Clients.Group(TripSeatsHub.Group(tripId)).SendAsync("SeatsChanged", new { seatCodes, status }, ct);
}
