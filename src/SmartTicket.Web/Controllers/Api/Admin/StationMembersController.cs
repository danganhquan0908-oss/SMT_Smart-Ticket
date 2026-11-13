using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTicket.Application.Operations;

namespace SmartTicket.Web.Controllers.Api.Admin;

[ApiController, Authorize(Roles = "SUPER_ADMIN"), Route("api/admin/stations/{stationId:guid}/members")]
public sealed class StationMembersController(IStationOperationsService operations) : ControllerBase
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(Guid stationId, StationMemberRequest request, CancellationToken ct)
    { await operations.AssignStationMemberAsync(stationId, request.Email, ct); return NoContent(); }
    [HttpDelete("{userId:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid stationId, Guid userId, CancellationToken ct)
    { await operations.DeactivateStationMemberAsync(stationId, userId, ct); return NoContent(); }
}
