using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartTicket.Application.Catalog;
using SmartTicket.Application.Operators;
using SmartTicket.Web.Models;

namespace SmartTicket.Web.Controllers;

[Authorize(Roles = "OPERATOR_OWNER,OPERATOR_DISPATCHER")]
[Route("operator")]
public sealed class OperatorPortalController(IOperatorService operators, ICatalogService catalog, IActivationEmailSender emailSender) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var userId = CurrentUserId();
        await PopulateOperatorProfileAsync(userId, ct);
        ViewBag.TemplateCount = (await operators.GetSeatTemplatesAsync(userId, ct)).Count;
        ViewBag.VehicleCount = (await operators.GetVehiclesAsync(userId, ct)).Count;
        ViewBag.DriverCount = (await operators.GetDriversAsync(userId, ct)).Count;
        ViewBag.TripCount = (await operators.GetTripsAsync(userId, ct)).Count;
        return View("~/Views/Operators/Index.cshtml");
    }

    [HttpGet("seat-templates")]
    public async Task<IActionResult> SeatTemplates(CancellationToken ct) => View("~/Views/Operators/SeatTemplates.cshtml", await operators.GetSeatTemplatesAsync(CurrentUserId(), ct));

    [HttpPost("seat-templates"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSeatTemplate(SeatTemplateRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View("~/Views/Operators/SeatTemplates.cshtml", await operators.GetSeatTemplatesAsync(CurrentUserId(), ct));
        try { await operators.CreateSeatTemplateAsync(CurrentUserId(), request, ct); TempData["Success"] = "Đã tạo mẫu sơ đồ ghế."; }
        catch (OperatorConflictException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(SeatTemplates));
    }

    [HttpGet("vehicles")]
    public async Task<IActionResult> Vehicles(CancellationToken ct) { await LoadTemplates(ct); return View("~/Views/Operators/Vehicles.cshtml", await operators.GetVehiclesAsync(CurrentUserId(), ct)); }

    [HttpPost("vehicles"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateVehicle(VehicleRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) { await LoadTemplates(ct); return View("~/Views/Operators/Vehicles.cshtml", await operators.GetVehiclesAsync(CurrentUserId(), ct)); }
        try { await operators.CreateVehicleAsync(CurrentUserId(), request, ct); TempData["Success"] = "Đã thêm xe."; }
        catch (OperatorConflictException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Vehicles));
    }

    [HttpPost("vehicles/{id:guid}/edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditVehicle(Guid id, VehicleRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) { TempData["Error"] = "Thông tin xe không hợp lệ."; return RedirectToAction(nameof(Vehicles)); }
        try { await operators.UpdateVehicleAsync(CurrentUserId(), id, request, ct); TempData["Success"] = "Đã cập nhật thông tin xe."; }
        catch (OperatorConflictException ex) { TempData["Error"] = ex.Message; }
        catch (OperatorNotFoundException) { return NotFound(); }
        return RedirectToAction(nameof(Vehicles));
    }

    [HttpPost("vehicles/{id:guid}/deactivate"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateVehicle(Guid id, CancellationToken ct) => await ChangeStatus(() => operators.DeactivateVehicleAsync(CurrentUserId(), id, ct), nameof(Vehicles));

    [HttpGet("drivers")]
    public async Task<IActionResult> Drivers(CancellationToken ct) => View("~/Views/Operators/Drivers.cshtml", await operators.GetDriversAsync(CurrentUserId(), ct));

    [HttpPost("drivers"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDriver(DriverRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View("~/Views/Operators/Drivers.cshtml", await operators.GetDriversAsync(CurrentUserId(), ct));
        try { await operators.CreateDriverAsync(CurrentUserId(), request, ct); TempData["Success"] = "Đã thêm tài xế."; }
        catch (OperatorConflictException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Drivers));
    }

    [HttpPost("drivers/{id:guid}/edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDriver(Guid id, DriverRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) { TempData["Error"] = "Thông tin tài xế không hợp lệ."; return RedirectToAction(nameof(Drivers)); }
        try { await operators.UpdateDriverAsync(CurrentUserId(), id, request, ct); TempData["Success"] = "Đã cập nhật thông tin tài xế."; }
        catch (OperatorConflictException ex) { TempData["Error"] = ex.Message; }
        catch (OperatorNotFoundException) { return NotFound(); }
        return RedirectToAction(nameof(Drivers));
    }

    [HttpPost("drivers/{id:guid}/deactivate"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateDriver(Guid id, CancellationToken ct) => await ChangeStatus(() => operators.DeactivateDriverAsync(CurrentUserId(), id, ct), nameof(Drivers));

    [HttpGet("trips")]
    public async Task<IActionResult> Trips(CancellationToken ct)
    {
        var userId = CurrentUserId();
        await PopulateOperatorProfileAsync(userId, ct);
        return View("~/Views/Operators/Trips.cshtml", await operators.GetTripsAsync(userId, ct));
    }

    [HttpGet("trips/new")]
    public async Task<IActionResult> CreateTrip(CancellationToken ct) { await LoadTripAssets(ct); return View("~/Views/Operators/TripEdit.cshtml", new OperatorTripForm { DepartureTime = LocalNow().AddDays(1), ArrivalTime = LocalNow().AddDays(1).AddHours(6) }); }

    [HttpPost("trips/new"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTrip(OperatorTripForm form, CancellationToken ct)
    {
        if (!ModelState.IsValid) { await LoadTripAssets(ct); return View("~/Views/Operators/TripEdit.cshtml", form); }
        try
        {
            await operators.CreateTripAsync(CurrentUserId(), ToRequest(form), ct);
            TempData["Success"] = "Đã tạo chuyến xe.";
            return RedirectToAction(nameof(Trips));
        }
        catch (OperatorConflictException ex) { ModelState.AddModelError(string.Empty, ex.Message); await LoadTripAssets(ct); return View("~/Views/Operators/TripEdit.cshtml", form); }
        catch (ArgumentException ex) { ModelState.AddModelError(string.Empty, ex.Message); await LoadTripAssets(ct); return View("~/Views/Operators/TripEdit.cshtml", form); }
    }

    [HttpGet("trips/{id:guid}/edit")]
    public async Task<IActionResult> EditTrip(Guid id, CancellationToken ct)
    {
        var trip = (await operators.GetTripsAsync(CurrentUserId(), ct)).SingleOrDefault(x => x.Id == id);
        if (trip is null) return NotFound();
        ViewData["TripId"] = id;
        ViewBag.SoldTicketCount = trip.SoldTicketCount;
        await LoadTripAssets(ct);
        return View("~/Views/Operators/TripEdit.cshtml", new OperatorTripForm { RouteId = trip.RouteId, VehicleId = trip.VehicleId, DriverId = trip.DriverId, DepartureTime = FromUtc(trip.DepartureTimeUtc), ArrivalTime = FromUtc(trip.ArrivalTimeUtc), TicketPrice = trip.TicketPrice });
    }

    [HttpPost("trips/{id:guid}/edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTrip(Guid id, OperatorTripForm form, CancellationToken ct)
    {
        ViewData["TripId"] = id;
        if (!ModelState.IsValid) { await LoadTripAssets(ct); return View("~/Views/Operators/TripEdit.cshtml", form); }
        try
        {
            await operators.UpdateTripScheduleAsync(CurrentUserId(), id, ToScheduleRequest(form), ct);
            TempData["Success"] = "Đã cập nhật thông tin chuyến.";
            return RedirectToAction(nameof(Trips));
        }
        catch (Exception ex) when (ex is OperatorConflictException or OperatorNotFoundException or InvalidOperationException or ArgumentException)
        { ModelState.AddModelError(string.Empty, ex.Message); await LoadTripAssets(ct); return View("~/Views/Operators/TripEdit.cshtml", form); }
    }

    [HttpPost("trips/{id:guid}/price"), ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePrice(Guid id, PriceChangeRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) { TempData["Error"] = "Giá vé không hợp lệ."; return RedirectToAction(nameof(Trips)); }
        try { await operators.ChangeTripPriceAsync(CurrentUserId(), id, request.NewPrice, ct); TempData["Success"] = "Đã cập nhật giá; thay đổi đã ghi audit log."; }
        catch (OperatorConflictException ex) { TempData["Error"] = ex.Message; }
        catch (OperatorNotFoundException) { return NotFound(); }
        return RedirectToAction(nameof(Trips));
    }

    [HttpGet("trips/{id:guid}/price-history")]
    public async Task<IActionResult> PriceHistory(Guid id, CancellationToken ct) => View("~/Views/Operators/PriceHistory.cshtml", await operators.GetPriceHistoryAsync(CurrentUserId(), id, ct));

    [HttpPost("dispatchers/invite"), Authorize(Roles = "OPERATOR_OWNER"), ValidateAntiForgeryToken]
    public async Task<IActionResult> InviteDispatcher(DispatcherInviteForm form, CancellationToken ct)
    {
        if (!ModelState.IsValid) { TempData["Error"] = "Email không hợp lệ."; return RedirectToAction(nameof(Index)); }
        try
        {
            var notice = await operators.InviteDispatcherAsync(CurrentUserId(), form.Email, ct);
            var link = Url.Action("Start", "OperatorActivation", new { token = notice.Token }, Request.Scheme)!;
            await emailSender.SendAsync(notice.Email, link, ct);
            TempData["Success"] = $"Đã gửi lời mời điều phối đến {notice.Email}.";
        }
        catch (OperatorConflictException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> ChangeStatus(Func<Task> action, string returnAction)
    {
        try { await action(); TempData["Success"] = "Đã cập nhật trạng thái."; }
        catch (OperatorConflictException ex) { TempData["Error"] = ex.Message; }
        catch (OperatorNotFoundException) { return NotFound(); }
        return RedirectToAction(returnAction);
    }

    private async Task PopulateOperatorProfileAsync(Guid userId, CancellationToken ct)
    {
        var profile = await operators.GetProfileAsync(userId, ct);
        ViewBag.OperatorName = profile?.CompanyName ?? "Doanh nghiệp vận tải";
        ViewBag.OperatorTaxCode = profile?.TaxCode ?? "";
    }

    private async Task LoadTemplates(CancellationToken ct) => ViewBag.Templates = (await operators.GetSeatTemplatesAsync(CurrentUserId(), ct)).Where(x => x.Id != Guid.Empty).Select(x => new SelectListItem($"{x.Name} · {x.DeckCount} tầng · {x.SeatsPerDeck * x.DeckCount} chỗ", x.Id.ToString())).ToList();

    private async Task LoadTripAssets(CancellationToken ct)
    {
        ViewBag.Routes = (await catalog.GetRoutesAsync(false, ct)).Select(x => new SelectListItem($"{x.DepartureStation} → {x.ArrivalStation}", x.Id.ToString())).ToList();
        ViewBag.Vehicles = (await operators.GetVehiclesAsync(CurrentUserId(), ct)).Where(x => x.IsActive).Select(x => new SelectListItem($"{x.PlateNumber} · {x.SeatCount} chỗ", x.Id.ToString())).ToList();
        ViewBag.Drivers = (await operators.GetDriversAsync(CurrentUserId(), ct)).Where(x => x.IsActive).Select(x => new SelectListItem(x.FullName, x.Id.ToString())).ToList();
    }

    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private static DateTime LocalNow() => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone());
    private static DateTime FromUtc(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), VietnamTimeZone());
    private static DateTime ToUtc(DateTime local) => local.Kind == DateTimeKind.Utc ? local : TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), VietnamTimeZone());
    private static TimeZoneInfo VietnamTimeZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
    }
    private static TripRequest ToRequest(OperatorTripForm form) => new()
    {
        RouteId = form.RouteId, VehicleId = form.VehicleId, DriverId = form.DriverId,
        DepartureTimeUtc = ToUtc(form.DepartureTime), ArrivalTimeUtc = ToUtc(form.ArrivalTime), TicketPrice = form.TicketPrice
    };
    private static TripScheduleRequest ToScheduleRequest(OperatorTripForm form) => new()
    {
        RouteId = form.RouteId, VehicleId = form.VehicleId, DriverId = form.DriverId,
        DepartureTimeUtc = ToUtc(form.DepartureTime), ArrivalTimeUtc = ToUtc(form.ArrivalTime)
    };
}
