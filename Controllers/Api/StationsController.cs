using BusGoSmartTicket.Models.DTOs.Catalog;
using BusGoSmartTicket.Services.Interfaces.Catalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusGoSmartTicket.Controllers.Api;

[ApiController]
[Route("api/catalog/stations")]
public sealed class StationsController(ICatalogService catalog) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<StationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await catalog.GetStationsAsync(false, ct));

    [HttpPost]
    [Authorize(Roles = "SUPER_ADMIN")]
    public async Task<IActionResult> Create(SaveStationRequest request, CancellationToken ct)
    {
        var result = await catalog.CreateStationAsync(request, ct);
        return Created("/api/catalog/stations", result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SUPER_ADMIN")]
    public async Task<IActionResult> Update(Guid id, SaveStationRequest request, CancellationToken ct) =>
        Ok(await catalog.UpdateStationAsync(id, request, ct));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SUPER_ADMIN")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await catalog.DeactivateStationAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/reactivate")]
    [Authorize(Roles = "SUPER_ADMIN")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
    {
        await catalog.ReactivateStationAsync(id, ct);
        return NoContent();
    }
}
