using BusGoSmartTicket.Models.Entities;

namespace BusGoSmartTicket.Services.Interfaces.Catalog;

public interface ICatalogRepository
{
    Task<IReadOnlyList<Station>> GetStationsAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<Station?> FindStationAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> StationCodeExistsAsync(string code, Guid? exceptId, CancellationToken cancellationToken);
    Task<bool> HasActiveTripsForStationAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<BusRoute>> GetRoutesAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<BusRoute?> FindRouteAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ActiveStationsExistAsync(Guid departureStationId, Guid arrivalStationId, CancellationToken cancellationToken);
    Task<bool> RouteExistsAsync(Guid departureStationId, Guid arrivalStationId, Guid? exceptId, CancellationToken cancellationToken);
    Task<bool> HasActiveTripsForRouteAsync(Guid id, CancellationToken cancellationToken);

    void Add(Station station);
    void Add(BusRoute route);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
