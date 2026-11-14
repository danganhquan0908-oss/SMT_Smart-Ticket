using BusGoSmartTicket.Exceptions.Catalog;
using BusGoSmartTicket.Exceptions.Operations;
using BusGoSmartTicket.Models.DTOs.Catalog;
using BusGoSmartTicket.Services.Interfaces.Catalog;
using BusGoSmartTicket.Services.Interfaces.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusGoSmartTicket.Controllers.Admin;

[Authorize(Roles = "SUPER_ADMIN")]
[Route("admin/stations")]
public sealed class StationsController(ICatalogService catalog, IStationOperationsService operations) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct) => View("~/Views/Admin/Stations/Index.cshtml", await catalog.GetStationsAsync(true, ct));

    [HttpGet("new")]
    public IActionResult Create() => View("~/Views/Admin/Stations/Edit.cshtml", new SaveStationRequest());

    [HttpPost("new"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SaveStationRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Admin/Stations/Edit.cshtml", request);
        try
        {
            await catalog.CreateStationAsync(request, ct);
            TempData["Success"] = "Đã thêm bến xe.";
            return RedirectToAction(nameof(Index));
        }
        catch (CatalogConflictException ex) { ModelState.AddModelError(nameof(request.Code), ex.Message); return View("~/Views/Admin/Stations/Edit.cshtml", request); }
    }

    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var station = (await catalog.GetStationsAsync(true, ct)).SingleOrDefault(x => x.Id == id);
        if (station is null)
            return NotFound();
        ViewData["StationId"] = station.Id;
        return View("~/Views/Admin/Stations/Edit.cshtml", new SaveStationRequest { Code = station.Code, Name = station.Name, Province = station.Province, Address = station.Address });
    }

    [HttpPost("{id:guid}/edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, SaveStationRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewData["StationId"] = id;
            return View("~/Views/Admin/Stations/Edit.cshtml", request);
        }
        try
        {
            await catalog.UpdateStationAsync(id, request, ct);
            TempData["Success"] = "Đã cập nhật bến xe.";
            return RedirectToAction(nameof(Index));
        }
        catch (CatalogConflictException ex) { ModelState.AddModelError(nameof(request.Code), ex.Message); ViewData["StationId"] = id; return View("~/Views/Admin/Stations/Edit.cshtml", request); }
        catch (CatalogNotFoundException) { return NotFound(); }
    }

    [HttpPost("{id:guid}/deactivate"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct) => await ChangeStatus(id, false, ct);

    [HttpPost("{id:guid}/reactivate"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct) => await ChangeStatus(id, true, ct);

    [HttpPost("{id:guid}/members"), ValidateAntiForgeryToken]
    public async Task<IActionResult> AddStationMember(Guid id, string email, CancellationToken ct)
    {
        try
        {
            await operations.AssignStationMemberAsync(id, email, ct);
            TempData["Success"] = $"Đã phân công {email} tại bến.";
        }
        catch (OperationsConflictException ex) { TempData["Error"] = ex.Message; }
        catch (OperationsNotFoundException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> ChangeStatus(Guid id, bool active, CancellationToken ct)
    {
        try
        {
            if (active)
                await catalog.ReactivateStationAsync(id, ct);
            else
                await catalog.DeactivateStationAsync(id, ct);
            TempData["Success"] = active ? "Đã kích hoạt lại bến xe." : "Đã ngừng hoạt động bến xe.";
        }
        catch (CatalogConflictException ex) { TempData["Error"] = ex.Message; }
        catch (CatalogNotFoundException) { return NotFound(); }
        return RedirectToAction(nameof(Index));
    }
}
