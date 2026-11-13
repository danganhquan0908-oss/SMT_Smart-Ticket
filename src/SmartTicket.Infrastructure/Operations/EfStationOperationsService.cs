using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartTicket.Application.Operations;
using SmartTicket.Domain.Entities;
using SmartTicket.Infrastructure.Persistence;

namespace SmartTicket.Infrastructure.Operations;

public sealed class EfStationOperationsService(SmartTicketDbContext db, UserManager<ApplicationUser> users, RoleManager<IdentityRole<Guid>> roles, IConfiguration configuration, ISettlementPayoutProvider settlementPayoutProvider) : IStationOperationsService
{
    public async Task<IReadOnlyList<OperationsTripDto>> GetTripsAsync(Guid userId, CancellationToken ct)
    {
        var stationIds = await db.StationMemberships.Where(x => x.UserId == userId && x.IsActive).Select(x => x.StationId).ToListAsync(ct);
        var operatorIds = await db.OperatorMemberships.Where(x => x.UserId == userId && x.IsActive && x.Operator!.Status == OperatorStatus.Active)
            .Select(x => x.OperatorId).ToListAsync(ct);
        var trips = await db.Trips.AsNoTracking().Include(x => x.Route).ThenInclude(x => x!.DepartureStation)
            .Include(x => x.Route).ThenInclude(x => x!.ArrivalStation).Include(x => x.Operator)
            .Include(x => x.Vehicle).ThenInclude(x => x!.SeatTemplate)
            .Include(x => x.Driver)
            .Include(x => x.Tickets).Where(x => stationIds.Contains(x.Route!.DepartureStationId) || stationIds.Contains(x.Route!.ArrivalStationId) || operatorIds.Contains(x.OperatorId))
            .OrderBy(x => x.DepartureTimeUtc).ToListAsync(ct);
        var tripIds = trips.Select(x => x.Id).ToArray();
        var clearances = await db.DispatchClearances.AsNoTracking()
            .Where(x => tripIds.Contains(x.TripId) && x.IsApproved)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(ct);
        var clearanceDict = clearances.GroupBy(x => x.TripId).ToDictionary(g => g.Key, g => g.First());

        return trips.Select(x =>
        {
            var hasApproved = clearanceDict.TryGetValue(x.Id, out var cl);
            var soldCount = x.Tickets.Count(t => t.Status is TicketLifecycleStatus.Sold or TicketLifecycleStatus.CheckedIn);
            var checkedInCount = x.Tickets.Count(t => t.Status == TicketLifecycleStatus.CheckedIn);
            var totalCap = x.Vehicle?.SeatTemplate != null
                ? x.Vehicle.SeatTemplate.DeckCount * x.Vehicle.SeatTemplate.SeatsPerDeck
                : 40;

            return new OperationsTripDto(
                x.Id,
                x.Route!.DepartureStation!.Name,
                x.Route.ArrivalStation!.Name,
                x.Operator!.CompanyName,
                x.Vehicle!.PlateNumber,
                x.Driver!.FullName,
                x.DepartureTimeUtc,
                x.Status.ToString(),
                soldCount,
                hasApproved,
                checkedInCount,
                totalCap,
                x.Driver?.Phone,
                cl?.Signature,
                cl?.CreatedAtUtc,
                cl?.Notes
            );
        }).ToList();
    }

