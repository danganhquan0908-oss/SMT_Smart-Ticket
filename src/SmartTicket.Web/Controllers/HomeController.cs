using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartTicket.Application.Catalog;
using SmartTicket.Web.Models;
using SmartTicket.Application.Bookings;

namespace SmartTicket.Web.Controllers;

public sealed class HomeController(ICatalogService catalog, IBookingService bookings) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> Index(Guid? departureStationId, Guid? arrivalStationId, DateOnly? travelDate, CancellationToken ct)
    {
        var date = travelDate ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var stations = await catalog.GetStationsAsync(false, ct);
        var model = new TripSearchViewModel { Stations = stations, DepartureStationId = departureStationId, ArrivalStationId = arrivalStationId, TravelDate = date };
        if (departureStationId.HasValue && arrivalStationId.HasValue)
        {
            try { model = new TripSearchViewModel { Stations = stations, DepartureStationId = departureStationId, ArrivalStationId = arrivalStationId, TravelDate = date, Results = await bookings.SearchAsync(new TripSearchQuery(departureStationId.Value, arrivalStationId.Value, date), ct) }; }
            catch (BookingConflictException ex) { model = new TripSearchViewModel { Stations = stations, DepartureStationId = departureStationId, ArrivalStationId = arrivalStationId, TravelDate = date, Error = ex.Message }; }
        }
        return View(model);
    }

    [AllowAnonymous]
    public IActionResult Error() => View("~/Views/Shared/Error.cshtml");
}
