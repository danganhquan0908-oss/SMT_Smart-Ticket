using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BusGoSmartTicket.Data;
using BusGoSmartTicket.Exceptions.Payments;
using BusGoSmartTicket.Models.DTOs.Payments;
using BusGoSmartTicket.Models.Entities;
using BusGoSmartTicket.Services.Interfaces.Payments;
using Microsoft.EntityFrameworkCore;

namespace BusGoSmartTicket.Services.Payments;

public sealed class EfPaymentService(ApplicationDbContext db, IConfiguration configuration) : IPaymentService
{
    public async Task<PaymentCheckoutDto> CreateCheckoutAsync(string lookupToken, CancellationToken ct)
    {
        var bookingMatch = await FindBookingAsync(lookupToken, ct) ?? throw new PaymentNotFoundException("Không tìm thấy lượt đặt vé.");
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var booking = await db.Bookings.FromSqlInterpolated($"SELECT * FROM \"Bookings\" WHERE \"Id\" = {bookingMatch.Id} FOR UPDATE").SingleAsync(ct);
        if (booking.Status != BookingStatus.Pending || booking.ExpiresAtUtc <= DateTime.UtcNow)
            throw new PaymentConflictException("Thời gian giữ ghế đã hết hoặc booking không còn chờ thanh toán.");
        var reference = "STKT" + booking.BookingCode;
        var payment = await db.Payments.SingleOrDefaultAsync(x => x.BookingId == booking.Id, ct);
        if (payment is null)
        {
            payment = new Payment(booking.Id, reference, booking.TotalAmount);
            db.Payments.Add(payment);
            await db.SaveChangesAsync(ct);
        }
        await transaction.CommitAsync(ct);
        if (payment.Status != PaymentStatus.Pending)
            throw new PaymentConflictException("Thanh toán này đã được xử lý.");
        var bin = configuration["Payments:BankBin"] ?? "";
        var account = configuration["Payments:AccountNumber"] ?? "";
        var payload = string.IsNullOrWhiteSpace(bin) || string.IsNullOrWhiteSpace(account) ? "" :
            NapasQrPayloadBuilder.Build(bin, account, configuration["Payments:AccountName"] ?? "", booking.TotalAmount, payment.Reference);
        return new PaymentCheckoutDto(booking.Id, booking.BookingCode, booking.TotalAmount, booking.ExpiresAtUtc, payload, lookupToken);
    }

    public async Task<PaymentStateDto?> GetPaymentStateAsync(string lookupToken, CancellationToken ct)
    {
        var booking = await FindBookingAsync(lookupToken, ct);
        if (booking is null)
            return null;
        var payment = await db.Payments.SingleOrDefaultAsync(x => x.BookingId == booking.Id, ct);
        var paid = payment?.Status is PaymentStatus.Succeeded or PaymentStatus.OverdueUnassigned;
        var lookupUrl = paid ? "/tickets/lookup?token=" + Uri.EscapeDataString(lookupToken) : null;
        return new PaymentStateDto(payment?.Status.ToString() ?? "Pending", payment?.CompletedAtUtc, lookupUrl);
    }

    public async Task<TicketLookupDto?> GetTicketsAsync(string lookupToken, CancellationToken ct)
    {
        var booking = await FindBookingAsync(lookupToken, ct);
        if (booking is null)
            return null;
        var items = await db.TripTickets.Where(x => x.BookingId == booking.Id).Include(x => x.Trip).ThenInclude(x => x!.Route).ThenInclude(x => x!.DepartureStation)
            .Include(x => x.Trip).ThenInclude(x => x!.Route).ThenInclude(x => x!.ArrivalStation)
            .OrderBy(x => x.SeatCode).ToListAsync(ct);
        return new TicketLookupDto(booking.BookingCode, booking.Status.ToString(), items.Select(x => new TicketDto(
            x.Id.ToString("N")[..12].ToUpperInvariant(), booking.PassengerName,
            $"{x.Trip!.Route!.DepartureStation!.Name} → {x.Trip.Route.ArrivalStation!.Name}", x.SeatCode,
            x.Trip.DepartureTimeUtc, x.PriceAtPurchase, x.CheckInToken)).ToList());
    }

