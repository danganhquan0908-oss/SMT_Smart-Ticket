using BusGoSmartTicket.Exceptions.Operators;
using BusGoSmartTicket.Models.DTOs.Operators;
using BusGoSmartTicket.Models.Entities;
using BusGoSmartTicket.Services.Interfaces.Operators;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BusGoSmartTicket.Data.Repositories.Operators;

public sealed class EfOperatorRepository(
    ApplicationDbContext db,
    UserManager<ApplicationUser> users,
    RoleManager<IdentityRole<Guid>> roles) : IOperatorRepository
{
    public Task<bool> OperatorTaxCodeExistsAsync(string taxCode, CancellationToken ct) =>
        db.BusOperators.AnyAsync(x => x.TaxCode == taxCode, ct);

    public async Task<bool> UserEmailExistsAsync(string email, CancellationToken ct) => await users.FindByEmailAsync(email) is not null;

    public async Task RegisterAsync(OperatorRegistrationRequest request, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var email = request.Email.Trim();
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = email, Email = email, PhoneNumber = request.Hotline.Trim(), IsActive = false };
        var created = await users.CreateAsync(user);
        if (!created.Succeeded)
            throw new OperatorConflictException(string.Join(" ", created.Errors.Select(x => x.Description)));
        var op = new BusOperator(user.Id, request.CompanyName, request.TaxCode.Trim(), request.Hotline, request.BusinessLicensePath);
        var member = new OperatorMembership(op.Id, user.Id, OperatorMemberRole.Owner);
        db.BusOperators.Add(op);
        db.OperatorMemberships.Add(member);
        await SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task<IReadOnlyList<OperatorApplicationDto>> GetApplicationsAsync(CancellationToken ct)
    {
        var applications = await db.BusOperators.AsNoTracking()
            .OrderByDescending(op => op.CreatedAtUtc)
            .ToListAsync(ct);
        var ownerIds = applications.Select(op => op.OwnerUserId).Distinct().ToArray();
        var ownerEmails = await db.Users.AsNoTracking()
            .Where(user => ownerIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.Email ?? string.Empty, ct);

        return applications.Select(op => new OperatorApplicationDto(
            op.Id,
            op.CompanyName,
            op.TaxCode,
            ownerEmails.GetValueOrDefault(op.OwnerUserId, string.Empty),
            op.Hotline,
            op.Status,
            op.ReviewNote,
            op.CreatedAtUtc)).ToArray();
    }

    public async Task<ActivationNotice?> ReviewAsync(Guid operatorId, bool approve, string? note, string tokenHash, DateTime expiresAtUtc, CancellationToken ct)
    {
        var op = await db.BusOperators.SingleOrDefaultAsync(x => x.Id == operatorId, ct);
        if (op is null || op.Status != OperatorStatus.PendingVerification)
            return null;
        if (!approve)
        {
            op.Reject(string.IsNullOrWhiteSpace(note) ? "Hồ sơ chưa đáp ứng yêu cầu." : note);
            await SaveChangesAsync(ct);
            return null;
        }

        op.Approve();
        await EnsureRoleAsync("OPERATOR_OWNER");
        var owner = await users.FindByIdAsync(op.OwnerUserId.ToString()) ?? throw new OperatorNotFoundException("Không tìm thấy chủ nhà xe.");
        var addRole = await users.AddToRoleAsync(owner, "OPERATOR_OWNER");
        if (!addRole.Succeeded && !await users.IsInRoleAsync(owner, "OPERATOR_OWNER"))
            throw new OperatorConflictException(string.Join(" ", addRole.Errors.Select(x => x.Description)));
        db.OperatorActivations.Add(new OperatorActivation(op.OwnerUserId, op.Id, tokenHash, expiresAtUtc));
        await SaveChangesAsync(ct);
        return new ActivationNotice(owner.Email!, string.Empty);
    }

    public async Task<ActivationNotice> InviteDispatcherAsync(Guid operatorId, string email, string tokenHash, DateTime expiresAtUtc, CancellationToken ct)
    {
        var op = await db.BusOperators.SingleOrDefaultAsync(x => x.Id == operatorId && x.Status == OperatorStatus.Active, ct)
            ?? throw new OperatorNotFoundException("Nhà xe chưa được kích hoạt.");
        email = email.Trim();
        if (await users.FindByEmailAsync(email) is not null)
            throw new OperatorConflictException("Email này đã có tài khoản.");
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = email, Email = email, EmailConfirmed = true, IsActive = false };
        var created = await users.CreateAsync(user);
        if (!created.Succeeded)
            throw new OperatorConflictException(string.Join(" ", created.Errors.Select(x => x.Description)));
        await EnsureRoleAsync("OPERATOR_DISPATCHER");
        var roleResult = await users.AddToRoleAsync(user, "OPERATOR_DISPATCHER");
        if (!roleResult.Succeeded)
            throw new OperatorConflictException(string.Join(" ", roleResult.Errors.Select(x => x.Description)));
        db.OperatorMemberships.Add(new OperatorMembership(operatorId, user.Id, OperatorMemberRole.Dispatcher));
        db.OperatorActivations.Add(new OperatorActivation(user.Id, op.Id, tokenHash, expiresAtUtc));
        await SaveChangesAsync(ct);
        return new ActivationNotice(email, string.Empty);
    }

    public async Task<ActivationNotice> ResendActivationAsync(Guid operatorId, string tokenHash, DateTime expiresAtUtc, CancellationToken ct)
    {
        var op = await db.BusOperators.SingleOrDefaultAsync(x => x.Id == operatorId && x.Status == OperatorStatus.Active, ct)
            ?? throw new OperatorNotFoundException("Không tìm thấy nhà xe đã được phê duyệt.");
        var owner = await users.FindByIdAsync(op.OwnerUserId.ToString());
        if (owner is null || owner.IsActive)
            throw new OperatorConflictException("Tài khoản nhà xe đã được kích hoạt hoặc không hợp lệ.");
        db.OperatorActivations.Add(new OperatorActivation(op.OwnerUserId, op.Id, tokenHash, expiresAtUtc));
        await SaveChangesAsync(ct);
        return new ActivationNotice(owner.Email!, string.Empty);
    }

    public async Task<string> GetLicensePathAsync(Guid operatorId, CancellationToken ct) =>
        await db.BusOperators.Where(x => x.Id == operatorId).Select(x => x.BusinessLicensePath).SingleOrDefaultAsync(ct)
        ?? throw new OperatorNotFoundException("Không tìm thấy hồ sơ nhà xe.");

    public async Task<ActivationInfo?> GetActivationAsync(string tokenHash, CancellationToken ct)
    {
        var activation = await db.OperatorActivations.Include(x => x.Operator)
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash, ct);
        if (activation is null || !activation.IsValid(DateTime.UtcNow, tokenHash) || activation.Operator?.Status != OperatorStatus.Active)
            return null;
        var activatingUser = await users.FindByIdAsync(activation.UserId.ToString());
        if (activatingUser is null || activatingUser.IsActive)
            return null;
        return new ActivationInfo(activatingUser.Email!, activation.Operator.CompanyName, activation.ExpiresAtUtc);
    }

    public async Task<AuthenticatorSetup> BeginActivationAsync(string tokenHash, string password, CancellationToken ct)
    {
        var activation = await GetActivationEntityAsync(tokenHash, ct);
        if (activation is null)
            throw new OperatorConflictException("Liên kết kích hoạt không hợp lệ, đã dùng hoặc đã hết hạn.");
        var user = await users.FindByIdAsync(activation.UserId.ToString()) ?? throw new OperatorNotFoundException("Không tìm thấy tài khoản.");
        if (!await users.HasPasswordAsync(user))
        {
            var result = await users.AddPasswordAsync(user, password);
            if (!result.Succeeded)
                throw new OperatorConflictException(string.Join(" ", result.Errors.Select(x => x.Description)));
        }
        var key = await users.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrWhiteSpace(key))
        {
            var reset = await users.ResetAuthenticatorKeyAsync(user);
            if (!reset.Succeeded)
                throw new InvalidOperationException("Không tạo được khóa xác thực.");
            key = await users.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("Không lưu được khóa xác thực.");
        }
        return new AuthenticatorSetup(user.Email!, key);
    }

    public async Task CompleteActivationAsync(string tokenHash, string authenticatorCode, CancellationToken ct)
    {
        var activation = await GetActivationEntityAsync(tokenHash, ct);
        if (activation is null)
            throw new OperatorConflictException("Liên kết kích hoạt không hợp lệ, đã dùng hoặc đã hết hạn.");
        var user = await users.FindByIdAsync(activation.UserId.ToString()) ?? throw new OperatorNotFoundException("Không tìm thấy tài khoản.");
        if (user.IsActive)
            throw new OperatorConflictException("Tài khoản này đã được kích hoạt.");
        var valid = await users.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, authenticatorCode.Replace(" ", string.Empty).Replace("-", string.Empty));
        if (!valid)
            throw new OperatorConflictException("Mã xác thực không chính xác.");
        var enabled = await users.SetTwoFactorEnabledAsync(user, true);
        if (!enabled.Succeeded)
            throw new InvalidOperationException("Không bật được xác thực hai lớp.");
        user.IsActive = true;
        var activationTokens = await db.OperatorActivations.Where(x => x.UserId == user.Id && x.UsedAtUtc == null).ToListAsync(ct);
        foreach (var activationToken in activationTokens)
            activationToken.Consume();
        await db.SaveChangesAsync(ct);
    }

    public Task<Guid?> GetActiveOperatorIdAsync(Guid userId, CancellationToken ct) =>
        db.OperatorMemberships.Join(db.Users, membership => membership.UserId, user => user.Id,
                (membership, user) => new { membership, user })
            .Where(x => x.membership.UserId == userId && x.membership.IsActive && x.user.IsActive &&
                x.membership.Operator!.Status == OperatorStatus.Active)
            .Select(x => (Guid?)x.membership.OperatorId).FirstOrDefaultAsync(ct);

    public Task<OperatorProfileDto?> GetProfileAsync(Guid userId, CancellationToken ct) =>
        db.OperatorMemberships.Join(db.Users, membership => membership.UserId, user => user.Id,
                (membership, user) => new { membership, user })
            .Where(x => x.membership.UserId == userId && x.membership.IsActive && x.user.IsActive &&
                x.membership.Operator!.Status == OperatorStatus.Active)
            .Select(x => new OperatorProfileDto(x.membership.Operator!.Id, x.membership.Operator.CompanyName, x.membership.Operator.TaxCode, x.membership.Operator.Hotline))
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<SeatTemplate>> GetSeatTemplatesAsync(Guid operatorId, CancellationToken ct) =>
        await db.SeatTemplates.Include(x => x.Seats).Where(x => x.OperatorId == operatorId).OrderBy(x => x.Name).ToListAsync(ct);

    public async Task<SeatTemplate> CreateSeatTemplateAsync(Guid operatorId, SeatTemplateRequest request, CancellationToken ct)
    {
        if (await db.SeatTemplates.AnyAsync(x => x.OperatorId == operatorId && x.Name == request.Name.Trim(), ct))
            throw new OperatorConflictException("Tên sơ đồ xe đã tồn tại.");
        var template = new SeatTemplate(operatorId, request.Name, request.VehicleType, request.DeckCount, request.SeatsPerDeck);
        db.SeatTemplates.Add(template);
        await SaveChangesAsync(ct);
        return template;
    }

    public async Task<IReadOnlyList<Vehicle>> GetVehiclesAsync(Guid operatorId, CancellationToken ct) =>
        await db.Vehicles.Include(x => x.SeatTemplate).ThenInclude(x => x!.Seats).Where(x => x.OperatorId == operatorId).OrderBy(x => x.PlateNumber).ToListAsync(ct);

    public async Task<Vehicle> CreateVehicleAsync(Guid operatorId, VehicleRequest request, CancellationToken ct)
    {
        if (!await db.SeatTemplates.AnyAsync(x => x.Id == request.SeatTemplateId && x.OperatorId == operatorId && x.IsActive, ct))
            throw new OperatorConflictException("Sơ đồ xe không thuộc nhà xe hoặc đã ngừng hoạt động.");
        if (await db.Vehicles.AnyAsync(x => x.OperatorId == operatorId && x.PlateNumber == request.PlateNumber.Trim().ToUpper(), ct))
            throw new OperatorConflictException("Biển số xe đã được khai báo.");
        var vehicle = new Vehicle(operatorId, request.SeatTemplateId, request.PlateNumber, request.VehicleType);
        db.Vehicles.Add(vehicle);
        await SaveChangesAsync(ct);
        return await db.Vehicles.Include(x => x.SeatTemplate).ThenInclude(x => x!.Seats).SingleAsync(x => x.Id == vehicle.Id, ct);
    }

    public async Task UpdateVehicleAsync(Guid operatorId, Guid vehicleId, VehicleRequest request, CancellationToken ct)
    {
        var vehicle = await db.Vehicles.SingleOrDefaultAsync(x => x.Id == vehicleId && x.OperatorId == operatorId && x.IsActive, ct)
            ?? throw new OperatorNotFoundException("Không tìm thấy xe đang hoạt động thuộc nhà xe của bạn.");
        if (!await db.SeatTemplates.AnyAsync(x => x.Id == request.SeatTemplateId && x.OperatorId == operatorId && x.IsActive, ct))
            throw new OperatorConflictException("Sơ đồ xe không thuộc nhà xe hoặc đã ngừng hoạt động.");
        if (await db.Vehicles.AnyAsync(x => x.OperatorId == operatorId && x.Id != vehicleId && x.PlateNumber == request.PlateNumber.Trim().ToUpper(), ct))
            throw new OperatorConflictException("Biển số xe đã được khai báo.");
        vehicle.Update(request.SeatTemplateId, request.PlateNumber, request.VehicleType);
        await SaveChangesAsync(ct);
    }

    public async Task DeactivateVehicleAsync(Guid operatorId, Guid vehicleId, CancellationToken ct)
    {
        var vehicle = await db.Vehicles.SingleOrDefaultAsync(x => x.Id == vehicleId && x.OperatorId == operatorId, ct)
            ?? throw new OperatorNotFoundException("Không tìm thấy xe thuộc nhà xe của bạn.");
        if (await db.Trips.AnyAsync(t => t.VehicleId == vehicleId && (t.Status == TripStatus.Scheduled || t.Status == TripStatus.Running), ct))
            throw new OperatorConflictException("Xe đang được phân công cho chuyến hoạt động.");
        vehicle.Deactivate();
        await SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Driver>> GetDriversAsync(Guid operatorId, CancellationToken ct) =>
        await db.Drivers.Where(x => x.OperatorId == operatorId).OrderBy(x => x.FullName).ToListAsync(ct);

    public async Task<Driver> CreateDriverAsync(Guid operatorId, DriverRequest request, CancellationToken ct)
    {
        if (await db.Drivers.AnyAsync(x => x.OperatorId == operatorId && x.LicenseNumber == request.LicenseNumber.Trim(), ct))
            throw new OperatorConflictException("Số giấy phép lái xe đã được khai báo.");
        var driver = new Driver(operatorId, request.FullName, request.Phone, request.LicenseNumber);
        db.Drivers.Add(driver);
        await SaveChangesAsync(ct);
        return driver;
    }

    public async Task UpdateDriverAsync(Guid operatorId, Guid driverId, DriverRequest request, CancellationToken ct)
    {
        var driver = await db.Drivers.SingleOrDefaultAsync(x => x.Id == driverId && x.OperatorId == operatorId && x.IsActive, ct)
            ?? throw new OperatorNotFoundException("Không tìm thấy tài xế đang hoạt động thuộc nhà xe của bạn.");
        if (await db.Drivers.AnyAsync(x => x.OperatorId == operatorId && x.Id != driverId && x.LicenseNumber == request.LicenseNumber.Trim(), ct))
            throw new OperatorConflictException("Số giấy phép lái xe đã được khai báo.");
        driver.Update(request.FullName, request.Phone, request.LicenseNumber);
        await SaveChangesAsync(ct);
    }

    public async Task DeactivateDriverAsync(Guid operatorId, Guid driverId, CancellationToken ct)
    {
        var driver = await db.Drivers.SingleOrDefaultAsync(x => x.Id == driverId && x.OperatorId == operatorId, ct)
            ?? throw new OperatorNotFoundException("Không tìm thấy tài xế thuộc nhà xe của bạn.");
        if (await db.Trips.AnyAsync(t => t.DriverId == driverId && (t.Status == TripStatus.Scheduled || t.Status == TripStatus.Running), ct))
            throw new OperatorConflictException("Tài xế đang được phân công cho chuyến hoạt động.");
        driver.Deactivate();
        await SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Trip>> GetTripsAsync(Guid operatorId, CancellationToken ct) =>
        await db.Trips.Include(x => x.Route).ThenInclude(x => x!.DepartureStation)
            .Include(x => x.Route).ThenInclude(x => x!.ArrivalStation)
            .Include(x => x.Vehicle).Include(x => x.Driver).Include(x => x.Tickets)
            .Where(x => x.OperatorId == operatorId).OrderBy(x => x.DepartureTimeUtc).ToListAsync(ct);

    public async Task<Trip> CreateTripAsync(Guid operatorId, Guid userId, TripRequest request, CancellationToken ct)
    {
        await EnsureTripAssetsAsync(operatorId, request.RouteId, request.VehicleId, request.DriverId, ct);
        var trip = new Trip(operatorId, request.RouteId, request.VehicleId, request.DriverId, request.DepartureTimeUtc, request.ArrivalTimeUtc, request.TicketPrice);
        var vehicle = await db.Vehicles.Include(v => v.SeatTemplate).ThenInclude(t => t!.Seats)
            .SingleAsync(v => v.Id == request.VehicleId && v.OperatorId == operatorId, ct);
        foreach (var layoutSeat in vehicle.SeatTemplate!.Seats)
            trip.Seats.Add(new Seat(trip.Id, layoutSeat.SeatCode, layoutSeat.Deck, layoutSeat.Row, layoutSeat.Column));
        db.Trips.Add(trip);
        await SaveChangesAsync(ct);
        return await LoadTripAsync(operatorId, trip.Id, ct);
    }

    public async Task UpdateTripScheduleAsync(Guid operatorId, Guid userId, Guid tripId, TripScheduleRequest request, CancellationToken ct)
    {
        var trip = await db.Trips.Include(x => x.Tickets).SingleOrDefaultAsync(x => x.Id == tripId && x.OperatorId == operatorId, ct)
            ?? throw new OperatorNotFoundException("Không tìm thấy chuyến thuộc nhà xe của bạn.");
        await EnsureTripAssetsAsync(operatorId, request.RouteId, request.VehicleId, request.DriverId, ct);
        trip.UpdateSchedule(request.RouteId, request.VehicleId, request.DriverId, request.DepartureTimeUtc, request.ArrivalTimeUtc,
            trip.Tickets.Any(t => t.Status == TicketLifecycleStatus.Sold));
        await SaveChangesAsync(ct);
    }

    public async Task ChangeTripPriceAsync(Guid operatorId, Guid userId, Guid tripId, decimal newPrice, CancellationToken ct)
    {
        var trip = await db.Trips.SingleOrDefaultAsync(x => x.Id == tripId && x.OperatorId == operatorId, ct)
            ?? throw new OperatorNotFoundException("Không tìm thấy chuyến thuộc nhà xe của bạn.");
        if (trip.TicketPrice == newPrice)
            return;
        var oldPrice = trip.TicketPrice;
        trip.ChangePrice(newPrice);
        db.TripAuditLogs.Add(new TripAuditLog(trip.Id, userId, oldPrice, newPrice, "UPDATE_PRICE"));
        await SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<TripPriceAuditDto>> GetPriceHistoryAsync(Guid operatorId, Guid tripId, CancellationToken ct)
    {
        if (!await db.Trips.AnyAsync(x => x.Id == tripId && x.OperatorId == operatorId, ct))
            throw new OperatorNotFoundException("Không tìm thấy chuyến thuộc nhà xe của bạn.");
        return await db.TripAuditLogs.Where(x => x.TripId == tripId).OrderByDescending(x => x.ChangedAtUtc)
            .Join(db.Users, log => log.ModifiedByUserId, user => user.Id,
                (log, user) => new TripPriceAuditDto(log.ChangedAtUtc, user.Email ?? user.UserName ?? "", log.OldPrice, log.NewPrice, log.Action))
            .ToListAsync(ct);
    }

    private async Task EnsureTripAssetsAsync(Guid operatorId, Guid routeId, Guid vehicleId, Guid driverId, CancellationToken ct)
    {
        if (!await db.Routes.AnyAsync(r => r.Id == routeId && r.IsActive && r.DepartureStation!.IsActive && r.ArrivalStation!.IsActive, ct))
            throw new OperatorConflictException("Tuyến đường không tồn tại hoặc đã ngừng hoạt động.");
        if (!await db.Vehicles.AnyAsync(v => v.Id == vehicleId && v.OperatorId == operatorId && v.IsActive, ct))
            throw new OperatorConflictException("Xe không thuộc nhà xe hoặc đã ngừng hoạt động.");
        if (!await db.Drivers.AnyAsync(d => d.Id == driverId && d.OperatorId == operatorId && d.IsActive, ct))
            throw new OperatorConflictException("Tài xế không thuộc nhà xe hoặc đã ngừng hoạt động.");
    }

    private async Task<Trip> LoadTripAsync(Guid operatorId, Guid id, CancellationToken ct) =>
        await db.Trips.Include(x => x.Route).ThenInclude(x => x!.DepartureStation)
            .Include(x => x.Route).ThenInclude(x => x!.ArrivalStation)
            .Include(x => x.Vehicle).Include(x => x.Driver).Include(x => x.Tickets)
            .SingleAsync(x => x.Id == id && x.OperatorId == operatorId, ct);

    private async Task<OperatorActivation?> GetActivationEntityAsync(string tokenHash, CancellationToken ct) =>
        await db.OperatorActivations.Include(x => x.Operator)
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash && x.ExpiresAtUtc > DateTime.UtcNow && x.UsedAtUtc == null &&
                x.Operator!.Status == OperatorStatus.Active, ct);

    private async Task EnsureRoleAsync(string role)
    {
        if (!await roles.RoleExistsAsync(role))
        {
            var result = await roles.CreateAsync(new IdentityRole<Guid>(role));
            if (!result.Succeeded)
                throw new OperatorConflictException(string.Join(" ", result.Errors.Select(x => x.Description)));
        }
    }

    private async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new OperatorConflictException("Thông tin này đã được sử dụng.");
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new OperatorConflictException("Dữ liệu vừa được cập nhật bởi người khác. Tải lại trang và thử lại.");
        }
    }
}
