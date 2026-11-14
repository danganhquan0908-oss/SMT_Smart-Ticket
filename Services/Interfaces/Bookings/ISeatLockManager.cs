namespace BusGoSmartTicket.Services.Interfaces.Bookings;

public interface ISeatLockManager
{
    Task<IAsyncDisposable?> TryAcquireLocksAsync(Guid tripId, IReadOnlyList<string> seatCodes, TimeSpan duration, CancellationToken ct);
}
