using BusGoSmartTicket.Exceptions.Catalog;
using BusGoSmartTicket.Models.DTOs.Catalog;
using BusGoSmartTicket.Models.Entities;
using BusGoSmartTicket.Services.Interfaces.Catalog;

namespace BusGoSmartTicket.Services.Catalog;

public sealed class CatalogService(ICatalogRepository repository, ICatalogCache cache) : ICatalogService
{
    private static readonly TimeSpan CatalogTtl = TimeSpan.FromHours(24);
    private const string StationsAllKey = "catalog:stations:all:v1";
    private const string StationsActiveKey = "catalog:stations:active:v1";
    private const string RoutesAllKey = "catalog:routes:all:v1";
    private const string RoutesActiveKey = "catalog:routes:active:v1";

    public async Task<IReadOnlyList<StationDto>> GetStationsAsync(bool includeInactive, CancellationToken ct)
    {
        var key = includeInactive ? StationsAllKey : StationsActiveKey;
        var cached = await cache.GetAsync<IReadOnlyList<StationDto>>(key, ct);
        if (cached is not null)
            return cached;

        var stations = (await repository.GetStationsAsync(includeInactive, ct)).Select(ToDto).ToList();
        await cache.SetAsync(key, stations, CatalogTtl, ct);
        return stations;
    }

