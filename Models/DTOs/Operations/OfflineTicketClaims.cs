namespace BusGoSmartTicket.Models.DTOs.Operations;

public sealed record OfflineTicketClaims(Guid SeatId, Guid TripId, string SeatCode, DateTime ExpiresAtUtc);

/// <summary>Use inside a trusted POS client to verify ticket authenticity when the station has no network.</summary>