    public async Task<TripManifestDto> GetTripManifestAsync(Guid userId, Guid tripId, CancellationToken ct)
    {
        var trip = await db.Trips.AsNoTracking()
            .Include(x => x.Route).ThenInclude(x => x!.DepartureStation)
            .Include(x => x.Route).ThenInclude(x => x!.ArrivalStation)
            .Include(x => x.Operator)
            .Include(x => x.Vehicle).ThenInclude(x => x!.SeatTemplate)
            .Include(x => x.Driver)
            .SingleOrDefaultAsync(x => x.Id == tripId, ct)
            ?? throw new OperationsNotFoundException("Không tìm thấy chuyến xe.");

        await EnsureTripAccessAsync(userId, trip, ct);

        var tickets = await db.TripTickets.AsNoTracking()
            .Where(x => x.TripId == tripId && (x.Status == TicketLifecycleStatus.Sold || x.Status == TicketLifecycleStatus.CheckedIn))
            .OrderBy(x => x.SeatCode)
            .ToListAsync(ct);

        var bookingIds = tickets.Select(t => t.BookingId).Distinct().ToArray();
        var bookings = await db.Bookings.AsNoTracking()
            .Where(b => bookingIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, ct);

        var manifestItems = tickets.Select(t =>
        {
            bookings.TryGetValue(t.BookingId, out var b);
            return new TripPassengerManifestItemDto(
                t.Id,
                t.SeatCode,
                b?.PassengerName ?? "Khách đặt vé",
                b?.PassengerPhone ?? "---",
                t.Status.ToString(),
                t.PriceAtPurchase,
                t.CheckedInAtUtc,
                t.CheckInToken
            );
        }).ToList();

        var clearance = await db.DispatchClearances.AsNoTracking()
            .Where(x => x.TripId == tripId && x.IsApproved)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

        var totalCap = trip.Vehicle?.SeatTemplate != null
            ? trip.Vehicle.SeatTemplate.DeckCount * trip.Vehicle.SeatTemplate.SeatsPerDeck
            : 40;

        return new TripManifestDto(
            trip.Id,
            $"{trip.Route!.DepartureStation!.Name} → {trip.Route.ArrivalStation!.Name}",
            trip.Route.DepartureStation.Name,
            trip.Route.ArrivalStation.Name,
            trip.Operator!.CompanyName,
            trip.Vehicle!.PlateNumber,
            trip.Driver!.FullName,
            trip.Driver?.Phone ?? "---",
            trip.DepartureTimeUtc,
            trip.Status.ToString(),
            totalCap,
            manifestItems.Count,
            manifestItems.Count(m => m.Status == TicketLifecycleStatus.CheckedIn.ToString()),
            clearance != null,
            clearance?.Signature,
            clearance?.CreatedAtUtc,
            clearance?.Notes,
            manifestItems
        );
    }

    public async Task<DispatchResult> ClearDepartureAsync(Guid userId, DispatchRequest request, CancellationToken ct)
    {
        var trip = await db.Trips.Include(x => x.Route).ThenInclude(x => x!.DepartureStation).Include(x => x.Vehicle).Include(x => x.Driver)
            .SingleOrDefaultAsync(x => x.Id == request.TripId, ct) ?? throw new OperationsNotFoundException("Không tìm thấy chuyến xe.");
        if (!await db.StationMemberships.AnyAsync(x => x.UserId == userId && x.IsActive && x.StationId == trip.Route!.DepartureStationId, ct))
            throw new OperationsConflictException("Bạn không có quyền xác nhận xuất bến tại bến đi của chuyến.");
        if (trip.Status != TripStatus.Scheduled) throw new OperationsConflictException("Chuyến không còn ở trạng thái chờ xuất bến.");
        var now = DateTime.UtcNow;
        var signatureBody = $"{trip.Id:N}|{trip.Route!.DepartureStationId:N}|{userId:N}|{request.VehicleDocumentsValid}|{request.DriverLicenseValid}|{request.DriverFitForDuty}|{request.PassengerCapacityValid}|{now:O}";
        var signature = Sign(signatureBody, "Dispatch:SigningKey");
        var clearance = new DispatchClearance(trip.Id, trip.Route.DepartureStationId, userId, request.VehicleDocumentsValid,
            request.DriverLicenseValid, request.DriverFitForDuty, request.PassengerCapacityValid, request.Notes, now, signature);
        db.DispatchClearances.Add(clearance);
        if (clearance.IsApproved) trip.Start();
        await db.SaveChangesAsync(ct);
        return new DispatchResult(clearance.IsApproved, clearance.IsApproved ? "APPROVED_AND_DISPATCHED" : "REJECTED", signature, now);
    }

