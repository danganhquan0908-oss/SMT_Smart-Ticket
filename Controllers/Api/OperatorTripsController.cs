using System.Security.Claims;
using BusGoSmartTicket.Models.DTOs.Operators;
using BusGoSmartTicket.Services.Interfaces.Operators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusGoSmartTicket.Controllers.Api;

[ApiController]
[Authorize(Roles = "OPERATOR_OWNER,OPERATOR_DISPATCHER")]
[Route("api/trips")]
public sealed class OperatorTripsController(IOperatorService operators) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await operators.GetTripsAsync(CurrentUserId(), ct));

    [HttpPost]
    public async Task<IActionResult> Create(TripRequest request, CancellationToken ct)
    {
        var result = await operators.CreateTripAsync(CurrentUserId(), request, ct);
        return Created("/api/trips", result);
    }

    [HttpPut("{id:guid}/schedule")]
    public async Task<IActionResult> UpdateSchedule(Guid id, TripScheduleRequest request, CancellationToken ct)
    {
        await operators.UpdateTripScheduleAsync(CurrentUserId(), id, request, ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/price")]
    public async Task<IActionResult> ChangePrice(Guid id, PriceChangeRequest request, CancellationToken ct)
    {
        await operators.ChangeTripPriceAsync(CurrentUserId(), id, request.NewPrice, ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/price-history")]
    public async Task<IActionResult> PriceHistory(Guid id, CancellationToken ct) => Ok(await operators.GetPriceHistoryAsync(CurrentUserId(), id, ct));

    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
