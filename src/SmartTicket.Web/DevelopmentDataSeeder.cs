using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartTicket.Domain.Entities;
using SmartTicket.Infrastructure.Persistence;

namespace SmartTicket.Web;

internal static class DevelopmentDataSeeder
{
    private static readonly (string Code, string Name, string Province, string Address)[] StationSeeds =
    [
        ("SMT-HCM", "Bến xe Miền Đông mới", "TP. Hồ Chí Minh", "501 Hoàng Hữu Nam, TP. Thủ Đức"),
        ("SMT-DLI", "Bến xe liên tỉnh Đà Lạt", "Lâm Đồng", "01 Tô Hiến Thành, Đà Lạt"),
        ("SMT-NTR", "Bến xe phía Nam Nha Trang", "Khánh Hòa", "Vĩnh Trung, Nha Trang"),
        ("SMT-CTO", "Bến xe Trung tâm Cần Thơ", "Cần Thơ", "Khu đô thị Nam Cần Thơ"),
        ("SMT-HAN", "Bến xe Mỹ Đình", "Hà Nội", "20 Phạm Hùng, Nam Từ Liêm"),
        ("SMT-HUE", "Bến xe phía Nam Huế", "TP. Huế", "An Đông, TP. Huế"),
        ("TPTN_01", "Bến xe trung tâm Thái Nguyên", "Thái Nguyên", "Đồng Quang, TP. Thái Nguyên")
    ];

    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        if (!configuration.GetValue("DemoData:Enabled", true)) return;

        var db = services.GetRequiredService<SmartTicketDbContext>();
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        var operatorEmail = configuration["DemoData:OperatorEmail"] ?? "nhaxe@smartticket.vn";
        var operatorPassword = configuration["DemoData:OperatorPassword"] ?? "SmartTicket@2026";
        var stationEmail = configuration["DemoData:StationEmail"] ?? "benxe@smartticket.vn";
        var stationPassword = configuration["DemoData:StationPassword"] ?? "SmartTicket@2026";

        var operatorUser = await EnsureUserAsync(users, roles, operatorEmail, operatorPassword, "OPERATOR_OWNER", "Đại diện SMT Express", "0900000111");
        var stationUser = await EnsureUserAsync(users, roles, stationEmail, stationPassword, "STATION_AGENT", "Kiểm soát Bến xe", "0900000222");

        // Đồng thời khởi tạo tài khoản dự phòng theo định dạng .local
        var legacyOperatorUser = await EnsureUserAsync(users, roles, "demo.operator@smartticket.local", "SmartTicket!Operator2026", "OPERATOR_OWNER", "SMT Express Local", "0900000112");
        var legacyStationUser = await EnsureUserAsync(users, roles, "demo.station@smartticket.local", "SmartTicket!Station2026", "STATION_AGENT", "Bến xe Local", "0900000223");
        var customerUser = await EnsureUserAsync(users, roles, "khachhang@smartticket.vn", "SmartTicket@2026", "CUSTOMER", "Đặng Anh Quân", "0345588495");

