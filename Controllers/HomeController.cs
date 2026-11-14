using BusGoSmartTicket.Exceptions.Bookings;
using BusGoSmartTicket.Models.DTOs.Bookings;
using BusGoSmartTicket.Models.ViewModels;
using BusGoSmartTicket.Services.Bookings;
using BusGoSmartTicket.Services.Interfaces.Bookings;
using BusGoSmartTicket.Services.Interfaces.Catalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BusGoSmartTicket.Controllers;

public sealed class HomeController(ICatalogService catalog, IBookingService bookings) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> Index(Guid? departureStationId, Guid? arrivalStationId, DateOnly? travelDate,
        string? vehicle, string? company, string? period, string? sort, bool availableOnly = false, int page = 1, CancellationToken ct = default)
    {
        var date = travelDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7).AddDays(1));
        var stations = await catalog.GetStationsAsync(false, ct);
        IReadOnlyList<TripSearchResult> results = [];
        string? error = null;
        if (departureStationId.HasValue && arrivalStationId.HasValue)
        {
            try
            {
                results = await bookings.SearchAsync(new(departureStationId.Value, arrivalStationId.Value, date), ct);
            }
            catch (BookingConflictException ex) { error = ex.Message; }
        }
        var companies = results.Select(x => x.Operator).Distinct().Order().ToList();
        var filtered = TripSearchFiltering.Apply(results, vehicle, company, period, sort, availableOnly).ToList();
        var total = filtered.Count;
        page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(total / 8m)));
        return View(new TripSearchViewModel
        {
            Stations = stations,
            Results = filtered.Skip((page - 1) * 8).Take(8).ToList(),
            DepartureStationId = departureStationId,
            ArrivalStationId = arrivalStationId,
            TravelDate = date,
            Error = error,
            Vehicle = vehicle,
            Company = company,
            Period = period,
            Sort = sort,
            AvailableOnly = availableOnly,
            Page = page,
            TotalResults = total,
            Companies = companies
        });
    }
    [AllowAnonymous] public IActionResult Error() => View("~/Views/Shared/Error.cshtml");
}
