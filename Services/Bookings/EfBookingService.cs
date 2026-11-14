using BusGoSmartTicket.Data;
using BusGoSmartTicket.Exceptions.Bookings;
using BusGoSmartTicket.Models.DTOs.Bookings;
using BusGoSmartTicket.Models.Entities;
using BusGoSmartTicket.Services.Interfaces.Bookings;
using Microsoft.EntityFrameworkCore;

namespace BusGoSmartTicket.Services.Bookings;

public sealed class EfBookingService(
    ApplicationDbContext db,
    ISeatLockManager lockManager,
    BusGoSmartTicket.Services.Interfaces.Account.IMembershipService? membership = null) : IBookingService
{
    private static readonly TimeSpan HoldDuration = TimeSpan.FromMinutes(7);

    public async Task<IReadOnlyList<TripSearchResult>> SearchAsync(TripSearchQuery query, CancellationToken ct)
    {
        if (query.DepartureStationId == query.ArrivalStationId)
            throw new BookingConflictException("Bến đi và bến đến phải khác nhau.");
        var startUtc = new DateTimeOffset(query.TravelDate.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(7)).UtcDateTime;
        var endUtc = startUtc.AddDays(1);
        var trips = await db.Trips.AsNoTracking()
            .Where(t => t.Route!.DepartureStationId == query.DepartureStationId && t.Route.ArrivalStationId == query.ArrivalStationId &&
                t.Status == TripStatus.Scheduled && t.DepartureTimeUtc >= startUtc && t.DepartureTimeUtc < endUtc)
            .OrderBy(t => t.DepartureTimeUtc).ThenBy(t => t.TicketPrice)
            .Select(t => new TripSearchResult(t.Id, t.RouteId, t.Operator!.CompanyName, t.Route!.DepartureStation!.Name,
                t.Route.ArrivalStation!.Name, t.Route.DepartureStation!.Province, t.Route.ArrivalStation!.Province,
                t.DepartureTimeUtc, t.ArrivalTimeUtc, t.TicketPrice, t.Vehicle!.VehicleType, 0, 0))
            .ToListAsync(ct);

        if (trips.Count == 0)
            return trips;

        var tripIds = trips.Select(x => x.TripId).ToArray();
        var seatCounts = await db.Seats.AsNoTracking()
            .Where(s => tripIds.Contains(s.TripId))
            .GroupBy(s => s.TripId)
            .Select(group => new
            {
                TripId = group.Key,
                Total = group.Count(),
                Available = group.Count(s => s.Status == SeatStatus.Available ||
                    (s.Status == SeatStatus.Holding && s.LockedUntilUtc <= DateTime.UtcNow))
            })
            .ToDictionaryAsync(x => x.TripId, ct);

        return trips.Select(trip => seatCounts.TryGetValue(trip.TripId, out var counts)
            ? trip with
            {
                AvailableSeats = counts.Available,
                TotalSeats = counts.Total
            }
            : trip).ToList();
    }

    public async Task<IReadOnlyList<SeatDto>> GetSeatsAsync(Guid tripId, CancellationToken ct)
    {
        if (!await db.Trips.AnyAsync(x => x.Id == tripId, ct))
            throw new BookingNotFoundException("Không tìm thấy chuyến xe.");
        var now = DateTime.UtcNow;
        return await db.Seats.AsNoTracking().Where(x => x.TripId == tripId).OrderBy(x => x.Deck).ThenBy(x => x.Row).ThenBy(x => x.Column)
            .Select(x => new SeatDto(x.Id, x.SeatCode, x.Deck, x.Row, x.Column,
                x.Status == SeatStatus.Holding && x.LockedUntilUtc <= now ? nameof(SeatStatus.Available) : x.Status.ToString(),
                x.Status == SeatStatus.Holding && x.LockedUntilUtc > now ? x.LockedUntilUtc : null)).ToListAsync(ct);
    }

    public async Task<BookingHoldResult> HoldSeatsAsync(Guid tripId, HoldSeatsRequest request, CancellationToken ct)
    {
        if (request.SeatCodes is null)
            throw new BookingConflictException("Chọn ít nhất một ghế.");
        var codes = request.SeatCodes.Select(x => x.Trim().ToUpperInvariant()).Distinct().Order().ToArray();
        if (codes.Length == 0 || codes.Length != request.SeatCodes.Count || codes.Length > 8)
            throw new BookingConflictException("Chọn từ 1 đến 8 ghế khác nhau.");
        if (string.IsNullOrWhiteSpace(request.PassengerName) || string.IsNullOrWhiteSpace(request.PassengerPhone))
            throw new BookingConflictException("Cần nhập tên và số điện thoại hành khách.");

        // Lớp 1 - Fast Fail tại In-Memory / Redis Distributed Lock (RedLock)
        await using var distributedLock = await lockManager.TryAcquireLocksAsync(tripId, codes, TimeSpan.FromSeconds(3), ct);
        if (distributedLock is null)
            throw new BookingConflictException("Một hoặc nhiều ghế đang được xử lý giữ chỗ bởi người khác. Vui lòng thử lại sau giây lát.");

        // Lớp 2 - DB Transaction với Khóa bi quan (Pessimistic Locking)
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var trip = await db.Trips.SingleOrDefaultAsync(x => x.Id == tripId && x.Status == TripStatus.Scheduled, ct)
            ?? throw new BookingNotFoundException("Không tìm thấy chuyến xe.");

        var now = DateTime.UtcNow;
        if (trip.DepartureTimeUtc <= now)
            throw new BookingConflictException("Chuyến xe đã khởi hành hoặc không còn mở bán.");
        var seats = await db.Seats.FromSqlInterpolated($"SELECT * FROM \"Seats\" WHERE \"TripId\" = {tripId} AND \"SeatCode\" = ANY ({codes}) ORDER BY \"SeatCode\" FOR UPDATE")
            .ToListAsync(ct);
        if (seats.Count != codes.Length)
            throw new BookingConflictException("Một hoặc nhiều ghế không tồn tại trên chuyến này.");
        foreach (var seat in seats)
        {
            if (!seat.TryHold(Guid.Empty, now, HoldDuration))
                throw new BookingConflictException($"Ghế {seat.SeatCode} vừa được người khác giữ hoặc đã bán.");
        }
        var booking = new Booking(tripId, request.PassengerName, request.PassengerPhone, request.PassengerEmail, trip.TicketPrice * seats.Count, request.CustomerUserId);
        var quote = membership is null ? null : await membership.QuoteAsync(request.CustomerUserId, trip.TicketPrice, ct);
        var netSeatPrice = quote?.NetSeatPrice ?? trip.TicketPrice;
        if (request.ExpectedUnitPrice.HasValue && request.ExpectedUnitPrice.Value != netSeatPrice)
            throw new BookingConflictException("Giá hoặc ưu đãi vừa thay đổi. Vui lòng tải lại trang để xem và xác nhận giá mới.");
        if (quote?.CampaignId is Guid campaignId)
            booking.ApplyDiscount(campaignId, quote.Name!, netSeatPrice * seats.Count);
        foreach (var seat in seats)
        {
            seat.ReleaseHold(Guid.Empty);
            if (!seat.TryHold(booking.Id, now, HoldDuration))
                throw new BookingConflictException($"Không thể giữ ghế {seat.SeatCode}.");
            booking.Seats.Add(new BookingSeat(booking.Id, seat.Id, seat.SeatCode, netSeatPrice));
        }
        db.Bookings.Add(booking);
        var paymentRef = "STKT" + booking.BookingCode;
        db.Payments.Add(new Payment(booking.Id, paymentRef, booking.TotalAmount));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return new BookingHoldResult(booking.Id, booking.BookingCode, booking.ExpiresAtUtc, booking.TotalAmount, codes, booking.LookupToken);
    }
}
