namespace BusGoSmartTicket.Models.DTOs.Catalog;

public sealed record BusRouteDto(
    Guid Id,
    Guid DepartureStationId,
    string DepartureStation,
    Guid ArrivalStationId,
    string ArrivalStation,
    decimal DistanceKm,
    int EstimatedHours,
    bool IsActive);
