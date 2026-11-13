namespace SmartTicket.Application.Bookings;

public sealed record TripSearchQuery(Guid DepartureStationId, Guid ArrivalStationId, DateOnly TravelDate);
public sealed record TripSearchResult(Guid TripId, Guid RouteId, string Operator, string DepartureStation, string ArrivalStation, string DepartureProvince, string ArrivalProvince, DateTime DepartureTimeUtc, DateTime ArrivalTimeUtc, decimal Price, string VehicleType, int AvailableSeats, int TotalSeats);
public sealed record SeatDto(Guid Id, string Code, int Deck, int Row, int Column, string Status, DateTime? LockedUntilUtc);
public sealed record HoldSeatsRequest(string PassengerName, string PassengerPhone, string? PassengerEmail, IReadOnlyList<string> SeatCodes, Guid? CustomerUserId = null);
public sealed record BookingHoldResult(Guid BookingId, string BookingCode, DateTime ExpiresAtUtc, decimal TotalAmount, IReadOnlyList<string> SeatCodes, string LookupToken);
public interface IBookingService
{
    Task<IReadOnlyList<TripSearchResult>> SearchAsync(TripSearchQuery query, CancellationToken ct);
    Task<IReadOnlyList<SeatDto>> GetSeatsAsync(Guid tripId, CancellationToken ct);
    Task<BookingHoldResult> HoldSeatsAsync(Guid tripId, HoldSeatsRequest request, CancellationToken ct);
}
public interface ISeatStatusNotifier
{
    Task NotifyAsync(Guid tripId, IReadOnlyList<string> seatCodes, string status, CancellationToken ct);
}

public interface ISeatLockManager
{
    Task<IAsyncDisposable?> TryAcquireLocksAsync(Guid tripId, IReadOnlyList<string> seatCodes, TimeSpan duration, CancellationToken ct);
}
public sealed class BookingConflictException(string message) : Exception(message);
public sealed class BookingNotFoundException(string message) : Exception(message);
