namespace BusGoSmartTicket.Models.DTOs.Payments;

public sealed record WebhookProcessResult(bool Processed, string Result, Guid? TripId = null, IReadOnlyList<string>? SeatCodes = null);
