using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SmartTicket.Web.Hubs;

[AllowAnonymous]
public sealed class TripSeatsHub : Hub
{
    public Task JoinTrip(Guid tripId) => Groups.AddToGroupAsync(Context.ConnectionId, Group(tripId));
    public static string Group(Guid tripId) => $"trip:{tripId:N}";

    public Task JoinBooking(Guid bookingId) => Groups.AddToGroupAsync(Context.ConnectionId, GroupForBooking(bookingId));
    public Task LeaveBooking(Guid bookingId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupForBooking(bookingId));
    public static string GroupForBooking(Guid bookingId) => $"booking:{bookingId:N}";
}
