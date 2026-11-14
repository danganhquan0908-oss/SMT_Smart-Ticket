namespace BusGoSmartTicket.Models.DTOs.Operations;

public sealed record TicketScanResult(string Result, bool Accepted, bool Alarm, string? SeatCode, string? Trip, DateTime? DepartureTimeUtc);
