namespace BusGoSmartTicket.Models.Entities;

public sealed class Vehicle
{
    private Vehicle()
    {
    }
    public Vehicle(Guid operatorId, Guid seatTemplateId, string plateNumber, string vehicleType)
    {
        Id = Guid.NewGuid();
        OperatorId = operatorId;
        SeatTemplateId = seatTemplateId;
        PlateNumber = plateNumber.Trim().ToUpperInvariant();
        VehicleType = vehicleType.Trim();
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }
    public Guid Id
    {
        get; private set;
    }
    public Guid OperatorId
    {
        get; private set;
    }
    public BusOperator? Operator
    {
        get; private set;
    }
    public Guid SeatTemplateId
    {
        get; private set;
    }
    public SeatTemplate? SeatTemplate
    {
        get; private set;
    }
    public string PlateNumber { get; private set; } = string.Empty;
    public string VehicleType { get; private set; } = string.Empty;
    public bool IsActive
    {
        get; private set;
    }
    public DateTime CreatedAtUtc
    {
        get; private set;
    }
    public void Update(Guid seatTemplateId, string plateNumber, string vehicleType)
    {
        SeatTemplateId = seatTemplateId;
        PlateNumber = plateNumber.Trim().ToUpperInvariant();
        VehicleType = vehicleType.Trim();
    }
    public void Deactivate() => IsActive = false;
    public void Reactivate() => IsActive = true;
}
