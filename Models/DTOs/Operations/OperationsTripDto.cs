namespace BusGoSmartTicket.Models.DTOs.Operations;

public sealed record OperationsTripDto(
    Guid Id,
    string DepartureStation,
    string ArrivalStation,
    string Operator,
    string VehiclePlate,
    string Driver,
    DateTime DepartureTimeUtc,
    string Status,
    int TicketCount,
    bool HasApprovedDispatch,
    int CheckedInCount = 0,
    int TotalCapacity = 0,
    string? DriverPhone = null,
    string? DispatchSignature = null,
    DateTime? DispatchedAtUtc = null,
    string? DispatchNotes = null
);
