using System.ComponentModel.DataAnnotations;

namespace BusGoSmartTicket.Models.ViewModels;

public sealed class OperatorTripForm
{
    [Required]
    public Guid RouteId
    {
        get; set;
    }
    [Required]
    public Guid VehicleId
    {
        get; set;
    }
    [Required]
    public Guid DriverId
    {
        get; set;
    }
    [Required]
    public DateTime DepartureTime
    {
        get; set;
    }
    [Required]
    public DateTime ArrivalTime
    {
        get; set;
    }
    [Range(typeof(decimal), "1", "100000000")]
    public decimal TicketPrice
    {
        get; set;
    }
}
