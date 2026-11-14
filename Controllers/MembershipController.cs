using System.Security.Claims;
using BusGoSmartTicket.Services.Interfaces.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BusGoSmartTicket.Controllers;

[Authorize(Roles = "CUSTOMER"), Route("membership")]
public sealed class MembershipController(IMembershipService membership) : Controller
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct) => View(await membership.OverviewAsync(UserId, ct));
    [HttpPost("notifications/{id:guid}/read"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Read(Guid id, CancellationToken ct) =>
        await membership.MarkReadAsync(UserId, id, ct) ? RedirectToAction(nameof(Index)) : NotFound();
}
