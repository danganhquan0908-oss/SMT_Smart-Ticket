using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BusGoSmartTicket.Controllers;

[AllowAnonymous, Route("journey")]
public sealed class JourneyController : Controller
{
    [HttpGet("help")] public IActionResult Help() => View();
}