    public async Task<StationDto> CreateStationAsync(SaveStationRequest request, CancellationToken ct)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        if (await repository.StationCodeExistsAsync(code, null, ct))
            throw new CatalogConflictException("Mã bến xe đã tồn tại.");
        var station = new Station(code, request.Name, request.Province, request.Address);
        repository.Add(station);
        await repository.SaveChangesAsync(ct);
        await InvalidateAsync(ct);
        return ToDto(station);
    }

    public async Task<StationDto> UpdateStationAsync(Guid id, SaveStationRequest request, CancellationToken ct)
    {
        var station = await repository.FindStationAsync(id, ct) ?? throw new CatalogNotFoundException("Không tìm thấy bến xe.");
        var code = request.Code.Trim().ToUpperInvariant();
        if (await repository.StationCodeExistsAsync(code, id, ct))
            throw new CatalogConflictException("Mã bến xe đã được sử dụng.");
        station.Update(code, request.Name, request.Province, request.Address);
        await repository.SaveChangesAsync(ct);
        await InvalidateAsync(ct);
        return ToDto(station);
    }

    public async Task DeactivateStationAsync(Guid id, CancellationToken ct)
    {
        var station = await repository.FindStationAsync(id, ct) ?? throw new CatalogNotFoundException("Không tìm thấy bến xe.");
        if (await repository.HasActiveTripsForStationAsync(id, ct))
            throw new CatalogConflictException("Không thể ngừng hoạt động bến xe đang có chuyến ở trạng thái SCHEDULED hoặc RUNNING.");
        station.Deactivate();
        await repository.SaveChangesAsync(ct);
        await InvalidateAsync(ct);
    }

    public async Task ReactivateStationAsync(Guid id, CancellationToken ct)
    {
        var station = await repository.FindStationAsync(id, ct) ?? throw new CatalogNotFoundException("Không tìm thấy bến xe.");
        station.Reactivate();
        await repository.SaveChangesAsync(ct);
        await InvalidateAsync(ct);
    }

    public async Task<IReadOnlyList<BusRouteDto>> GetRoutesAsync(bool includeInactive, CancellationToken ct)
    {
        var key = includeInactive ? RoutesAllKey : RoutesActiveKey;
        var cached = await cache.GetAsync<IReadOnlyList<BusRouteDto>>(key, ct);
        if (cached is not null)
            return cached;

        var routes = (await repository.GetRoutesAsync(includeInactive, ct)).Select(ToDto).ToList();
        await cache.SetAsync(key, routes, CatalogTtl, ct);
        return routes;
    }

    public async Task<BusRouteDto> CreateRouteAsync(SaveBusRouteRequest request, CancellationToken ct)
    {
        await ValidateRouteAsync(request.DepartureStationId, request.ArrivalStationId, null, ct);
        var route = new BusRoute(request.DepartureStationId, request.ArrivalStationId, request.DistanceKm, request.EstimatedHours);
        repository.Add(route);
        await repository.SaveChangesAsync(ct);
        await InvalidateAsync(ct);
        var saved = await repository.FindRouteAsync(route.Id, ct) ?? throw new InvalidOperationException("Không đọc lại được tuyến vừa tạo.");
        return ToDto(saved);
    }

    public async Task<BusRouteDto> UpdateRouteAsync(Guid id, SaveBusRouteRequest request, CancellationToken ct)
    {
        var route = await repository.FindRouteAsync(id, ct) ?? throw new CatalogNotFoundException("Không tìm thấy tuyến đường.");
        await ValidateRouteAsync(request.DepartureStationId, request.ArrivalStationId, id, ct);
        route.Update(request.DepartureStationId, request.ArrivalStationId, request.DistanceKm, request.EstimatedHours);
        await repository.SaveChangesAsync(ct);
        await InvalidateAsync(ct);
        var saved = await repository.FindRouteAsync(route.Id, ct) ?? throw new InvalidOperationException("Không đọc lại được tuyến vừa cập nhật.");
        return ToDto(saved);
    }

    public async Task DeactivateRouteAsync(Guid id, CancellationToken ct)
    {
        var route = await repository.FindRouteAsync(id, ct) ?? throw new CatalogNotFoundException("Không tìm thấy tuyến đường.");
        if (await repository.HasActiveTripsForRouteAsync(id, ct))
            throw new CatalogConflictException("Không thể ngừng hoạt động tuyến đường đang có chuyến SCHEDULED hoặc RUNNING.");
        route.Deactivate();
        await repository.SaveChangesAsync(ct);
        await InvalidateAsync(ct);
    }

    public async Task ReactivateRouteAsync(Guid id, CancellationToken ct)
    {
        var route = await repository.FindRouteAsync(id, ct) ?? throw new CatalogNotFoundException("Không tìm thấy tuyến đường.");
        await EnsureStationsActiveAsync(route.DepartureStationId, route.ArrivalStationId, ct);
        route.Reactivate();
        await repository.SaveChangesAsync(ct);
        await InvalidateAsync(ct);
    }

    private async Task ValidateRouteAsync(Guid departureId, Guid arrivalId, Guid? exceptId, CancellationToken ct)
    {
        await EnsureStationsActiveAsync(departureId, arrivalId, ct);
        if (await repository.RouteExistsAsync(departureId, arrivalId, exceptId, ct))
            throw new CatalogConflictException("Tuyến đường giữa hai bến này đã tồn tại.");
    }

    private async Task EnsureStationsActiveAsync(Guid departureId, Guid arrivalId, CancellationToken ct)
    {
        if (departureId == Guid.Empty || arrivalId == Guid.Empty || departureId == arrivalId ||
            !await repository.ActiveStationsExistAsync(departureId, arrivalId, ct))
            throw new CatalogConflictException("Bến đi và bến đến phải khác nhau, tồn tại và đang hoạt động.");
    }

    private async Task InvalidateAsync(CancellationToken ct)
    {
        foreach (var key in new[] { StationsAllKey, StationsActiveKey, RoutesAllKey, RoutesActiveKey })
            await cache.RemoveAsync(key, ct);
    }

    private static StationDto ToDto(Station x) => new(x.Id, x.Code, x.Name, x.Province, x.Address, x.IsActive);
    private static BusRouteDto ToDto(BusRoute x) => new(x.Id, x.DepartureStationId, x.DepartureStation?.Name ?? "(Bến đã ngừng hoạt động)",
        x.ArrivalStationId, x.ArrivalStation?.Name ?? "(Bến đã ngừng hoạt động)", x.DistanceKm, x.EstimatedHours, x.IsActive);
}
