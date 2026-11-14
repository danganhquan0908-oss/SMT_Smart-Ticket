using System.ComponentModel.DataAnnotations;

namespace BusGoSmartTicket.Models.DTOs.Operations;

public sealed class DispatchRequest
{
    [Required]
    public Guid TripId
    {
        get; init;
    }
    public bool VehicleDocumentsValid
    {
        get; init;
    }
    public bool DriverLicenseValid
    {
        get; init;
    }
    public bool DriverFitForDuty
    {
        get; init;
    }
    public bool PassengerCapacityValid
    {
        get; init;
    }
    [StringLength(500)]
    public string? Notes
    {
        get; init;
    }
}
