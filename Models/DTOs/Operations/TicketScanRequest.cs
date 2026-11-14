namespace BusGoSmartTicket.Models.DTOs.Operations;

public sealed record TicketScanRequest(string Jwt, bool IsOffline = false, DateTime? ScannedAtUtc = null);
