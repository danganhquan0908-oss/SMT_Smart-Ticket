using System.ComponentModel.DataAnnotations;

namespace BusGoSmartTicket.Models.DTOs.Operators;

public sealed class TripScheduleRequest
{
    [Required(ErrorMessage = "Vui lòng chọn tuyến đường.")]
    public Guid RouteId
    {
        get; init;
    }

    [Required(ErrorMessage = "Vui lòng chọn phương tiện.")]
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
}
