using System.ComponentModel.DataAnnotations;

namespace BusGoSmartTicket.Models.DTOs.Catalog;

public sealed class SaveBusRouteRequest
{
    [Required(ErrorMessage = "Vui lòng chọn bến khởi hành (Bến đi).")]
    public Guid DepartureStationId
    {
        get; init;
    }

    [Required(ErrorMessage = "Vui lòng chọn bến kết thúc (Bến đến).")]
    public Guid ArrivalStationId
    {
        get; init;
    }

    [Range(typeof(decimal), "0.1", "99999", ErrorMessage = "Khoảng cách phải từ 0.1 đến 99,999 km.")]
    public decimal DistanceKm
    {
        get; init;
    }

    [Range(1, 100, ErrorMessage = "Thời lượng di chuyển dự kiến phải từ 1 đến 100 giờ.")]
    public int EstimatedHours
    {
        get; init;
    }
}
