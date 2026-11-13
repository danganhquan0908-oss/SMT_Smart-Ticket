using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTicket.Application.Bookings;
using SmartTicket.Infrastructure.Persistence;

namespace SmartTicket.Web.Controllers;

[AllowAnonymous, Route("trips")]
public sealed class TripsController(
    IBookingService bookings, 
    SmartTicketDbContext db, 
    IConfiguration configuration, 
    IHostEnvironment environment,
    UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet("{tripId:guid}/seats")]
    public async Task<IActionResult> Seats(Guid tripId, CancellationToken ct)
    {
        var trip = await db.Trips
            .Include(t => t.Route).ThenInclude(r => r!.DepartureStation)
            .Include(t => t.Route).ThenInclude(r => r!.ArrivalStation)
            .Include(t => t.Operator)
            .Include(t => t.Vehicle)
            .SingleOrDefaultAsync(t => t.Id == tripId, ct);

        if (trip == null) return NotFound();

        if (trip.Status != SmartTicket.Domain.Entities.TripStatus.Scheduled)
        {
            TempData["Error"] = "Chuyến xe này không còn mở bán vé trực tuyến.";
            return RedirectToAction("Index", "Home");
        }

        if (trip.DepartureTimeUtc <= DateTime.UtcNow)
        {
            if (environment.IsDevelopment())
            {
                var duration = trip.ArrivalTimeUtc > trip.DepartureTimeUtc ? trip.ArrivalTimeUtc - trip.DepartureTimeUtc : TimeSpan.FromHours(4);
                var newDep = DateTime.UtcNow.AddHours(24);
                trip.UpdateSchedule(trip.RouteId, trip.VehicleId, trip.DriverId, newDep, newDep.Add(duration), false);
                await db.SaveChangesAsync(ct);
            }
            else
            {
                TempData["Error"] = "Chuyến xe này đã khởi hành hoặc không còn mở bán vé trực tuyến.";
                return RedirectToAction("Index", "Home");
            }
        }

        var seats = await bookings.GetSeatsAsync(tripId, ct);
        ViewBag.TripId = tripId;
        ViewBag.Trip = trip;
        ViewBag.TicketPrice = trip.TicketPrice;
        ViewBag.OperatorName = trip.Operator?.CompanyName ?? "Smart Ticket Express";
        ViewBag.RouteName = $"{trip.Route?.DepartureStation?.Name} → {trip.Route?.ArrivalStation?.Name}";
        ViewBag.DepartureTimeUtc = trip.DepartureTimeUtc;
        ViewBag.VehicleType = trip.Vehicle?.VehicleType ?? "Limousine 34 phòng";
        ViewBag.SeatsJson = System.Text.Json.JsonSerializer.Serialize(seats, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));

        ViewBag.BankBin = configuration["Payments:BankBin"] ?? "";
        ViewBag.BankName = configuration["Payments:BankName"] ?? "";
        ViewBag.AccountNumber = configuration["Payments:AccountNumber"] ?? "";
        ViewBag.AccountName = configuration["Payments:AccountName"] ?? "";
        ViewBag.IsDev = environment.IsDevelopment();

        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await userManager.GetUserAsync(User);
            if (user is not null)
            {
                ViewBag.CustomerName = user.FullName ?? user.UserName ?? "";
                ViewBag.CustomerPhone = user.PhoneNumber ?? "";
                ViewBag.CustomerEmail = user.Email ?? "";
            }
        }

        return View("~/Views/Trips/Seats.cshtml");
    }
}
