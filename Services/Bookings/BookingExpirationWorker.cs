using BusGoSmartTicket.Data;
using BusGoSmartTicket.Services.Interfaces.Bookings;
using Microsoft.EntityFrameworkCore;

namespace BusGoSmartTicket.Services.Bookings;

public sealed class BookingExpirationWorker(IServiceScopeFactory scopeFactory, ILogger<BookingExpirationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await ProcessExpiredBookingsAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Không thể giải phóng các lượt giữ ghế hết hạn.");
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Graceful shutdown khi ứng dụng dừng hoặc hủy token
        }
    }

    private async Task ProcessExpiredBookingsAsync(CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await using var tx = await db.Database.BeginTransactionAsync(stoppingToken);
        var now = DateTime.UtcNow;
        var expired = await db.Bookings.FromSqlInterpolated($"SELECT * FROM \"Bookings\" WHERE \"Status\" = 'Pending' AND \"ExpiresAtUtc\" <= {now} ORDER BY \"ExpiresAtUtc\" LIMIT 200 FOR UPDATE SKIP LOCKED").ToListAsync(stoppingToken);

        var releasedTripSeats = new List<(Guid TripId, List<string> SeatCodes)>();

        foreach (var booking in expired)
        {
            booking.Expire();
            var bookingSeats = await db.BookingSeats.Where(x => x.BookingId == booking.Id).ToListAsync(stoppingToken);
            var seatIds = bookingSeats.Select(x => x.SeatId).ToArray();
            var seats = seatIds.Length == 0 ? [] : await db.Seats.FromSqlInterpolated($"SELECT * FROM \"Seats\" WHERE \"Id\" = ANY ({seatIds}) ORDER BY \"Id\" FOR UPDATE").ToListAsync(stoppingToken);
            foreach (var seat in seats)
            {
                seat?.ReleaseHold(booking.Id);
            }
            releasedTripSeats.Add((booking.TripId, bookingSeats.Select(x => x.SeatCode).ToList()));
        }

        if (expired.Count > 0)
        {
            await db.SaveChangesAsync(stoppingToken);
            await tx.CommitAsync(stoppingToken);

            var notifier = scope.ServiceProvider.GetRequiredService<ISeatStatusNotifier>();
            foreach (var (tripId, seatCodes) in releasedTripSeats)
            {
                if (seatCodes.Count > 0)
                {
                    await notifier.NotifyAsync(tripId, seatCodes, "Available", stoppingToken);
                }
            }
        }
        else
        {
            await tx.CommitAsync(stoppingToken);
        }
    }
}
