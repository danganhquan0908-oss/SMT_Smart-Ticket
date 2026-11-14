using BusGoSmartTicket.Data;
using BusGoSmartTicket.Models.Entities;
using BusGoSmartTicket.Models.ViewModels;
using BusGoSmartTicket.Services.Interfaces.Account;
using BusGoSmartTicket.Services.Interfaces.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusGoSmartTicket.Controllers;

[AllowAnonymous, Route("trips")]
public sealed class TripsController(
    IBookingService bookings,
    ApplicationDbContext db,
    IConfiguration configuration,
    UserManager<ApplicationUser> userManager,
    IMembershipService membership) : Controller
{
    [HttpGet("{tripId:guid}/seats")]
    public async Task<IActionResult> Seats(Guid tripId, CancellationToken ct)
    {
        var trip = await db.Trips.AsNoTracking()
            .Include(t => t.Route).ThenInclude(r => r!.DepartureStation)
            .Include(t => t.Route).ThenInclude(r => r!.ArrivalStation)
            .Include(t => t.Operator).Include(t => t.Vehicle)
            .SingleOrDefaultAsync(t => t.Id == tripId, ct);

        if (trip is null)
            return NotFound();
        if (trip.Status != TripStatus.Scheduled || trip.DepartureTimeUtc <= DateTime.UtcNow)
        {
            TempData["Error"] = "Chuyến xe đã khởi hành hoặc không còn mở bán.";
            return RedirectToAction("Index", "Home");
        }

        var customer = User.Identity?.IsAuthenticated == true ? await userManager.GetUserAsync(User) : null;
        var quote = await membership.QuoteAsync(customer?.Id, trip.TicketPrice, ct);
        var model = new SeatSelectionViewModel
        {
            TripId = trip.Id,
            TicketPrice = quote.NetSeatPrice,
            DiscountName = quote.Name,
            OperatorName = trip.Operator?.CompanyName ?? "Nhà xe đối tác",
            RouteName = $"{trip.Route?.DepartureStation?.Name} → {trip.Route?.ArrivalStation?.Name}",
            DepartureTimeUtc = trip.DepartureTimeUtc,
            VehicleType = trip.Vehicle?.VehicleType ?? "Xe khách",
            Seats = await bookings.GetSeatsAsync(tripId, ct),
            CustomerName = customer?.FullName ?? customer?.UserName ?? "",
            CustomerPhone = customer?.PhoneNumber ?? "",
            CustomerEmail = customer?.Email ?? "",
            Bank = new PaymentBankViewModel
            {
                Bin = configuration["Payments:BankBin"] ?? "",
                Name = configuration["Payments:BankName"] ?? "",
                AccountNumber = configuration["Payments:AccountNumber"] ?? "",
                AccountName = configuration["Payments:AccountName"] ?? ""
            }
        };
        return View(model);
    }
}