    public async Task<TicketScanResult> ScanTicketAsync(Guid userId, TicketScanRequest request, CancellationToken ct)
    {
        var scannedAt = request.ScannedAtUtc?.ToUniversalTime() ?? DateTime.UtcNow;
        if (request.IsOffline && (scannedAt > DateTime.UtcNow.AddMinutes(2) || scannedAt < DateTime.UtcNow.AddDays(-3)))
            throw new OperationsConflictException("Thời điểm quét ngoại tuyến không hợp lệ.");
        var signingKey = configuration["Tickets:SigningKey"] ?? "";
        if (!OfflineTicketVerifier.TryVerify(request.Jwt, signingKey, scannedAt, out var claims) || claims is null)
            return new TicketScanResult("INVALID_TOKEN", false, true, null, null, null);

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var ticket = await db.TripTickets.FromSqlInterpolated($"SELECT * FROM \"Tickets\" WHERE \"SeatId\" = {claims.SeatId} AND \"TripId\" = {claims.TripId} FOR UPDATE").SingleOrDefaultAsync(ct);
        if (ticket is null || !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(ticket.CheckInToken), Encoding.UTF8.GetBytes(request.Jwt)) || ticket.SeatCode != claims.SeatCode)
            return new TicketScanResult("INVALID_TICKET", false, true, null, null, null);
        var trip = await db.Trips.Include(x => x.Route).ThenInclude(x => x!.DepartureStation).Include(x => x.Route).ThenInclude(x => x!.ArrivalStation)
            .SingleAsync(x => x.Id == ticket.TripId, ct);
        await EnsureTripAccessAsync(userId, trip, ct);
        if (scannedAt < trip.DepartureTimeUtc.AddHours(-12) || scannedAt > trip.ArrivalTimeUtc.AddDays(1))
            return new TicketScanResult("OUTSIDE_TRIP_WINDOW", false, true, ticket.SeatCode, null, trip.DepartureTimeUtc);
        if (ticket.Status == TicketLifecycleStatus.CheckedIn)
        {
            var repeatStationId = await ResolveStationId(userId, trip.Route!.DepartureStationId, trip.Route.ArrivalStationId, ct);
            db.TicketScanLogs.Add(new TicketScanLog(ticket.Id, userId, repeatStationId, "ALREADY_USED", request.IsOffline, scannedAt));
            await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
            return new TicketScanResult("ALREADY_USED", false, true, ticket.SeatCode, $"{trip.Route!.DepartureStation!.Name} → {trip.Route.ArrivalStation!.Name}", trip.DepartureTimeUtc);
        }
        if (ticket.Status != TicketLifecycleStatus.Sold)
            return new TicketScanResult("INVALID_STATUS", false, true, ticket.SeatCode, null, trip.DepartureTimeUtc);

