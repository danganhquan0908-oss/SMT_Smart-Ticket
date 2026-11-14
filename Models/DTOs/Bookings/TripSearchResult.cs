namespace BusGoSmartTicket.Models.DTOs.Bookings;

public sealed record TripSearchResult(Guid TripId, Guid RouteId, string Operator, string DepartureStation, string ArrivalStation, string DepartureProvince, string ArrivalProvince, DateTime DepartureTimeUtc, DateTime ArrivalTimeUtc, decimal Price, string VehicleType, int AvailableSeats, int TotalSeats);
