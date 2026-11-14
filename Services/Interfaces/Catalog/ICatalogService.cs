using BusGoSmartTicket.Models.DTOs.Catalog;

namespace BusGoSmartTicket.Services.Interfaces.Catalog;

public interface ICatalogService
{
    Task<IReadOnlyList<StationDto>> GetStationsAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<StationDto> CreateStationAsync(SaveStationRequest request, CancellationToken cancellationToken);
    Task<StationDto> UpdateStationAsync(Guid id, SaveStationRequest request, CancellationToken cancellationToken);
    Task DeactivateStationAsync(Guid id, CancellationToken cancellationToken);
    Task ReactivateStationAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<BusRouteDto>> GetRoutesAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<BusRouteDto> CreateRouteAsync(SaveBusRouteRequest request, CancellationToken cancellationToken);
    Task<BusRouteDto> UpdateRouteAsync(Guid id, SaveBusRouteRequest request, CancellationToken cancellationToken);
    Task DeactivateRouteAsync(Guid id, CancellationToken cancellationToken);
    Task ReactivateRouteAsync(Guid id, CancellationToken cancellationToken);
}
