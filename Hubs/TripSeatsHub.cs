using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BusGoSmartTicket.Hubs;

[AllowAnonymous]
public sealed class TripSeatsHub : Hub
{
    public Task JoinTrip(Guid tripId) => Groups.AddToGroupAsync(Context.ConnectionId, Group(tripId));
    public static string Group(Guid tripId) => $"trip:{tripId:N}";
}
