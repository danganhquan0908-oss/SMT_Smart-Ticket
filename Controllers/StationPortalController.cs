using System.Security.Claims;
using BusGoSmartTicket.Services.Interfaces.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusGoSmartTicket.Controllers;

[Authorize(Roles = "STATION_AGENT,OPERATOR_OWNER,OPERATOR_DISPATCHER,SUPER_ADMIN"), Route("station")]
public sealed class StationPortalController(IStationOperationsService operations) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var trips = await operations.GetTripsAsync(userId, ct);
        ViewBag.IsStationAgent = User.IsInRole("STATION_AGENT");
        ViewBag.UserRole = User.IsInRole("STATION_AGENT") ? "Kiểm soát viên Bến xe" : (User.IsInRole("OPERATOR_OWNER") ? "Chủ nhà xe" : "Điều hành nhà xe");
        return View("~/Views/Station/Index.cshtml", trips);
    }
}
