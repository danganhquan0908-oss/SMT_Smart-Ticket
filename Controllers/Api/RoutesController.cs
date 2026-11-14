using BusGoSmartTicket.Models.DTOs.Catalog;
using BusGoSmartTicket.Services.Interfaces.Catalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusGoSmartTicket.Controllers.Api;

[ApiController]
[Route("api/catalog/routes")]
public sealed class RoutesController(ICatalogService catalog) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<BusRouteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await catalog.GetRoutesAsync(false, ct));

    [HttpPost]
    [Authorize(Roles = "SUPER_ADMIN")]
    public async Task<IActionResult> Create(SaveBusRouteRequest request, CancellationToken ct)
    {
        var result = await catalog.CreateRouteAsync(request, ct);
        return Created("/api/catalog/routes", result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SUPER_ADMIN")]
    public async Task<IActionResult> Update(Guid id, SaveBusRouteRequest request, CancellationToken ct) =>
        Ok(await catalog.UpdateRouteAsync(id, request, ct));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SUPER_ADMIN")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await catalog.DeactivateRouteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/reactivate")]
    [Authorize(Roles = "SUPER_ADMIN")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
    {
        await catalog.ReactivateRouteAsync(id, ct);
        return NoContent();
    }
}
