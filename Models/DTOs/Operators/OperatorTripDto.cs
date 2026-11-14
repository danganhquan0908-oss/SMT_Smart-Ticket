using BusGoSmartTicket.Models.Entities;

namespace BusGoSmartTicket.Models.DTOs.Operators;

public sealed record OperatorTripDto(Guid Id, string RouteName, Guid RouteId, string PlateNumber, Guid VehicleId, string DriverName, Guid DriverId, DateTime DepartureTimeUtc, DateTime ArrivalTimeUtc, decimal TicketPrice, TripStatus Status, int SoldTicketCount, long Version);