        var stations = new Dictionary<string, Station>(StringComparer.OrdinalIgnoreCase);
        foreach (var seed in StationSeeds)
        {
            var station = await db.Stations.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Code == seed.Code);
            if (station is null)
            {
                station = new Station(seed.Code, seed.Name, seed.Province, seed.Address);
                db.Stations.Add(station);
            }
            else if (!station.IsActive) station.Reactivate();
            stations.Add(seed.Code, station);
        }
        await db.SaveChangesAsync();

        var routeSeeds = new[]
        {
            (From: "SMT-HCM", To: "SMT-DLI", Km: 303m, Hours: 6),
            (From: "SMT-HCM", To: "SMT-NTR", Km: 435m, Hours: 8),
            (From: "SMT-HCM", To: "SMT-CTO", Km: 170m, Hours: 4),
            (From: "SMT-HAN", To: "SMT-HUE", Km: 665m, Hours: 12)
        };
        var routes = new Dictionary<(string From, string To), BusRoute>();
        foreach (var seed in routeSeeds)
        {
            var route = await db.Routes.IgnoreQueryFilters().SingleOrDefaultAsync(x =>
                x.DepartureStationId == stations[seed.From].Id && x.ArrivalStationId == stations[seed.To].Id);
            if (route is null)
            {
                route = new BusRoute(stations[seed.From].Id, stations[seed.To].Id, seed.Km, seed.Hours);
                db.Routes.Add(route);
            }
            else if (!route.IsActive) route.Reactivate();
            routes.Add((seed.From, seed.To), route);
        }

        var busOperator = await db.BusOperators.SingleOrDefaultAsync(x => x.TaxCode == "9999999901");
        if (busOperator is null)
        {
            busOperator = new BusOperator(operatorUser.Id, "SMT Express", "9999999901", "0900 000 111", "demo/smt-express-license.pdf");
            db.BusOperators.Add(busOperator);
        }
        if (busOperator.Status != OperatorStatus.Active) busOperator.Approve();
        await db.SaveChangesAsync();

        if (!await db.OperatorMemberships.AnyAsync(x => x.OperatorId == busOperator.Id && x.UserId == operatorUser.Id))
            db.OperatorMemberships.Add(new OperatorMembership(busOperator.Id, operatorUser.Id, OperatorMemberRole.Owner));
        if (!await db.OperatorMemberships.AnyAsync(x => x.OperatorId == busOperator.Id && x.UserId == legacyOperatorUser.Id))
            db.OperatorMemberships.Add(new OperatorMembership(busOperator.Id, legacyOperatorUser.Id, OperatorMemberRole.Owner));

        foreach (var station in stations.Values)
        {
            if (!await db.StationMemberships.AnyAsync(x => x.StationId == station.Id && x.UserId == stationUser.Id))
                db.StationMemberships.Add(new StationMembership(station.Id, stationUser.Id));
            if (!await db.StationMemberships.AnyAsync(x => x.StationId == station.Id && x.UserId == legacyStationUser.Id))
                db.StationMemberships.Add(new StationMembership(station.Id, legacyStationUser.Id));
        }

        var sleeper = await EnsureTemplateAsync(db, busOperator.Id, "Giường nằm 40 chỗ · Demo", "Giường nằm", 2, 20);
        var limousine = await EnsureTemplateAsync(db, busOperator.Id, "Limousine 20 chỗ · Demo", "Limousine", 1, 20);
        var drivers = await EnsureDriversAsync(db, busOperator.Id);
        var vehicles = await EnsureVehiclesAsync(db, busOperator.Id, sleeper.Id, limousine.Id);
        await db.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.Now);
        var tripSeeds = new[]
        {
            (Route: ("SMT-HCM", "SMT-DLI"), Day: today.AddDays(1), Time: new TimeOnly(20, 30), Price: 320_000m, Vehicle: vehicles[0], Driver: drivers[0]),
            (Route: ("SMT-HCM", "SMT-NTR"), Day: today.AddDays(1), Time: new TimeOnly(21, 15), Price: 450_000m, Vehicle: vehicles[0], Driver: drivers[1]),
            (Route: ("SMT-HCM", "SMT-DLI"), Day: today.AddDays(2), Time: new TimeOnly(22, 0), Price: 300_000m, Vehicle: vehicles[1], Driver: drivers[1])
        };

        foreach (var seed in tripSeeds)
        {
            var route = routes[seed.Route];
            var departureUtc = new DateTimeOffset(seed.Day.ToDateTime(seed.Time), TimeSpan.FromHours(7)).UtcDateTime;
            var trip = await db.Trips.SingleOrDefaultAsync(x => x.OperatorId == busOperator.Id && x.RouteId == route.Id && x.DepartureTimeUtc == departureUtc);
            if (trip is null)
            {
                var duration = TimeSpan.FromHours(route.EstimatedHours);
                trip = new Trip(busOperator.Id, route.Id, seed.Vehicle.Id, seed.Driver.Id, departureUtc, departureUtc.Add(duration), seed.Price);
                db.Trips.Add(trip);
                await db.SaveChangesAsync();
            }

            var templateSeats = await db.SeatTemplateSeats.AsNoTracking()
                .Where(x => x.SeatTemplateId == seed.Vehicle.SeatTemplateId)
                .OrderBy(x => x.Deck).ThenBy(x => x.Number).ToListAsync();
            var existingCodes = await db.Seats.Where(x => x.TripId == trip.Id).Select(x => x.SeatCode).ToListAsync();
            var existing = existingCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var seat in templateSeats.Where(x => !existing.Contains(x.SeatCode)))
                db.Seats.Add(new Seat(trip.Id, seat.SeatCode, seat.Deck, seat.Row, seat.Column));
        }

        await db.SaveChangesAsync();

        // Seed demo booking & tickets for Epic 5 station turnstile verification
        var firstTrip = await db.Trips.Include(t => t.Seats).FirstOrDefaultAsync(t => t.OperatorId == busOperator.Id);
        if (firstTrip != null && !await db.Bookings.AnyAsync(b => b.TripId == firstTrip.Id))
        {
            var price = firstTrip.TicketPrice;
            var demoBooking = new Booking(firstTrip.Id, "Nguyễn Văn An", "0912345678", "nguyen.an@demo.vn", price * 2);
            demoBooking.MarkPaid();
            db.Bookings.Add(demoBooking);
            await db.SaveChangesAsync();

            var availableSeats = firstTrip.Seats.Take(2).ToList();
            var signingKey = configuration["Tickets:SigningKey"] ?? "smartticket-jwt-signing-key-at-least-32-chars-long!";
            foreach (var seat in availableSeats)
            {
                seat.TryHold(demoBooking.Id, DateTime.UtcNow, TimeSpan.FromHours(1));
                seat.ConfirmBooking(demoBooking.Id);
                var bSeat = new BookingSeat(demoBooking.Id, seat.Id, seat.SeatCode, price);
                db.BookingSeats.Add(bSeat);

                var jwt = CreateDemoCheckInJwt(demoBooking, seat.Id, seat.SeatCode, firstTrip.Id, firstTrip.DepartureTimeUtc, signingKey);
                var ticket = new TripTicket(firstTrip.Id, demoBooking.Id, seat.Id, seat.SeatCode, price, jwt);
                db.TripTickets.Add(ticket);
            }
            await db.SaveChangesAsync();
        }

        // Khởi tạo các nhà xe độc lập khác nhằm bảo đảm nguyên tắc Tenant Isolation
        await SeedPhuongTrangOperatorAsync(db, users, roles, routes, today, busOperator.Id);
        await SeedHoangLongOperatorAsync(db, users, roles, routes, today, busOperator.Id);
        await SeedDucQuanOperatorAsync(db, users, roles, configuration);
    }

    private static async Task SeedPhuongTrangOperatorAsync(SmartTicketDbContext db,
        UserManager<ApplicationUser> users, RoleManager<IdentityRole<Guid>> roles,
        Dictionary<(string From, string To), BusRoute> routes, DateOnly today, Guid smtOperatorId)
    {
        var owner = await EnsureUserAsync(users, roles, "phuongtrang@smartticket.vn", "SmartTicket@2026", "OPERATOR_OWNER");
        var busOperator = await db.BusOperators.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.TaxCode == "0302820379");
        if (busOperator is null)
        {
            busOperator = new BusOperator(owner.Id, "Phương Trang (FUTA Bus Lines)", "0302820379", "1900 6067", "demo/futa-license.pdf");
            db.BusOperators.Add(busOperator);
        }
        if (busOperator.Status != OperatorStatus.Active) busOperator.Approve();
        await db.SaveChangesAsync();

        if (!await db.OperatorMemberships.AnyAsync(x => x.OperatorId == busOperator.Id && x.UserId == owner.Id))
            db.OperatorMemberships.Add(new OperatorMembership(busOperator.Id, owner.Id, OperatorMemberRole.Owner));

        var limousine = await EnsureTemplateAsync(db, busOperator.Id, "Limousine 20 chỗ · Phương Trang", "Limousine", 1, 20);
        var driver = await db.Drivers.SingleOrDefaultAsync(x => x.OperatorId == busOperator.Id && x.LicenseNumber == "FUTA-B2-01");
        if (driver is null)
        {
            driver = new Driver(busOperator.Id, "Trần Minh Trí", "0911000201", "FUTA-B2-01");
            db.Drivers.Add(driver);
        }
        var vehicle = await db.Vehicles.SingleOrDefaultAsync(x => x.OperatorId == busOperator.Id && x.PlateNumber == "51B-FUTA.01");
        if (vehicle is null)
        {
            vehicle = new Vehicle(busOperator.Id, limousine.Id, "51B-FUTA.01", "Limousine");
            db.Vehicles.Add(vehicle);
        }
        await db.SaveChangesAsync();

        var route = routes[("SMT-HCM", "SMT-CTO")];
        var departureUtc = new DateTimeOffset(today.AddDays(1).ToDateTime(new TimeOnly(8, 0)), TimeSpan.FromHours(7)).UtcDateTime;

        // Nếu trước đó chuyến Sài Gòn - Cần Thơ từng bị gán nhầm vào SMT Express, di chuyển quyền sở hữu về đúng Phương Trang
        var existingTrip = await db.Trips.Include(t => t.Seats).SingleOrDefaultAsync(x => x.RouteId == route.Id && x.DepartureTimeUtc == departureUtc);
        if (existingTrip is not null && existingTrip.OperatorId != busOperator.Id)
        {
            db.Entry(existingTrip).Property(t => t.OperatorId).CurrentValue = busOperator.Id;
            db.Entry(existingTrip).Property(t => t.VehicleId).CurrentValue = vehicle.Id;
            db.Entry(existingTrip).Property(t => t.DriverId).CurrentValue = driver.Id;
            await db.SaveChangesAsync();
        }
        else if (existingTrip is null)
        {
            var duration = TimeSpan.FromHours(route.EstimatedHours);
            var trip = new Trip(busOperator.Id, route.Id, vehicle.Id, driver.Id, departureUtc, departureUtc.Add(duration), 210_000m);
            db.Trips.Add(trip);
            await db.SaveChangesAsync();

            var templateSeats = await db.SeatTemplateSeats.AsNoTracking()
                .Where(x => x.SeatTemplateId == limousine.Id)
                .OrderBy(x => x.Deck).ThenBy(x => x.Number).ToListAsync();
            foreach (var seat in templateSeats)
                db.Seats.Add(new Seat(trip.Id, seat.SeatCode, seat.Deck, seat.Row, seat.Column));
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedHoangLongOperatorAsync(SmartTicketDbContext db,
        UserManager<ApplicationUser> users, RoleManager<IdentityRole<Guid>> roles,
        Dictionary<(string From, string To), BusRoute> routes, DateOnly today, Guid smtOperatorId)
    {
        var owner = await EnsureUserAsync(users, roles, "hoanglong@smartticket.vn", "SmartTicket@2026", "OPERATOR_OWNER");
        var busOperator = await db.BusOperators.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.TaxCode == "0200424568");
        if (busOperator is null)
        {
            busOperator = new BusOperator(owner.Id, "Vận tải Hoàng Long", "0200424568", "1900 1207", "demo/hoanglong-license.pdf");
            db.BusOperators.Add(busOperator);
        }
        if (busOperator.Status != OperatorStatus.Active) busOperator.Approve();
        await db.SaveChangesAsync();

        if (!await db.OperatorMemberships.AnyAsync(x => x.OperatorId == busOperator.Id && x.UserId == owner.Id))
            db.OperatorMemberships.Add(new OperatorMembership(busOperator.Id, owner.Id, OperatorMemberRole.Owner));

        var sleeper = await EnsureTemplateAsync(db, busOperator.Id, "Giường nằm 40 chỗ · Hoàng Long", "Giường nằm", 2, 20);
        var driver = await db.Drivers.SingleOrDefaultAsync(x => x.OperatorId == busOperator.Id && x.LicenseNumber == "HL-B2-01");
        if (driver is null)
        {
            driver = new Driver(busOperator.Id, "Lê Hoàng Long", "0911000301", "HL-B2-01");
            db.Drivers.Add(driver);
        }
        var vehicle = await db.Vehicles.SingleOrDefaultAsync(x => x.OperatorId == busOperator.Id && x.PlateNumber == "29B-HL.001");
        if (vehicle is null)
        {
            vehicle = new Vehicle(busOperator.Id, sleeper.Id, "29B-HL.001", "Giường nằm");
            db.Vehicles.Add(vehicle);
        }
        await db.SaveChangesAsync();

        var route = routes[("SMT-HAN", "SMT-HUE")];
        var departureUtc = new DateTimeOffset(today.AddDays(1).ToDateTime(new TimeOnly(18, 45)), TimeSpan.FromHours(7)).UtcDateTime;

        // Nếu trước đó chuyến Hà Nội - Huế từng bị gán nhầm vào SMT Express, di chuyển quyền sở hữu về đúng Hoàng Long
        var existingTrip = await db.Trips.Include(t => t.Seats).SingleOrDefaultAsync(x => x.RouteId == route.Id && x.DepartureTimeUtc == departureUtc);
        if (existingTrip is not null && existingTrip.OperatorId != busOperator.Id)
        {
            db.Entry(existingTrip).Property(t => t.OperatorId).CurrentValue = busOperator.Id;
            db.Entry(existingTrip).Property(t => t.VehicleId).CurrentValue = vehicle.Id;
            db.Entry(existingTrip).Property(t => t.DriverId).CurrentValue = driver.Id;
            await db.SaveChangesAsync();
        }
        else if (existingTrip is null)
        {
            var duration = TimeSpan.FromHours(route.EstimatedHours);
            var trip = new Trip(busOperator.Id, route.Id, vehicle.Id, driver.Id, departureUtc, departureUtc.Add(duration), 590_000m);
            db.Trips.Add(trip);
            await db.SaveChangesAsync();

            var templateSeats = await db.SeatTemplateSeats.AsNoTracking()
                .Where(x => x.SeatTemplateId == sleeper.Id)
                .OrderBy(x => x.Deck).ThenBy(x => x.Number).ToListAsync();
            foreach (var seat in templateSeats)
                db.Seats.Add(new Seat(trip.Id, seat.SeatCode, seat.Deck, seat.Row, seat.Column));
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedDucQuanOperatorAsync(SmartTicketDbContext db,
        UserManager<ApplicationUser> users, RoleManager<IdentityRole<Guid>> roles, IConfiguration configuration)
    {
        var owner = await EnsureUserAsync(users, roles,
            configuration["DemoData:DucQuanEmail"] ?? "ducquan@smartticket.vn",
            configuration["DemoData:DucQuanPassword"] ?? "SmartTicket@2026", "OPERATOR_OWNER");
        var legacyOwner = await EnsureUserAsync(users, roles, "duc.quan@smartticket.local", "DucQuan!2026", "OPERATOR_OWNER");

        var stationFrom = await db.Stations.SingleOrDefaultAsync(x => x.Code == "TPTN_01");
        var stationTo = await db.Stations.SingleOrDefaultAsync(x => x.Code == "SMT-HAN");
        if (stationFrom is null || stationTo is null)
            throw new InvalidOperationException("Không tìm thấy bến Thái Nguyên hoặc bến Mỹ Đình để tạo chuyến demo Đức Quân.");

        var route = await db.Routes.IgnoreQueryFilters().SingleOrDefaultAsync(x =>
            x.DepartureStationId == stationFrom.Id && x.ArrivalStationId == stationTo.Id);
        if (route is null)
        {
            route = new BusRoute(stationFrom.Id, stationTo.Id, 75m, 2);
            db.Routes.Add(route);
        }
        else if (!route.IsActive) route.Reactivate();

        var busOperator = await db.BusOperators.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.TaxCode == "9999999902");
        if (busOperator is null)
        {
            busOperator = new BusOperator(owner.Id, "Nhà xe Đức Quân", "9999999902", "0208 555 0199", "demo/duc-quan-license.pdf");
            db.BusOperators.Add(busOperator);
        }
        if (busOperator.Status != OperatorStatus.Active) busOperator.Approve();
        await db.SaveChangesAsync();

        if (!await db.OperatorMemberships.AnyAsync(x => x.OperatorId == busOperator.Id && x.UserId == owner.Id))
            db.OperatorMemberships.Add(new OperatorMembership(busOperator.Id, owner.Id, OperatorMemberRole.Owner));
        if (!await db.OperatorMemberships.AnyAsync(x => x.OperatorId == busOperator.Id && x.UserId == legacyOwner.Id))
            db.OperatorMemberships.Add(new OperatorMembership(busOperator.Id, legacyOwner.Id, OperatorMemberRole.Owner));

        var template = await EnsureTemplateAsync(db, busOperator.Id, "Limousine 16 chỗ · Đức Quân", "Limousine", 1, 16);
        var driver = await db.Drivers.SingleOrDefaultAsync(x => x.OperatorId == busOperator.Id && x.LicenseNumber == "DQ-DEMO-B2-01");
        if (driver is null)
        {
            driver = new Driver(busOperator.Id, "Nguyễn Văn Đức", "0987654321", "DQ-DEMO-B2-01");
            db.Drivers.Add(driver);
        }
        var vehicle = await db.Vehicles.SingleOrDefaultAsync(x => x.OperatorId == busOperator.Id && x.PlateNumber == "20B-DQ-001.00");
        if (vehicle is null)
        {
            vehicle = new Vehicle(busOperator.Id, template.Id, "20B-DQ-001.00", "Limousine");
            db.Vehicles.Add(vehicle);
        }
        await db.SaveChangesAsync();

        var tomorrow = DateOnly.FromDateTime(DateTime.Now).AddDays(1);
        var departureUtc = new DateTimeOffset(tomorrow.ToDateTime(new TimeOnly(7, 30)), TimeSpan.FromHours(7)).UtcDateTime;
        var trip = await db.Trips.SingleOrDefaultAsync(x => x.OperatorId == busOperator.Id && x.RouteId == route.Id && x.DepartureTimeUtc == departureUtc);
        if (trip is null)
        {
            trip = new Trip(busOperator.Id, route.Id, vehicle.Id, driver.Id, departureUtc,
                departureUtc.AddHours(Math.Max(route.EstimatedHours, 2)), 120_000m);
            db.Trips.Add(trip);
            await db.SaveChangesAsync();
        }

        var templateSeats = await db.SeatTemplateSeats.AsNoTracking().Where(x => x.SeatTemplateId == template.Id).ToListAsync();
        var existingSeatCodes = (await db.Seats.Where(x => x.TripId == trip.Id).Select(x => x.SeatCode).ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var seat in templateSeats.Where(x => !existingSeatCodes.Contains(x.SeatCode)))
            db.Seats.Add(new Seat(trip.Id, seat.SeatCode, seat.Deck, seat.Row, seat.Column));
        await db.SaveChangesAsync();
    }

    private static async Task<ApplicationUser> EnsureUserAsync(UserManager<ApplicationUser> users,
        RoleManager<IdentityRole<Guid>> roles, string email, string password, string role, string? fullName = null, string? phone = null)
    {
        if (!await roles.RoleExistsAsync(role))
        {
            var roleResult = await roles.CreateAsync(new IdentityRole<Guid>(role));
            if (!roleResult.Succeeded) throw new InvalidOperationException($"Không tạo được role {role}: {string.Join("; ", roleResult.Errors.Select(x => x.Description))}");
        }

        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser { Id = Guid.NewGuid(), UserName = email, Email = email, FullName = fullName, PhoneNumber = phone, EmailConfirmed = true, IsActive = true };
            var create = await users.CreateAsync(user, password);
            if (!create.Succeeded) throw new InvalidOperationException($"Không tạo được tài khoản demo {email}: {string.Join("; ", create.Errors.Select(x => x.Description))}");
        }
        else
        {
            var isDirty = false;
            if (!user.IsActive || !user.EmailConfirmed || (fullName != null && user.FullName != fullName) || (phone != null && user.PhoneNumber != phone))
            {
                user.IsActive = true;
                user.EmailConfirmed = true;
                if (fullName != null) user.FullName = fullName;
                if (phone != null) user.PhoneNumber = phone;
                isDirty = true;
            }
            if (isDirty)
            {
                var update = await users.UpdateAsync(user);
                if (!update.Succeeded) throw new InvalidOperationException($"Không cập nhật được tài khoản demo {email}.");
            }

            if (!await users.CheckPasswordAsync(user, password))
            {
                var resetToken = await users.GeneratePasswordResetTokenAsync(user);
                await users.ResetPasswordAsync(user, resetToken, password);
            }
        }

        if (!await users.IsInRoleAsync(user, role))
        {
            var addRole = await users.AddToRoleAsync(user, role);
            if (!addRole.Succeeded) throw new InvalidOperationException($"Không gán được role {role} cho {email}.");
        }
        return user;
    }

    private static async Task<SeatTemplate> EnsureTemplateAsync(SmartTicketDbContext db, Guid operatorId,
        string name, string type, int decks, int seatsPerDeck)
    {
        var template = await db.SeatTemplates.Include(x => x.Seats)
            .SingleOrDefaultAsync(x => x.OperatorId == operatorId && x.Name == name);
        if (template is not null) return template;
        template = new SeatTemplate(operatorId, name, type, decks, seatsPerDeck);
        db.SeatTemplates.Add(template);
        await db.SaveChangesAsync();
        return template;
    }

    private static async Task<Driver[]> EnsureDriversAsync(SmartTicketDbContext db, Guid operatorId)
    {
        var seeds = new[] { (Name: "Nguyễn Minh An", Phone: "0900000101", License: "SMT-DEMO-B2-01"), (Name: "Trần Quốc Bình", Phone: "0900000102", License: "SMT-DEMO-B2-02") };
        var result = new List<Driver>();
        foreach (var seed in seeds)
        {
            var driver = await db.Drivers.SingleOrDefaultAsync(x => x.OperatorId == operatorId && x.LicenseNumber == seed.License);
            if (driver is null) { driver = new Driver(operatorId, seed.Name, seed.Phone, seed.License); db.Drivers.Add(driver); }
            result.Add(driver);
        }
        await db.SaveChangesAsync();
        return result.ToArray();
    }

    private static async Task<Vehicle[]> EnsureVehiclesAsync(SmartTicketDbContext db, Guid operatorId, Guid sleeperTemplateId, Guid limousineTemplateId)
    {
        var seeds = new[] { (Plate: "51B-DEMO1", Type: "Giường nằm", Template: sleeperTemplateId), (Plate: "51B-DEMO2", Type: "Limousine", Template: limousineTemplateId) };
        var result = new List<Vehicle>();
        foreach (var seed in seeds)
        {
            var vehicle = await db.Vehicles.SingleOrDefaultAsync(x => x.OperatorId == operatorId && x.PlateNumber == seed.Plate);
            if (vehicle is null) { vehicle = new Vehicle(operatorId, seed.Template, seed.Plate, seed.Type); db.Vehicles.Add(vehicle); }
            result.Add(vehicle);
        }
        await db.SaveChangesAsync();
        return result.ToArray();
    }

    private static string CreateDemoCheckInJwt(Booking booking, Guid seatId, string seatCode, Guid tripId, DateTime departureTimeUtc, string signingKey)
    {
        static string B64(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var header = B64(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));
        var expiresUtc = departureTimeUtc.AddDays(1) > DateTime.UtcNow.AddDays(2) ? departureTimeUtc.AddDays(1) : DateTime.UtcNow.AddDays(2);
        var claims = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new
        {
            sub = seatId.ToString("N"),
            booking = booking.BookingCode,
            trip = tripId,
            seat = seatCode,
            exp = new DateTimeOffset(expiresUtc, TimeSpan.Zero).ToUnixTimeSeconds()
        });
        var body = header + "." + B64(claims);
        var signature = System.Security.Cryptography.HMACSHA256.HashData(System.Text.Encoding.UTF8.GetBytes(signingKey), System.Text.Encoding.ASCII.GetBytes(body));
        return body + "." + B64(signature);
    }
}
