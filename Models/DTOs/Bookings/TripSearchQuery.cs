namespace BusGoSmartTicket.Models.DTOs.Bookings;

public sealed record TripSearchQuery(Guid DepartureStationId, Guid ArrivalStationId, DateOnly TravelDate);