    public async Task<WebhookProcessResult> ProcessWebhookAsync(PaymentWebhookPayload payload, string rawPayload, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(payload.Reference) || string.IsNullOrWhiteSpace(payload.TransactionId) || payload.TransactionId.Length > 120)
            throw new PaymentConflictException("Thiếu mã tham chiếu hoặc mã giao dịch hợp lệ.");
        if (await db.Payments.AnyAsync(x => x.ProviderTransactionId == payload.TransactionId, ct))
            return new WebhookProcessResult(false, "duplicate_transaction");

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var payment = await db.Payments.FromSqlInterpolated($"SELECT * FROM \"Payments\" WHERE \"Reference\" = {payload.Reference} FOR UPDATE").SingleOrDefaultAsync(ct);
        if (payment is null)
        {
            var bookingCode = payload.Reference.StartsWith("STKT", StringComparison.OrdinalIgnoreCase)
                ? payload.Reference[4..]
                : payload.Reference;
            var bookingRecord = await db.Bookings.SingleOrDefaultAsync(x => x.BookingCode == bookingCode, ct);
            if (bookingRecord is not null)
            {
                payment = new Payment(bookingRecord.Id, payload.Reference, bookingRecord.TotalAmount);
                db.Payments.Add(payment);
                await db.SaveChangesAsync(ct);
            }
            else
            {
                throw new PaymentNotFoundException("Không tìm thấy khoản thanh toán.");
            }
        }
        if (payment.Status != PaymentStatus.Pending)
        {
            await tx.CommitAsync(ct);
            return new WebhookProcessResult(false, "already_processed");
        }
        if (payload.Amount != payment.Amount)
            throw new PaymentConflictException("Số tiền webhook không khớp số tiền booking.");
        if (!string.Equals(payload.Status, "SUCCESS", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(payload.Status, "PAID", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(payload.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase))
        {
            payment.MarkFailed(payload.TransactionId, rawPayload);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new WebhookProcessResult(true, "payment_failed");
        }

        var booking = await db.Bookings.FromSqlInterpolated($"SELECT * FROM \"Bookings\" WHERE \"Id\" = {payment.BookingId} FOR UPDATE").SingleAsync(ct);
        var trip = await db.Trips.SingleAsync(x => x.Id == booking.TripId, ct);
        if (booking.Status is not (BookingStatus.Pending or BookingStatus.Expired))
            throw new PaymentConflictException("Booking không ở trạng thái có thể thanh toán.");
        var bookingSeats = await db.BookingSeats.Where(x => x.BookingId == booking.Id).OrderBy(x => x.SeatCode).ToListAsync(ct);
        var seatIds = bookingSeats.Select(x => x.SeatId).ToArray();
        var seats = await db.Seats.FromSqlInterpolated($"SELECT * FROM \"Seats\" WHERE \"Id\" = ANY ({seatIds}) ORDER BY \"Id\" FOR UPDATE").ToListAsync(ct);
        var now = DateTime.UtcNow;
        var canAssign = trip.DepartureTimeUtc > now && seats.Count == bookingSeats.Count && seats.All(s =>
            s.Status == SeatStatus.Available ||
            (s.Status == SeatStatus.Holding && s.LockedUntilUtc <= now) ||
            (s.Status == SeatStatus.Holding && s.CurrentBookingId == booking.Id && s.LockedUntilUtc > now));
        if (!canAssign)
        {
            booking.MarkPaidOverdueUnassigned();
            payment.MarkOverdue(payload.TransactionId, rawPayload);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new WebhookProcessResult(true, "paid_overdue_unassigned");
        }

        foreach (var seat in seats)
        {
            if (!(seat.Status == SeatStatus.Holding && seat.CurrentBookingId == booking.Id && seat.LockedUntilUtc > now))
            {
                if (!seat.TryHold(booking.Id, now, TimeSpan.FromMinutes(1)))
                    throw new PaymentConflictException("Ghế vừa được giữ bởi giao dịch khác.");
            }
            seat.ConfirmBooking(booking.Id);
        }
        booking.MarkPaidAfterSeatRecovery();
        foreach (var item in bookingSeats)
            db.TripTickets.Add(new TripTicket(booking.TripId, booking.Id, item.SeatId, item.SeatCode, item.PriceAtPurchase, CreateCheckInJwt(booking, item, trip)));
        payment.MarkSucceeded(payload.TransactionId, rawPayload);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return new WebhookProcessResult(true, "paid_and_tickets_issued", booking.TripId, bookingSeats.Select(x => x.SeatCode).ToArray());
    }

    private Task<Booking?> FindBookingAsync(string token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Task.FromResult<Booking?>(null);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        return db.Bookings.AsNoTracking().SingleOrDefaultAsync(x => x.LookupTokenHash == hash, ct);
    }

    private string CreateCheckInJwt(Booking booking, BookingSeat seat, Trip trip)
    {
        var configuredKey = configuration["Tickets:SigningKey"];
        if (string.IsNullOrWhiteSpace(configuredKey) || Encoding.UTF8.GetByteCount(configuredKey) < 32)
            throw new InvalidOperationException("Cần cấu hình Tickets:SigningKey tối thiểu 32 byte trước khi phát hành vé.");
        static string B64(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var header = B64(Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));
        var expiresUtc = trip.DepartureTimeUtc.AddDays(1) > DateTime.UtcNow.AddDays(2) ? trip.DepartureTimeUtc.AddDays(1) : DateTime.UtcNow.AddDays(2);
        var claims = JsonSerializer.SerializeToUtf8Bytes(new
        {
            sub = seat.Id.ToString("N"),
            booking = booking.BookingCode,
            trip = booking.TripId,
            seat = seat.SeatCode,
            exp = new DateTimeOffset(expiresUtc, TimeSpan.Zero).ToUnixTimeSeconds()
        });
        var body = header + "." + B64(claims);
        var signature = HMACSHA256.HashData(Encoding.UTF8.GetBytes(configuredKey), Encoding.ASCII.GetBytes(body));
        return body + "." + B64(signature);
    }
}
