namespace SmartTicket.Domain.Entities;

public sealed class DispatchClearance
{
    private DispatchClearance() { }
    public DispatchClearance(Guid tripId, Guid stationId, Guid reviewedByUserId, bool vehicleDocumentsValid, bool driverLicenseValid, bool driverFitForDuty, bool passengerCapacityValid, string? notes, DateTime signedAtUtc, string signature)
    { Id = Guid.NewGuid(); TripId = tripId; StationId = stationId; ReviewedByUserId = reviewedByUserId; VehicleDocumentsValid = vehicleDocumentsValid; DriverLicenseValid = driverLicenseValid; DriverFitForDuty = driverFitForDuty; PassengerCapacityValid = passengerCapacityValid; Notes = notes?.Trim(); Signature = signature; IsApproved = vehicleDocumentsValid && driverLicenseValid && driverFitForDuty && passengerCapacityValid; CreatedAtUtc = signedAtUtc; }
    public Guid Id { get; private set; }
    public Guid TripId { get; private set; }
    public Trip? Trip { get; private set; }
    public Guid StationId { get; private set; }
    public Guid ReviewedByUserId { get; private set; }
    public bool VehicleDocumentsValid { get; private set; }
    public bool DriverLicenseValid { get; private set; }
    public bool DriverFitForDuty { get; private set; }
    public bool PassengerCapacityValid { get; private set; }
    public string? Notes { get; private set; }
    public bool IsApproved { get; private set; }
    public string Signature { get; private set; } = "";
    public DateTime CreatedAtUtc { get; private set; }
}
