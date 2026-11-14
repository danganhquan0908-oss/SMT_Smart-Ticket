namespace BusGoSmartTicket.Models.DTOs.Operations;

public sealed record TripManifestDto(
    Guid TripId,
    string RouteName,
    string DepartureStation,
    string ArrivalStation,
    string OperatorName,
    string VehiclePlate,
    string DriverName,
    string DriverPhone,
    DateTime DepartureTimeUtc,
    string Status,
    int TotalCapacity,
    int SoldTicketCount,
    int CheckedInCount,
    bool HasApprovedDispatch,
    string? DispatchSignature,
    DateTime? DispatchedAtUtc,
    string? DispatchNotes,
    IReadOnlyList<TripPassengerManifestItemDto> Passengers
);
