namespace BusGoSmartTicket.Models.DTOs.Payments;

public sealed record TicketDto(string TicketNumber, string PassengerName, string Trip, string SeatCode, DateTime DepartureTimeUtc, decimal Price, string CheckInJwt);
