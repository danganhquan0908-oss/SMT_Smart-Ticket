using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartTicket.Application.Account;
using SmartTicket.Domain.Entities;
using SmartTicket.Infrastructure.Payments;
using SmartTicket.Infrastructure.Persistence;

namespace SmartTicket.Infrastructure.Account;

public sealed class EfCustomerAccountService(SmartTicketDbContext db, IConfiguration? configuration = null) : ICustomerAccountService
{
    public async Task<CustomerProfileDto?> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == userId, ct);
        if (user is null) return null;

        return new CustomerProfileDto(
            user.Id,
            user.FullName ?? user.UserName ?? "",
            user.Email ?? "",
            user.PhoneNumber);
    }

    public async Task<IReadOnlyList<CustomerBookingSummaryDto>> GetBookingsAsync(Guid userId, CancellationToken ct = default)
    {
        var bookings = await db.Bookings.AsNoTracking()
            .Where(b => b.CustomerUserId == userId)
            .Include(b => b.Trip).ThenInclude(t => t!.Route).ThenInclude(r => r!.DepartureStation)
            .Include(b => b.Trip).ThenInclude(t => t!.Route).ThenInclude(r => r!.ArrivalStation)
            .Include(b => b.Trip).ThenInclude(t => t!.Operator)
            .Include(b => b.Trip).ThenInclude(t => t!.Vehicle)
            .Include(b => b.Seats)
            .OrderByDescending(b => b.CreatedAtUtc)
            .ToListAsync(ct);

        if (bookings.Count == 0) return [];

        var bookingIds = bookings.Select(x => x.Id).ToArray();
        var tickets = await db.TripTickets.AsNoTracking()
            .Where(t => bookingIds.Contains(t.BookingId))
            .OrderBy(t => t.SeatCode)
            .ToListAsync(ct);

        var ticketsByBooking = tickets.GroupBy(t => t.BookingId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<CustomerTicketDto>)g.Select(t => new CustomerTicketDto(
                    t.Id,
                    t.Id.ToString("N")[..12].ToUpperInvariant(),
                    t.SeatCode,
                    t.PriceAtPurchase,
                    t.IssuedAtUtc,
                    t.Status.ToString(),
                    t.CheckInToken)).ToList());

        return bookings.Select(b =>
        {
            ticketsByBooking.TryGetValue(b.Id, out var ticketList);
            return new CustomerBookingSummaryDto(
                b.Id,
                b.BookingCode,
                b.Status.ToString(),
                b.TotalAmount,
                b.CreatedAtUtc,
                b.ExpiresAtUtc,
                b.Trip?.Route?.DepartureStation?.Name ?? "Bến đi",
                b.Trip?.Route?.ArrivalStation?.Name ?? "Bến đến",
                b.Trip?.Operator?.CompanyName ?? "Smart Ticket Express",
                b.Trip?.Vehicle?.VehicleType ?? "Xe khách chất lượng cao",
                b.Trip?.DepartureTimeUtc ?? DateTime.UtcNow,
                b.Seats.Select(s => s.SeatCode).OrderBy(x => x).ToList(),
                ticketList ?? []);
        }).ToList();
    }

    public async Task<CustomerBookingSummaryDto?> GetBookingDetailsAsync(Guid userId, Guid bookingId, CancellationToken ct = default)
    {
        var b = await db.Bookings.AsNoTracking()
            .Where(x => x.Id == bookingId && x.CustomerUserId == userId)
            .Include(x => x.Trip).ThenInclude(t => t!.Route).ThenInclude(r => r!.DepartureStation)
            .Include(x => x.Trip).ThenInclude(t => t!.Route).ThenInclude(r => r!.ArrivalStation)
            .Include(x => x.Trip).ThenInclude(t => t!.Operator)
            .Include(x => x.Trip).ThenInclude(t => t!.Vehicle)
            .Include(x => x.Seats)
            .SingleOrDefaultAsync(ct);

        if (b is null) return null;

        var tickets = (await db.TripTickets.AsNoTracking()
            .Where(t => t.BookingId == bookingId)
            .OrderBy(t => t.SeatCode)
            .ToListAsync(ct))
            .Select(t => new CustomerTicketDto(
                t.Id,
                t.Id.ToString("N")[..12].ToUpperInvariant(),
                t.SeatCode,
                t.PriceAtPurchase,
                t.IssuedAtUtc,
                t.Status.ToString(),
                t.CheckInToken))
            .ToList();

        return new CustomerBookingSummaryDto(
            b.Id,
            b.BookingCode,
            b.Status.ToString(),
            b.TotalAmount,
            b.CreatedAtUtc,
            b.ExpiresAtUtc,
            b.Trip?.Route?.DepartureStation?.Name ?? "Bến đi",
            b.Trip?.Route?.ArrivalStation?.Name ?? "Bến đến",
            b.Trip?.Operator?.CompanyName ?? "Smart Ticket Express",
            b.Trip?.Vehicle?.VehicleType ?? "Xe khách chất lượng cao",
            b.Trip?.DepartureTimeUtc ?? DateTime.UtcNow,
            b.Seats.Select(s => s.SeatCode).OrderBy(x => x).ToList(),
            tickets);
    }

    public async Task<CustomerPaymentDetailsDto?> GetPaymentDetailsAsync(Guid userId, Guid bookingId, CancellationToken ct = default)
    {
        var booking = await db.Bookings.AsNoTracking().SingleOrDefaultAsync(x => x.Id == bookingId && x.CustomerUserId == userId, ct);
        if (booking is null) return null;

        var secondsRemaining = (int)Math.Max(0, (booking.ExpiresAtUtc - DateTime.UtcNow).TotalSeconds);
        var isExpired = booking.Status != BookingStatus.Pending || secondsRemaining <= 0;

        var bankBin = configuration?["Payments:BankBin"] ?? "970415";
        var bankName = configuration?["Payments:BankName"] ?? "VietinBank";
        var accountNumber = configuration?["Payments:AccountNumber"] ?? "103880262473";
        var accountName = configuration?["Payments:AccountName"] ?? "DANG ANH QUAN";
        var reference = "STKT" + booking.BookingCode;

        string? qrPayload = null;
        if (!isExpired)
        {
            try
            {
                qrPayload = NapasQrPayloadBuilder.Build(bankBin, accountNumber, accountName, booking.TotalAmount, reference);
            }
            catch
            {
                // fallback
            }
        }

        var amountFormatted = booking.TotalAmount.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        var vietQrUrl = $"https://img.vietqr.io/image/{bankBin}-{accountNumber}-compact2.png?amount={amountFormatted}&addInfo={Uri.EscapeDataString(reference)}&accountName={Uri.EscapeDataString(accountName)}";

        return new CustomerPaymentDetailsDto(
            booking.Id,
            booking.BookingCode,
            booking.TotalAmount,
            reference,
            bankBin,
            bankName,
            accountNumber,
            accountName,
            qrPayload,
            null,
            vietQrUrl,
            secondsRemaining,
            isExpired ? "Expired" : booking.Status.ToString());
    }

    public async Task<CustomerPaymentStateDto?> GetPaymentStateAsync(Guid userId, Guid bookingId, CancellationToken ct = default)
    {
        var booking = await db.Bookings.AsNoTracking().SingleOrDefaultAsync(x => x.Id == bookingId && x.CustomerUserId == userId, ct);
        if (booking is null) return null;

        var payment = await db.Payments.AsNoTracking().SingleOrDefaultAsync(x => x.BookingId == bookingId, ct);
        var isPaid = booking.Status == BookingStatus.Paid || payment?.Status == PaymentStatus.Succeeded;
        var isExpired = booking.Status == BookingStatus.Expired || (booking.Status == BookingStatus.Pending && booking.ExpiresAtUtc <= DateTime.UtcNow);
        var status = isPaid ? "Paid" : (isExpired ? "Expired" : (payment?.Status.ToString() ?? booking.Status.ToString()));

        return new CustomerPaymentStateDto(status, isPaid, isExpired, payment?.CompletedAtUtc);
    }
}
