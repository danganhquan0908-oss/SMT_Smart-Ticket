using System.ComponentModel.DataAnnotations;
using SmartTicket.Domain.Entities;

namespace SmartTicket.Application.Catalog;

public sealed record StationDto(
    Guid Id,
    string Code,
    string Name,
    string Province,
    string Address,
    bool IsActive);

public sealed record BusRouteDto(
    Guid Id,
    Guid DepartureStationId,
    string DepartureStation,
    Guid ArrivalStationId,
    string ArrivalStation,
    decimal DistanceKm,
    int EstimatedHours,
    bool IsActive);

public sealed class SaveStationRequest
{
    [Required(ErrorMessage = "Vui lòng nhập mã bến xe.")]
    [StringLength(12, MinimumLength = 2, ErrorMessage = "Mã bến xe phải từ 2 đến 12 ký tự.")]
    public string Code { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tên bến xe.")]
    [StringLength(160, ErrorMessage = "Tên bến xe không được vượt quá 160 ký tự.")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập Tỉnh / Thành phố.")]
    [StringLength(100, ErrorMessage = "Tên Tỉnh/Thành phố không được vượt quá 100 ký tự.")]
    public string Province { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ chi tiết của bến xe.")]
    [StringLength(300, ErrorMessage = "Địa chỉ không được vượt quá 300 ký tự.")]
    public string Address { get; init; } = string.Empty;
}

public sealed class SaveBusRouteRequest
{
    [Required(ErrorMessage = "Vui lòng chọn bến khởi hành (Bến đi).")]
    public Guid DepartureStationId { get; init; }

    [Required(ErrorMessage = "Vui lòng chọn bến kết thúc (Bến đến).")]
    public Guid ArrivalStationId { get; init; }

    [Range(typeof(decimal), "0.1", "99999", ErrorMessage = "Khoảng cách phải từ 0.1 đến 99,999 km.")]
    public decimal DistanceKm { get; init; }

    [Range(1, 100, ErrorMessage = "Thời lượng di chuyển dự kiến phải từ 1 đến 100 giờ.")]
    public int EstimatedHours { get; init; }
}

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

public interface ICatalogCache
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken);
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken);
    Task RemoveAsync(string key, CancellationToken cancellationToken);
}

public sealed class CatalogNotFoundException(string message) : Exception(message);
public sealed class CatalogConflictException(string message) : Exception(message);
