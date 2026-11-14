using BusGoSmartTicket.Exceptions.Catalog;
using BusGoSmartTicket.Models.Entities;
using BusGoSmartTicket.Services.Interfaces.Catalog;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BusGoSmartTicket.Data.Repositories.Catalog;

public sealed class EfCatalogRepository(ApplicationDbContext db) : ICatalogRepository
{
    public async Task<IReadOnlyList<Station>> GetStationsAsync(bool includeInactive, CancellationToken ct) =>
        await db.Stations.IgnoreQueryFilters().Where(x => includeInactive || x.IsActive)
            .OrderBy(x => x.Province).ThenBy(x => x.Name).ToListAsync(ct);

    public Task<Station?> FindStationAsync(Guid id, CancellationToken ct) =>
        db.Stations.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == id, ct);

    public Task<bool> StationCodeExistsAsync(string code, Guid? exceptId, CancellationToken ct) =>
        db.Stations.IgnoreQueryFilters().AnyAsync(x => x.Code == code && (!exceptId.HasValue || x.Id != exceptId.Value), ct);

    public async Task<bool> HasActiveTripsForStationAsync(Guid id, CancellationToken ct)
    {
        var routeIds = db.Routes.IgnoreQueryFilters().Where(r => r.DepartureStationId == id || r.ArrivalStationId == id).Select(r => r.Id);
        return await db.Trips.AnyAsync(t => routeIds.Contains(t.RouteId) &&
            (t.Status == TripStatus.Scheduled || t.Status == TripStatus.Running), ct);
    }

    public async Task<IReadOnlyList<BusRoute>> GetRoutesAsync(bool includeInactive, CancellationToken ct) =>
        await db.Routes.IgnoreQueryFilters()
            .Where(x => includeInactive || (x.IsActive && x.DepartureStation!.IsActive && x.ArrivalStation!.IsActive))
            .Include(x => x.DepartureStation).Include(x => x.ArrivalStation)
            .OrderBy(x => x.DepartureStation!.Province).ThenBy(x => x.DepartureStation!.Name)
            .ThenBy(x => x.ArrivalStation!.Name).ToListAsync(ct);

    public Task<BusRoute?> FindRouteAsync(Guid id, CancellationToken ct) =>
        db.Routes.IgnoreQueryFilters().Include(x => x.DepartureStation).Include(x => x.ArrivalStation)
            .SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<bool> ActiveStationsExistAsync(Guid departureId, Guid arrivalId, CancellationToken ct) =>
        await db.Stations.CountAsync(s => s.IsActive && (s.Id == departureId || s.Id == arrivalId), ct) == 2;

    public Task<bool> RouteExistsAsync(Guid departureId, Guid arrivalId, Guid? exceptId, CancellationToken ct) =>
        db.Routes.IgnoreQueryFilters().AnyAsync(x => x.DepartureStationId == departureId && x.ArrivalStationId == arrivalId &&
            (!exceptId.HasValue || x.Id != exceptId.Value), ct);

    public Task<bool> HasActiveTripsForRouteAsync(Guid id, CancellationToken ct) =>
        db.Trips.AnyAsync(t => t.RouteId == id && (t.Status == TripStatus.Scheduled || t.Status == TripStatus.Running), ct);

    public void Add(Station station) => db.Stations.Add(station);
    public void Add(BusRoute route) => db.Routes.Add(route);

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new CatalogConflictException("Mã bến hoặc tuyến đường đã tồn tại.");
        }
    }
}
