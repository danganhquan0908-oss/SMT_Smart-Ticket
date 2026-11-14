using BusGoSmartTicket.Models.DTOs.Bookings;
namespace BusGoSmartTicket.Services.Interfaces.Bookings;

public interface IBookingService
{
    Task<IReadOnlyList<TripSearchResult>> SearchAsync(TripSearchQuery query, CancellationToken ct);
    Task<IReadOnlyList<SeatDto>> GetSeatsAsync(Guid tripId, CancellationToken ct);
    Task<BookingHoldResult> HoldSeatsAsync(Guid tripId, HoldSeatsRequest request, CancellationToken ct);
}
