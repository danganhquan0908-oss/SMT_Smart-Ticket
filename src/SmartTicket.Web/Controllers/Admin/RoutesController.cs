using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTicket.Application.Catalog;

namespace SmartTicket.Web.Controllers.Admin;

[Authorize(Roles = "SUPER_ADMIN")]
[Route("admin/routes")]
public sealed class RoutesController(ICatalogService catalog) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct) => View("~/Views/Admin/Routes/Index.cshtml", await catalog.GetRoutesAsync(true, ct));

    [HttpGet("new")]
    public async Task<IActionResult> Create(CancellationToken ct) { await LoadStations(ct); return View("~/Views/Admin/Routes/Edit.cshtml", new SaveBusRouteRequest()); }

    [HttpPost("new"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SaveBusRouteRequest request, CancellationToken ct)
    {
        if (request.DepartureStationId == request.ArrivalStationId) ModelState.AddModelError(nameof(request.ArrivalStationId), "Bến đi và bến đến phải khác nhau.");
        if (!ModelState.IsValid) { await LoadStations(ct); return View("~/Views/Admin/Routes/Edit.cshtml", request); }
        try { await catalog.CreateRouteAsync(request, ct); TempData["Success"] = "Đã tạo tuyến đường."; return RedirectToAction(nameof(Index)); }
        catch (CatalogConflictException ex) { ModelState.AddModelError(string.Empty, ex.Message); await LoadStations(ct); return View("~/Views/Admin/Routes/Edit.cshtml", request); }
    }

    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var route = (await catalog.GetRoutesAsync(true, ct)).SingleOrDefault(x => x.Id == id);
        if (route is null) return NotFound();
        await LoadStations(ct);
        ViewData["RouteId"] = route.Id;
        return View("~/Views/Admin/Routes/Edit.cshtml", new SaveBusRouteRequest { DepartureStationId = route.DepartureStationId, ArrivalStationId = route.ArrivalStationId, DistanceKm = route.DistanceKm, EstimatedHours = route.EstimatedHours });
    }

    [HttpPost("{id:guid}/edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, SaveBusRouteRequest request, CancellationToken ct)
    {
        if (request.DepartureStationId == request.ArrivalStationId) ModelState.AddModelError(nameof(request.ArrivalStationId), "Bến đi và bến đến phải khác nhau.");
        if (!ModelState.IsValid) { ViewData["RouteId"] = id; await LoadStations(ct); return View("~/Views/Admin/Routes/Edit.cshtml", request); }
        try { await catalog.UpdateRouteAsync(id, request, ct); TempData["Success"] = "Đã cập nhật tuyến đường."; return RedirectToAction(nameof(Index)); }
        catch (CatalogConflictException ex) { ModelState.AddModelError(string.Empty, ex.Message); ViewData["RouteId"] = id; await LoadStations(ct); return View("~/Views/Admin/Routes/Edit.cshtml", request); }
        catch (CatalogNotFoundException) { return NotFound(); }
    }

    [HttpPost("{id:guid}/deactivate"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct) => await ChangeStatus(id, false, ct);

    [HttpPost("{id:guid}/reactivate"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct) => await ChangeStatus(id, true, ct);

    private async Task<IActionResult> ChangeStatus(Guid id, bool active, CancellationToken ct)
    {
        try
        {
            if (active) await catalog.ReactivateRouteAsync(id, ct); else await catalog.DeactivateRouteAsync(id, ct);
            TempData["Success"] = active ? "Đã kích hoạt lại tuyến đường." : "Đã ngừng hoạt động tuyến đường.";
        }
        catch (CatalogConflictException ex) { TempData["Error"] = ex.Message; }
        catch (CatalogNotFoundException) { return NotFound(); }
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadStations(CancellationToken ct) => ViewBag.Stations = (await catalog.GetStationsAsync(false, ct)).Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(x.Name + " — " + x.Province, x.Id.ToString())).ToList();
}