        var stationId = await ResolveStationId(userId, trip.Route!.DepartureStationId, trip.Route.ArrivalStationId, ct);
        ticket.CheckIn(userId, scannedAt);
        db.TicketScanLogs.Add(new TicketScanLog(ticket.Id, userId, stationId, request.IsOffline ? "CHECKED_IN_OFFLINE_SYNC" : "CHECKED_IN", request.IsOffline, scannedAt));
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return new TicketScanResult("CHECKED_IN", true, false, ticket.SeatCode, $"{trip.Route.DepartureStation!.Name} → {trip.Route.ArrivalStation!.Name}", trip.DepartureTimeUtc);
    }

    public async Task AssignStationMemberAsync(Guid stationId, string email, CancellationToken ct)
    {
        if (!await db.Stations.AnyAsync(x => x.Id == stationId, ct)) throw new OperationsNotFoundException("Không tìm thấy bến xe.");
        var user = await users.FindByEmailAsync(email.Trim()) ?? throw new OperationsNotFoundException("Tài khoản người dùng chưa tồn tại.");
        if (!user.IsActive) throw new OperationsConflictException("Chỉ có thể phân quyền tài khoản đang hoạt động.");
        var existing = await db.StationMemberships.SingleOrDefaultAsync(x => x.StationId == stationId && x.UserId == user.Id, ct);
        if (existing?.IsActive == true) throw new OperationsConflictException("Tài khoản đã được phân công tại bến này.");
        const string role = "STATION_AGENT";
        if (!await roles.RoleExistsAsync(role)) await roles.CreateAsync(new IdentityRole<Guid>(role));
        var roleResult = await users.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded && !await users.IsInRoleAsync(user, role)) throw new OperationsConflictException("Không cấp được quyền nhân viên bến.");
        if (existing is null) db.StationMemberships.Add(new StationMembership(stationId, user.Id));
        else existing.Reactivate();
        await db.SaveChangesAsync(ct);
    }

    public async Task DeactivateStationMemberAsync(Guid stationId, Guid userId, CancellationToken ct)
    {
        var membership = await db.StationMemberships.SingleOrDefaultAsync(x => x.StationId == stationId && x.UserId == userId, ct)
            ?? throw new OperationsNotFoundException("Không tìm thấy nhân viên bến.");
        membership.Deactivate();
        await db.SaveChangesAsync(ct);
    }

    public async Task CompleteTripAsync(Guid userId, Guid tripId, CancellationToken ct)
    {
        var trip = await db.Trips.SingleOrDefaultAsync(x => x.Id == tripId, ct) ?? throw new OperationsNotFoundException("Không tìm thấy chuyến xe.");
        if (!await IsOperatorOwnerAsync(userId, trip.OperatorId, ct)) throw new OperationsConflictException("Chỉ chủ nhà xe mới được hoàn tất chuyến.");
        if (!await db.DispatchClearances.AnyAsync(x => x.TripId == tripId && x.IsApproved, ct)) throw new OperationsConflictException("Chuyến chưa có lệnh xuất bến được bến xe duyệt.");
        trip.Complete(); await db.SaveChangesAsync(ct);
    }

    public async Task SetPayoutAccountAsync(Guid userId, PayoutAccountRequest request, CancellationToken ct)
    {
        var membership = await db.OperatorMemberships.Include(x => x.Operator).SingleOrDefaultAsync(x => x.UserId == userId && x.IsActive && x.Role == OperatorMemberRole.Owner && x.Operator!.Status == OperatorStatus.Active, ct)
            ?? throw new OperationsConflictException("Chỉ chủ nhà xe đang hoạt động mới được cập nhật tài khoản nhận tiền.");
        membership.Operator!.SetPayoutAccount(request.BankBin, request.AccountNumber, request.AccountName);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<SettlementDto>> GetSettlementsAsync(Guid userId, CancellationToken ct)
    {
        var operatorIds = await db.OperatorMemberships.Where(x => x.UserId == userId && x.IsActive && x.Role == OperatorMemberRole.Owner && x.Operator!.Status == OperatorStatus.Active).Select(x => x.OperatorId).ToListAsync(ct);
        return await db.TripSettlements.Where(x => operatorIds.Contains(x.OperatorId)).Include(x => x.Operator)
            .OrderByDescending(x => x.CreatedAtUtc).Select(x => new SettlementDto(x.Id, x.TripId, x.Operator!.CompanyName, x.GrossAmount, x.CommissionAmount, x.NetAmount, x.Status.ToString(), x.ProviderReference, x.CreatedAtUtc, x.PaidAtUtc)).ToListAsync(ct);
    }

    public async Task<TicketScanResult> ManualCheckInSeatAsync(Guid userId, Guid tripId, string seatCode, CancellationToken ct)
    {
        var trip = await db.Trips.Include(x => x.Route).ThenInclude(x => x!.DepartureStation)
            .Include(x => x.Route).ThenInclude(x => x!.ArrivalStation)
            .SingleOrDefaultAsync(x => x.Id == tripId, ct)
            ?? throw new OperationsNotFoundException("Không tìm thấy chuyến xe.");

        await EnsureTripAccessAsync(userId, trip, ct);

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var ticket = await db.TripTickets.FromSqlInterpolated($"SELECT * FROM \"Tickets\" WHERE \"TripId\" = {tripId} AND \"SeatCode\" = {seatCode} FOR UPDATE").SingleOrDefaultAsync(ct);
        if (ticket is null)
            return new TicketScanResult("TICKET_NOT_FOUND", false, true, seatCode, null, trip.DepartureTimeUtc);

        var now = DateTime.UtcNow;
        var stationId = await ResolveStationId(userId, trip.Route!.DepartureStationId, trip.Route.ArrivalStationId, ct);

        if (ticket.Status == TicketLifecycleStatus.CheckedIn)
        {
            db.TicketScanLogs.Add(new TicketScanLog(ticket.Id, userId, stationId, "MANUAL_ALREADY_USED", false, now));
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new TicketScanResult("ALREADY_USED", false, true, ticket.SeatCode, $"{trip.Route!.DepartureStation!.Name} → {trip.Route.ArrivalStation!.Name}", trip.DepartureTimeUtc);
        }

        if (ticket.Status != TicketLifecycleStatus.Sold)
            return new TicketScanResult("INVALID_STATUS", false, true, ticket.SeatCode, null, trip.DepartureTimeUtc);

        ticket.CheckIn(userId, now);
        db.TicketScanLogs.Add(new TicketScanLog(ticket.Id, userId, stationId, "MANUAL_CHECKED_IN", false, now));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new TicketScanResult("CHECKED_IN", true, false, ticket.SeatCode, $"{trip.Route!.DepartureStation!.Name} → {trip.Route.ArrivalStation!.Name}", trip.DepartureTimeUtc);
    }

    public async Task TriggerSettlementPayoutAsync(Guid userId, Guid settlementId, CancellationToken ct)
    {
        var settlement = await db.TripSettlements.Include(x => x.Operator).SingleOrDefaultAsync(x => x.Id == settlementId, ct)
            ?? throw new OperationsNotFoundException("Không tìm thấy bản ghi đối soát.");
        if (!await IsOperatorOwnerAsync(userId, settlement.OperatorId, ct))
            throw new OperationsConflictException("Chỉ chủ nhà xe mới được yêu cầu giải ngân đối soát.");
        if (settlement.Status == SettlementStatus.Paid)
            throw new OperationsConflictException("Bản ghi đối soát này đã được chi trả thành công.");

        var op = settlement.Operator!;
        if (string.IsNullOrWhiteSpace(op.PayoutBankBin) || string.IsNullOrWhiteSpace(op.PayoutAccountNumber) || string.IsNullOrWhiteSpace(op.PayoutAccountName))
            throw new OperationsConflictException("Nhà xe chưa khai báo tài khoản nhận tiền Napas.");

        settlement.MarkProcessing();
        await db.SaveChangesAsync(ct);

        var instruction = new PayoutInstruction(settlement.Id, $"STT-{settlement.TripId:N}", settlement.NetAmount, op.PayoutBankBin, op.PayoutAccountNumber, op.PayoutAccountName);
        var result = await settlementPayoutProvider.TransferAsync(instruction, ct);
        if (result.Succeeded) settlement.MarkPaid(result.ProviderReference!);
        else settlement.MarkFailed(result.Error ?? "Giải ngân Napas 247 thất bại.");
        await db.SaveChangesAsync(ct);
    }

    private async Task EnsureTripAccessAsync(Guid userId, Trip trip, CancellationToken ct)
    {
        var stationIds = await db.StationMemberships.Where(x => x.UserId == userId && x.IsActive).Select(x => x.StationId).ToListAsync(ct);
        var stationAccess = stationIds.Contains(trip.Route!.DepartureStationId) || stationIds.Contains(trip.Route.ArrivalStationId);
        if (stationAccess || await IsOperatorMemberAsync(userId, trip.OperatorId, ct)) return;
        throw new OperationsConflictException("Tài khoản không có quyền soát vé của chuyến này.");
    }

    private async Task<bool> IsOperatorMemberAsync(Guid userId, Guid operatorId, CancellationToken ct) =>
        await db.OperatorMemberships.AnyAsync(x => x.UserId == userId && x.OperatorId == operatorId && x.IsActive && x.Operator!.Status == OperatorStatus.Active, ct);

    private async Task<bool> IsOperatorOwnerAsync(Guid userId, Guid operatorId, CancellationToken ct) =>
        await db.OperatorMemberships.AnyAsync(x => x.UserId == userId && x.OperatorId == operatorId && x.IsActive && x.Role == OperatorMemberRole.Owner && x.Operator!.Status == OperatorStatus.Active, ct);

    private async Task<Guid?> ResolveStationId(Guid userId, Guid departureId, Guid arrivalId, CancellationToken ct) =>
        await db.StationMemberships.Where(x => x.UserId == userId && x.IsActive && (x.StationId == departureId || x.StationId == arrivalId)).Select(x => (Guid?)x.StationId).FirstOrDefaultAsync(ct);

    private string Sign(string value, string setting)
    {
        var key = configuration[setting];
        if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32) throw new InvalidOperationException($"Cần cấu hình {setting} tối thiểu 32 byte.");
        return Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(value)));
    }
}
