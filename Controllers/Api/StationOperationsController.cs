using System.Security.Claims;
using BusGoSmartTicket.Exceptions.Operations;
using BusGoSmartTicket.Models.DTOs.Operations;
using BusGoSmartTicket.Services.Interfaces.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusGoSmartTicket.Controllers.Api;

[ApiController, Route("api/operations")]
[Authorize(Roles = "STATION_AGENT,OPERATOR_OWNER,OPERATOR_DISPATCHER")]
public sealed class StationOperationsController(IStationOperationsService operations) : ControllerBase
{
    [HttpGet("trips")]
    public async Task<IActionResult> Trips(CancellationToken ct) => Ok(await operations.GetTripsAsync(UserId(), ct));
    [HttpGet("trips/{tripId:guid}/manifest")]
    public async Task<IActionResult> Manifest(Guid tripId, CancellationToken ct) => Ok(await operations.GetTripManifestAsync(UserId(), tripId, ct));

    [HttpPost("dispatch")]
    [Authorize(Roles = "STATION_AGENT")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Dispatch(DispatchRequest request, CancellationToken ct)
    {
        try
        {
            return Ok(await operations.ClearDepartureAsync(UserId(), request, ct));
        }
        catch (OperationsConflictException ex)
        {
            return Conflict(new
            {
                approved = false,
                message = ex.Message
            });
        }
        catch (OperationsNotFoundException ex)
        {
            return NotFound(new
            {
                approved = false,
                message = ex.Message
            });
        }
    }
    [HttpPost("scan")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Scan(TicketScanRequest request, CancellationToken ct)
    {
        var result = await operations.ScanTicketAsync(UserId(), request, ct);
        return result.Alarm ? StatusCode(StatusCodes.Status409Conflict, result) : Ok(result);
    }
    [HttpPost("trips/{tripId:guid}/manual-checkin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManualCheckIn(Guid tripId, ManualCheckInRequest request, CancellationToken ct)
    {
        var result = await operations.ManualCheckInSeatAsync(UserId(), tripId, request.SeatCode.Trim(), ct);
        return result.Alarm ? StatusCode(StatusCodes.Status409Conflict, result) : Ok(result);
    }
    [HttpPost("trips/{tripId:guid}/complete")]
    [Authorize(Roles = "OPERATOR_OWNER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(Guid tripId, CancellationToken ct)
    {
        await operations.CompleteTripAsync(UserId(), tripId, ct);
        return NoContent();
    }
    [HttpPut("payout-account")]
    [Authorize(Roles = "OPERATOR_OWNER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PayoutAccount(PayoutAccountRequest request, CancellationToken ct)
    {
        await operations.SetPayoutAccountAsync(UserId(), request, ct);
        return NoContent();
    }
    [HttpGet("settlements")]
    [Authorize(Roles = "OPERATOR_OWNER")]
    public async Task<IActionResult> Settlements(CancellationToken ct) => Ok(await operations.GetSettlementsAsync(UserId(), ct));
    [HttpPost("settlements/{settlementId:guid}/payout-trigger")]
    [Authorize(Roles = "OPERATOR_OWNER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TriggerPayout(Guid settlementId, CancellationToken ct)
    {
        await operations.TriggerSettlementPayoutAsync(UserId(), settlementId, ct);
        return NoContent();
    }
    private Guid UserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

public sealed class ManualCheckInRequest
{
    public string SeatCode { get; set; } = string.Empty;
}
