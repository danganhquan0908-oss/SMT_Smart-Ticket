using System.ComponentModel.DataAnnotations;

namespace BusGoSmartTicket.Models.DTOs.Operators;

public sealed class TripRequest
{
    [Required(ErrorMessage = "Vui lòng chọn tuyến đường.")]
    public Guid RouteId
    {
        get; init;
    }

    [Required(ErrorMessage = "Vui lòng chọn phương tiện vận tải.")]
    public Guid VehicleId
    {
        get; init;
    }

    [Required(ErrorMessage = "Vui lòng chọn tài xế.")]
    public Guid DriverId
    {
        get; init;
    }

    [Required(ErrorMessage = "Vui lòng chọn thời gian khởi hành.")]
    public DateTime DepartureTimeUtc
    {
        get; init;
    }

    [Required(ErrorMessage = "Vui lòng chọn thời gian đến dự kiến.")]
    public DateTime ArrivalTimeUtc
    {
        get; init;
    }

    [Range(typeof(decimal), "10000", "100000000", ErrorMessage = "Giá vé niêm yết từ 10,000 đ đến 100,000,000 đ.")]
    public decimal TicketPrice
    {
        get; init;
    }
}
